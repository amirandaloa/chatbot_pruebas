using EnlaceVA.Models;
using EnlaceVA.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using EnlaceVA.Controllers;
using Newtonsoft.Json;
using Microsoft.Bot.Connector;
using System.Globalization;

namespace EnlaceVA.Dialogs
{
    public class FeedBack : ComponentDialog
    {        
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;        

        public FeedBack(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(FeedBack))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();            
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));                                    

            var FeedBackDialog = new WaterfallStep[]
            {
                StarDialogAsync,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(FeedBackDialog), FeedBackDialog));            
        }
        

        public async Task<DialogTurnResult> StarDialogAsync (WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {         
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = MessagesFeedBack.Dialog;
            await _userState.SaveChangesAsync(stepContext.Context, true);

            if (userProfile.ChannelId == Channels.Directline)
            {                
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("FeedBackMessage"), cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("UseFeedBackcard"), cancellationToken: cancellationToken);
            }           
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
       
     
        public async Task<DialogTurnResult> FinishDialogAsync (WaterfallStepContext stepContext,CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("FeedBackReceived"), cancellationToken: cancellationToken);
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = null;
            userProfile.TerminateWork = true;
            userProfile.Conversations.Last().AnalyticScore = await TextAnaliticsController.RunAsync(userProfile.Conversations.Last().Questions);
            await _userState.SaveChangesAsync(stepContext.Context, true);
            return await stepContext.ContinueDialogAsync();            
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var userProfile = await _accessor.GetAsync(innerDc.Context, () => new UserProfileState());
            userProfile.Dialogturn = MessagesFeedBack.Dialog;
            await _userState.SaveChangesAsync(innerDc.Context, true);

            try
            {
                int ScoreMessage = 0;
                if (innerDc.Context.Activity.Text != null)
                {
                    ScoreMessage = Int32.Parse(innerDc.Context.Activity.Text);
                }

                string channel = userProfile.ChannelId;               
                var validationChannel = ValidateChannel(channel,ScoreMessage,innerDc.Context.Activity.Value);
                                                
                switch (validationChannel)
                {
                    case MessagesFeedBack.ContinueMessage:
                        userProfile.Conversations.Last().ConversationScore = ScoreMessage;
                        return await innerDc.ContinueDialogAsync();                                            
                    case MessagesFeedBack.ContinueCard:
                        Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(innerDc.Context.Activity.Value.ToString());
                        var button = Int32.Parse(response["boton"]);
                        userProfile.Conversations.Last().ConversationScore = button;
                        return await innerDc.ContinueDialogAsync();                                            
                    default:
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("FinishedMessageRetry"), cancellationToken: cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);                        
                }                      
            }catch (FormatException)
            {
                await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("FinishedMessageRetry"), cancellationToken: cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }            
        }

        public static string ValidateChannel (string channel, int score, object valuecard)
        {
            channel = channel.ToLower(CultureInfo.CurrentCulture);

            string Message;
            if (channel == Channels.Directline)
            {
                Message = ValidatescoreDirectline(score);
                return Message;
            }
            else
            {
                if (valuecard == null)
                {
                    Message = ValidatescoreDirectline(score);
                    return Message;
                }
                else
                {
                    Message = Validatescore (valuecard);
                    return Message;
                }
                
            }           
        }

        public static string ValidatescoreDirectline (int score)
        {
            string Message;
            if (score > 0 && score < 11)
            {
                Message = MessagesFeedBack.ContinueMessage;
                return Message;
            }
            else
            {
                Message = MessagesFeedBack.Retry;
                return Message;
            }              
        }

        public static string Validatescore (object score)
        {
            Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(score.ToString());
            var button = Int32.Parse(response["boton"]);
            string Message;
            if (button > 0 && button < 11)
            {
                Message = MessagesFeedBack.ContinueCard;
                return Message;
            }
            else
            {
                Message = MessagesFeedBack.Retry;
                return Message;
            }
        }

        public class MessagesFeedBack
        {
            public const string Dialog = "FeedBackDialog";
            public const string ContinueMessage = "ContinueMessage";
            public const string ContinueCard = "ContinueCard";
            public const string Retry = "Retry";

            protected MessagesFeedBack()
            {

            }
        };
    }
}
