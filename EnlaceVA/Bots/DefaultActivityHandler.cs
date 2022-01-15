using EnlaceVA.Models;
using EnlaceVA.Services;
using EnlaceVA.Utilities;
using Microsoft.Azure.Cosmos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static EnlaceVA.Utilities.Utility;

namespace EnlaceVA.Bots
{
    public class DefaultActivityHandler<T> : TeamsActivityHandler
        where T : Dialog
    {
        private readonly Dialog _dialog;
        private readonly BotState _conversationState;
        private readonly BotState _userState;
        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        private readonly IStatePropertyAccessor<UserProfileState> _userProfileState;
        private readonly LocaleTemplateManager _templateManager;
        private ITurnContext<IMessageActivity> _turnContext;
        private readonly BotSettings settings;
        private readonly CosmosClient cosmosClient;
        private readonly Database database;
        private readonly Microsoft.Azure.Cosmos.Container container;

        public DefaultActivityHandler(IServiceProvider serviceProvider, T dialog)
        {
            _dialog = dialog;
            _dialog.TelemetryClient = serviceProvider.GetService<IBotTelemetryClient>();
            _conversationState = serviceProvider.GetService<ConversationState>();
            _userState = serviceProvider.GetService<UserState>();
            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            _userProfileState = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            _templateManager = serviceProvider.GetService<LocaleTemplateManager>();
            settings = serviceProvider.GetService<BotSettings>();
            this.cosmosClient = new CosmosClient(settings.CosmosDb.CosmosDbEndpoint, settings.CosmosDb.AuthKey, null);
            this.database = this.cosmosClient.GetDatabase(settings.CosmosDb.DatabaseId);
            this.container = this.database.GetContainer(settings.CosmosDb.ContainerId);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, true, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, true, cancellationToken);

        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var userProfile = await _userProfileState.GetAsync(turnContext, () => new UserProfileState(), cancellationToken);
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                if (turnContext != null)
                {
                    Activity activity = (Activity)turnContext.Activity;                    
                    (var companyName, var channelName) = ValidateCompany(activity, userProfile);                    
                    userProfile.DidBotWelcomeUser = true;
                    userProfile.Company = companyName.ToUpper(CultureInfo.CurrentCulture);
                    userProfile.ChannelId = channelName;
                    userProfile.CompanyName = Utility.GetCompanyName(companyName);
                    userProfile.ContactMessage = Utility.GetContactInfo(companyName);
                    await _userState.SaveChangesAsync(turnContext, true);

                    var displayMessage = "";

                    if (companyName.ToLower().Equals(MenuSubMenuEnum.CompanyName.Gdo))
                    {
                        displayMessage = "NewUserMessageGDO";
                    }
                    else
                    {
                        displayMessage = "NewUserMessage";
                    }

                    Activity message = _templateManager.GenerateActivityForLocale(displayMessage, userProfile);

                    //message.Attachments = new List<Attachment> {
                    //new Attachment(){
                    //        ContentUrl = "https://vaenlacepromdev-bot.azurewebsites.net/ImagenContratoCEO.png",
                    //        ContentType = "image/jpg",
                    //        Name = "datauri" }};
                    await turnContext.SendActivityAsync(message, cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync(_templateManager.GenerateActivityForLocale("ReturningUserIntroCard", userProfile), cancellationToken);
            }


            await _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            const int delayTime = 300000;
            var time = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
            var userProfile = await _userProfileState.GetAsync(turnContext, () => new UserProfileState());
            var delay = userProfile.Delay.ToString(CultureInfo.CurrentCulture);
            userProfile.TaskState = "Inicia " + time + " Delay " + delay;

            _turnContext = turnContext;

            userProfile.lastUpdate = DateTime.UtcNow;
            userProfile.UserState = Utilities.MenuSubMenuEnum.userState.active;

            if (!userProfile.WorkStarted && !userProfile.TerminateWork)
            {
                userProfile.WorkStarted = true;
                userProfile.Delay = delayTime;
                await Task.Run(() => InactivityChecker());
            }
            await _userProfileState.SetAsync(turnContext, userProfile);
            await _userProfileState.SetAsync(_turnContext, userProfile);
            await _userState.SaveChangesAsync(_turnContext, true);
            await _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);
        }

        protected override Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            return _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {

            var ev = turnContext.Activity.AsEventActivity();
            switch (ev.Name)
            {
                case TokenEvents.TokenResponseEventName:
                    {
                        await _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);
                        break;
                    }

                default:
                    {
                        await turnContext.SendActivityAsync(new Activity(
                            type: ActivityTypes.Trace, text: $"Unknown Event '{ev.Name ?? "undefined"}' was received but not processed."),
                            cancellationToken);
                        break;
                    }
            }
        }

        protected override async Task OnEndOfConversationActivityAsync(ITurnContext<IEndOfConversationActivity> turnContext, CancellationToken cancellationToken)
        {
            await _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);
        }

        private async void InactivityChecker()
        {
            var retry = true;

            while (retry)
            {
                var userProfile = await _userProfileState.GetAsync(_turnContext, () => new UserProfileState());
                bool restart = false;
                const int minutos = 5;
                await Task.Delay(userProfile.Delay);
                try
                {
                    string formatDate = "yyyy-MM-dd hh:mm";
                    var horaActual = DateTime.ParseExact(DateTime.UtcNow.ToString(formatDate, CultureInfo.CurrentCulture), formatDate, CultureInfo.InvariantCulture);

                    var sqlQueryText = GetCosmosData();

                    QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                    FeedIterator<UserProfileState> queryResultSetIterator = this.container.GetItemQueryIterator<UserProfileState>(queryDefinition);
                    List<UserProfileState> usersProfiles = new List<UserProfileState>();

                    while (queryResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<UserProfileState> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (UserProfileState userprofile in currentResultSet)
                        {
                            usersProfiles.Add(userprofile);
                        }
                    }
                    var timeNow = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
                    userProfile = usersProfiles.Last();
                    userProfile.TaskState = "Paso thread join " + timeNow;
                    userProfile.lastUpdate = DateTime.ParseExact(userProfile.lastUpdate.ToString("yyyy-MM-dd hh:mm"), "yyyy-MM-dd hh:mm", CultureInfo.InvariantCulture);
                    userProfile.WorkStarted = true;
                    if (userProfile.Conversations != null)
                    {
                        if (userProfile.TerminateWork || userProfile.Conversations.Last().Questions == null)
                        {
                            userProfile.TerminateWork = false;
                            userProfile.WorkStarted = false;
                        }
                        else if (userProfile.lastUpdate.AddMinutes(minutos) <= horaActual)
                        {

                            var messageSend = await SendMessageAsync(userProfile);
                            if (messageSend)
                            {
                                userProfile.UserState = Utilities.MenuSubMenuEnum.userState.inactive;
                                userProfile.TerminateWork = true;
                                restart = false;
                            }
                        }
                        else if (userProfile.lastUpdate.AddMinutes(minutos) > horaActual)
                        {
                            TimeSpan cv = userProfile.lastUpdate.AddMinutes(minutos).Subtract(horaActual);
                            var milliseconds = cv.TotalMilliseconds.ToString();
                            userProfile.Delay = int.Parse(milliseconds, CultureInfo.CurrentCulture);
                            userProfile.UserState = Utilities.MenuSubMenuEnum.userState.active;
                            userProfile.WorkStarted = true;
                            restart = true;
                        }
                    }
                    else
                    {
                        userProfile.TerminateWork = false;
                        userProfile.WorkStarted = false;
                    }
                }
                catch (Exception ex)
                {
                    userProfile.TaskState = string.Format(CultureInfo.CurrentCulture, "ERROR EN VALIDACIÓN POR INACTIVIDAD {0}{1}{2}{3}{4}", ex.Message, Environment.NewLine,
                        ex.Source ?? "Null Source", Environment.NewLine,
                        ex.StackTrace ?? "Null Source");
                }
                await _userProfileState.SetAsync(_turnContext, userProfile);
                await _userState.SaveChangesAsync(_turnContext, true);

                if (!restart)
                {
                    retry = false;
                }

            }

        }

        public string GetCosmosData()
        {
            var sqlQueryText = String.Format(CultureInfo.CurrentCulture, "SELECT c.document.UserProfileState.Name, c.document.UserProfileState.DidBotWelcomeUser," +
                           "c.document.UserProfileState.NumberMessage, c.document.UserProfileState.Conversations, c.document.UserProfileState.Minutas," +
                           "c.document.UserProfileState.lastUpdate,c.document.UserProfileState.UserState, c.document.UserProfileState.WorkStarted, " +
                           "c.document.UserProfileState.TerminateWork, c.document.UserProfileState.isMinuta, c.document.UserProfileState.Options," +
                           "c.document.UserProfileState.lastOptions , c.document.UserProfileState.TaskState, c.document.UserProfileState.ConversationStarted," +
                           "c.document.UserProfileState.conversationId, c.document.UserProfileState.Dialogturn,c.document.UserProfileState.Company, " +
                           " c.document.UserProfileState.MenuStarted, c.document.UserProfileState.SubMenuStarted, c.document.UserProfileState.SubMenuFind," +
                           "c.document.UserProfileState.SubMenustep, c.document.UserProfileState.MenuFinished, c.document.UserProfileState.ChannelId " +
                           "FROM c Where c.realId = '{0}/users/{1}'", _turnContext.Activity.ChannelId, _turnContext.Activity.From.Id);

            return sqlQueryText;
        }

        public async Task<bool> SendMessageAsync(UserProfileState userProfile)
        {
            const string endPoint = "https://vaenlacepromdev-function.azurewebsites.net/api/";

            if (userProfile != null)
            {
                if (userProfile.ChannelId == Channels.Directline)
                {
                    TwilioServices twilioServices = new TwilioServices(endPoint);
                    _turnContext.Activity.Text = _templateManager.GenerateActivityForLocale("InactiveTimeMessage").Text;
                    await twilioServices.Post("SendWhatsappMessage?code=dFB6eY0POb/lj2gWJaiqnHWv73m9dzmpbG5c7ggBvAU1AzjQdPM31w==", _turnContext.Activity);
                }
                else
                {
                    var connector = new ConnectorClient(new Uri(_turnContext.Activity.ServiceUrl), settings.MicrosoftAppId, settings.MicrosoftAppPassword);
                    IMessageActivity message = Activity.CreateMessageActivity();
                    message.From = _turnContext.Activity.Recipient;
                    message.Recipient = _turnContext.Activity.From;
                    message.Conversation = _turnContext.Activity.Conversation;
                    message.ReplyToId = _turnContext.Activity.Id;
                    message.Text = _templateManager.GenerateActivityForLocale("InactiveTimeMessage").Text;
                    message.Locale = "es-es";
                    await connector.Conversations.ReplyToActivityAsync((Activity)message);
                }
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}