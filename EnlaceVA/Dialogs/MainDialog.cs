using EnlaceVA.Models;
using EnlaceVA.Services;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions;
using Microsoft.Bot.Solutions.Extensions;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Bot.Solutions.Skills.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnlaceVA.Utilities;
using static EnlaceVA.Utilities.Utility;
using EnlaceVA.DialogsCards;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Text.RegularExpressions;

namespace EnlaceVA.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        public static readonly string ActiveSkillPropertyName = $"{typeof(MainDialog).FullName}.ActiveSkillProperty";
        private const string FaqDialogId = "Faq";

        private readonly LocaleTemplateManager _templateManager;
        private readonly BotServices _services;
        private readonly SwitchSkillDialog _switchSkillDialog;
        private readonly SkillsConfiguration _skillsConfig;
        private readonly IStatePropertyAccessor<UserProfileState> _userProfileState;
        private readonly IStatePropertyAccessor<List<Activity>> _previousResponseAccessor;
        private readonly IStatePropertyAccessor<BotFrameworkSkill> _activeSkillProperty;
        private readonly FeedBack _feedBack;
        private readonly TratamientoDialog _tratamientoDialog;
        private readonly TrasladoDeudaDialog _trasladoDeudaDialog;
        private readonly ExencionContribucionDialog _exencionContribucionDialog;
        private readonly CambioNombreDialog _cambioNombreDialog;
        private readonly TerminacionContratoDialog _terminacionContratoDialog;
        private readonly EmergenciasDialog _emergenciasDialog;
        private readonly ConstaciaPagoDialog _constaciaPagoDialog;
        private readonly ConceptoFacturaDialog _conceptoFacturaDialog;
        private readonly EstadoCuentaDialog _estadoCuentaDialog;
        private readonly TomaLecturaDialog _tomaLecturaDialog;
        private readonly ConsultaPQRdialog _consultaPQRdialog;
        private readonly ConceptoFacturaDialogCard _conceptoFacturaCard;
        private readonly ConsumoFacturadoDialog _consumoFacturadoDialog;
        private readonly ConsumoFacturadoDialogCard _consumoFacturadoCard;
        private readonly PagoParcialDialog _pagoParcialDialog;
        private readonly ConsultaPQRDialogCard _consultaPQRDialogCard;
        private readonly TomalecturaDialogCard _tomalecturaDialogCard;
        private readonly EstadoCuentaDialogCard _estadoCuentaDialogCard;
        private readonly PagoParcialDialogCard _pagoParcialDialogCard;
        private readonly VentaSegurosBrillaDialog _ventaSegurosBrillaDialog;
        private readonly VentaSegurosBrillaDialogCard _ventaSegurosBrillaDialogCard;
        private readonly VisitaFinanciacionDialog _visitaFinanciacionDialog;
        private readonly VisitaFinanciacionDialogCard _visitaFinanciacionDialogCard;
        private readonly ReconexionPorPagoDialog _reconexionPorPago;
        private readonly ReconexionPorPagoDialogCard _reconexionPorPagoCard;
        private readonly ConsultaCupoBrillaDialog _consultaCupoBrillaDialog;
        private readonly ConsultaCupoBrillaDialogCard _consultaCupoBrillaDialogCard;

        public IServiceProvider _serviceProvider;


        public string Channel { get; set; }

        private string companyId;
        public MainDialog(
            IServiceProvider serviceProvider)
            : base(nameof(MainDialog))
        {
            _services = serviceProvider.GetService<BotServices>();
            _templateManager = serviceProvider.GetService<LocaleTemplateManager>();
            _skillsConfig = serviceProvider.GetService<SkillsConfiguration>();

            var userState = serviceProvider.GetService<UserState>();
            _userProfileState = userState.CreateProperty<UserProfileState>(nameof(UserProfileState));

            var conversationState = serviceProvider.GetService<ConversationState>();
            _previousResponseAccessor = conversationState.CreateProperty<List<Activity>>(StateProperties.PreviousBotResponse);

            _activeSkillProperty = conversationState.CreateProperty<BotFrameworkSkill>(ActiveSkillPropertyName);

            _serviceProvider = serviceProvider;

            var steps = new WaterfallStep[]
            {
                IntroStepAsync,
                RouteStepAsync,
                FinalStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(MainDialog), steps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(TextPrompt)));
            InitialDialogId = nameof(MainDialog);

            _switchSkillDialog = serviceProvider.GetService<SwitchSkillDialog>();
            _feedBack = serviceProvider.GetService<FeedBack>();
            _tratamientoDialog = serviceProvider.GetService<TratamientoDialog>();
            _trasladoDeudaDialog = serviceProvider.GetService<TrasladoDeudaDialog>();
            _exencionContribucionDialog = serviceProvider.GetService<ExencionContribucionDialog>();
            _cambioNombreDialog = serviceProvider.GetService<CambioNombreDialog>();
            _terminacionContratoDialog = serviceProvider.GetService<TerminacionContratoDialog>();
            _emergenciasDialog = serviceProvider.GetService<EmergenciasDialog>();
            _constaciaPagoDialog = serviceProvider.GetService<ConstaciaPagoDialog>();
            _conceptoFacturaDialog = serviceProvider.GetService<ConceptoFacturaDialog>();
            _estadoCuentaDialog = serviceProvider.GetService<EstadoCuentaDialog>();
            _tomaLecturaDialog = serviceProvider.GetService<TomaLecturaDialog>();
            _consultaPQRdialog = serviceProvider.GetService<ConsultaPQRdialog>();
            _pagoParcialDialog = serviceProvider.GetService<PagoParcialDialog>();
            _consumoFacturadoDialog = serviceProvider.GetService<ConsumoFacturadoDialog>();
            _consumoFacturadoCard = serviceProvider.GetService<ConsumoFacturadoDialogCard>();
            _conceptoFacturaCard = serviceProvider.GetService<ConceptoFacturaDialogCard>();
            _consultaPQRDialogCard = serviceProvider.GetService<ConsultaPQRDialogCard>();
            _tomalecturaDialogCard = serviceProvider.GetService<TomalecturaDialogCard>();
            _estadoCuentaDialogCard = serviceProvider.GetService<EstadoCuentaDialogCard>();
            _pagoParcialDialogCard = serviceProvider.GetService<PagoParcialDialogCard>();
            _ventaSegurosBrillaDialog = serviceProvider.GetService<VentaSegurosBrillaDialog>();
            _ventaSegurosBrillaDialogCard = serviceProvider.GetService<VentaSegurosBrillaDialogCard>();
            _visitaFinanciacionDialog = serviceProvider.GetService<VisitaFinanciacionDialog>();
            _visitaFinanciacionDialogCard = serviceProvider.GetService<VisitaFinanciacionDialogCard>();
            _reconexionPorPago = serviceProvider.GetService<ReconexionPorPagoDialog>();
            _reconexionPorPagoCard = serviceProvider.GetService<ReconexionPorPagoDialogCard>();
            _consultaCupoBrillaDialog = serviceProvider.GetService<ConsultaCupoBrillaDialog>();
            _consultaCupoBrillaDialogCard = serviceProvider.GetService<ConsultaCupoBrillaDialogCard>();

            AddDialog(_switchSkillDialog);
            AddDialog(_feedBack);
            AddDialog(_tratamientoDialog);
            AddDialog(_trasladoDeudaDialog);
            AddDialog(_exencionContribucionDialog);
            AddDialog(_cambioNombreDialog);
            AddDialog(_terminacionContratoDialog);
            AddDialog(_emergenciasDialog);
            AddDialog(_constaciaPagoDialog);
            AddDialog(_conceptoFacturaDialog);
            AddDialog(_estadoCuentaDialog);
            AddDialog(_tomaLecturaDialog);
            AddDialog(_consultaPQRdialog);
            AddDialog(_pagoParcialDialog);
            AddDialog(_consumoFacturadoDialog);
            AddDialog(_consumoFacturadoCard);
            AddDialog(_conceptoFacturaCard);
            AddDialog(_consultaPQRDialogCard);
            AddDialog(_tomalecturaDialogCard);
            AddDialog(_estadoCuentaDialogCard);
            AddDialog(_pagoParcialDialogCard);
            AddDialog(_ventaSegurosBrillaDialog);
            AddDialog(_ventaSegurosBrillaDialogCard);
            AddDialog(_visitaFinanciacionDialog);
            AddDialog(_visitaFinanciacionDialogCard);
            AddDialog(_reconexionPorPago);
            AddDialog(_reconexionPorPagoCard);
            AddDialog(_consultaCupoBrillaDialog);
            AddDialog(_consultaCupoBrillaDialogCard);

            var skillDialogs = serviceProvider.GetServices<SkillDialog>();
            foreach (var dialog in skillDialogs)
            {
                AddDialog(dialog);
            }
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default)
        {
            var activity = innerDc.Context.Activity;

            if (activity.Type == ActivityTypes.Message && !string.IsNullOrEmpty(activity.Text))
            {
                var localizedServices = _services.GetCognitiveModels();

                var dispatchResult = await localizedServices.DispatchService.RecognizeAsync<DispatchLuis>(innerDc.Context, cancellationToken);
                innerDc.Context.TurnState.Add(StateProperties.DispatchResult, dispatchResult);

                if (dispatchResult.TopIntent().intent == DispatchLuis.Intent.l_General)
                {
                    var generalResult = await localizedServices.LuisServices["General"].RecognizeAsync<GeneralLuis>(innerDc.Context, cancellationToken);
                    innerDc.Context.TurnState.Add(StateProperties.GeneralResult, generalResult);
                }
            }

            innerDc.Context.OnSendActivities(StoreOutgoingActivitiesAsync);
            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var userProfile = await _userProfileState.GetAsync(innerDc.Context, () => new UserProfileState());

            if (userProfile.WorkStarted && userProfile.TerminateWork)
            {
                await _userProfileState.SetAsync(innerDc.Context, userProfile);
                return await innerDc.ReplaceDialogAsync(InitialDialogId);
            }
            var localizedServices = _services.GetCognitiveModels();

            if (!userProfile.MenuStarted)
            {

                if (userProfile.Dialogturn == null)
                {
                    if (userProfile.groupQuestion)
                    {

                        var dispatchResult = await localizedServices.DispatchService.RecognizeAsync<DispatchLuis>(innerDc.Context, cancellationToken);

                        Metadata[] MetadataFilter = null;
                        MetadataFilter = new Metadata[]
                                   {
                                        new Metadata()
                                        {
                                            Name = "kb",
                                            Value = userProfile.Company.ToLower()
                                        }
                        };

                        switch (dispatchResult.TopIntent().intent)
                        {
                            case DispatchLuis.Intent.l_General:
                                break;
                            case DispatchLuis.Intent.q_Faq:
                                var QnaService_ = _services.GetCognitiveModels().QnAConfiguration["Faq"];
                                var SampleQnAFac = new QnAMaker(new QnAMakerEndpoint
                                {
                                    KnowledgeBaseId = QnaService_.KnowledgeBaseId,
                                    EndpointKey = QnaService_.EndpointKey,
                                    Host = QnaService_.Host
                                }, new QnAMakerOptions
                                {
                                    ScoreThreshold = Convert.ToSingle(0.6),
                                    StrictFilters = MetadataFilter
                                });

                                var responseQnaFac = await SampleQnAFac.GetAnswersAsync(innerDc.Context);

                                if (responseQnaFac.Any())
                                {
                                    
                                    if (responseQnaFac?.LastOrDefault().Answer.ToString() != "Cuéntame, ¿En qué te puedo ayudar?")
                                    {
                                        await CallInterupAsync(innerDc, cancellationToken);
                                        return await base.OnContinueDialogAsync(innerDc, cancellationToken);
                                    }
                                    else
                                    {
                                        await innerDc.Context.SendActivityAsync(responseQnaFac?.LastOrDefault().Answer.ToString());
                                    }
                                    
                                    //userProfile.groupQuestion = false;
                                    return EndOfTurn;
                                }
                                break;
                            case DispatchLuis.Intent.q_Chitchat:
                                var QnaService = _services.GetCognitiveModels().QnAConfiguration["Chitchat"];
                                var SampleQnA = new QnAMaker(new QnAMakerEndpoint
                                {
                                    KnowledgeBaseId = QnaService.KnowledgeBaseId,
                                    EndpointKey = QnaService.EndpointKey,
                                    Host = QnaService.Host
                                }, new QnAMakerOptions
                                {
                                    ScoreThreshold = Convert.ToSingle(0.6),
                                    StrictFilters = MetadataFilter
                                });
                                var responseQna = await SampleQnA.GetAnswersAsync(innerDc.Context);

                                if (responseQna.Any())
                                {
                                    List<string> menu = responseQna?.LastOrDefault().Answer.ToString().Trim(new Char[] { '-' }).Split("\"").ToList().Where(x => x.Count() > 5).ToList();

                                    var titleMenu = menu.FirstOrDefault();
                                    menu.RemoveAt(0);

                                    var card = new HeroCard
                                    {
                                        Text = titleMenu.Replace("-", "").Replace("\n", ""),
                                        Buttons = menu.Select(choice => new CardAction(ActionTypes.ImBack, choice, value: choice)).ToList(),
                                    };

                                    List<Choice> choices = (ChoiceFactory.ToChoices(menu)).ToList();

                                    await innerDc.PromptAsync(nameof(TextPrompt), new PromptOptions
                                    {
                                        Prompt = (Activity)MessageFactory.Attachment(card.ToAttachment()),
                                        Choices = choices,
                                        Style = ListStyle.None
                                    }, cancellationToken);

                                    userProfile.groupQuestion = false;
                                    //userProfile.groupQuestion = false;
                                }


                                //return EndOfTurn;
                                break;

                            case DispatchLuis.Intent.None:
                                break;
                            default:
                                break;
                        }

                        //if (dispatchResult.TopIntent().intent == DispatchLuis.Intent.l_General)
                        //{
                        //    var generalResult = await localizedServices.LuisServices["General"].RecognizeAsync<GeneralLuis>(innerDc.Context, cancellationToken);
                        //    innerDc.Context.TurnState.Add(StateProperties.GeneralResult, generalResult);
                        //}


                        //var QnaService = _services.GetCognitiveModels().QnAConfiguration["Chitchat"];
                        //var QnaService_ = _services.GetCognitiveModels().QnAConfiguration["Faq"];

                        //Metadata[] MetadataFilter = null;
                        //MetadataFilter = new Metadata[]
                        //           {
                        //        new Metadata()
                        //        {
                        //            Name = "kb",
                        //            Value = userProfile.Company.ToLower()
                        //        }
                        //};

                        //var SampleQnA = new QnAMaker(new QnAMakerEndpoint
                        //{
                        //    KnowledgeBaseId = QnaService.KnowledgeBaseId,
                        //    EndpointKey = QnaService.EndpointKey,
                        //    Host = QnaService.Host
                        //}, new QnAMakerOptions
                        //{
                        //    ScoreThreshold = Convert.ToSingle(0.6),
                        //    StrictFilters = MetadataFilter
                        //});

                        //var SampleQnAFac = new QnAMaker(new QnAMakerEndpoint
                        //{
                        //    KnowledgeBaseId = QnaService_.KnowledgeBaseId,
                        //    EndpointKey = QnaService_.EndpointKey,
                        //    Host = QnaService_.Host
                        //}, new QnAMakerOptions
                        //{
                        //    ScoreThreshold = Convert.ToSingle(0.6),
                        //    StrictFilters = MetadataFilter
                        //});

                        //var responseQna = await SampleQnA.GetAnswersAsync(innerDc.Context);
                        //var responseQnaFac = await SampleQnAFac.GetAnswersAsync(innerDc.Context);

                        //if (responseQna.Any())
                        //{
                        //    if (responseQnaFac.Any() && responseQnaFac.FirstOrDefault().Score > responseQna.FirstOrDefault().Score)
                        //    {
                        //        await innerDc.Context.SendActivityAsync(responseQnaFac?.LastOrDefault().Answer.ToString());
                        //        await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage"), cancellationToken);
                        //        //userProfile.groupQuestion = false;
                        //        return EndOfTurn;
                        //    }
                        //    else
                        //    {
                        //        List<string> menu = responseQna?.LastOrDefault().Answer.ToString().Trim(new Char[] { '-' }).Split("\"").ToList().Where(x => x.Count() > 5).ToList();

                        //        //await innerDc.Context.SendActivityAsync(responseQna?.LastOrDefault().Answer.ToString());


                        //        //var options = await innerDc.PromptAsync(
                        //        //              nameof(ChoicePrompt),
                        //        //              new PromptOptions
                        //        //              {
                        //        //                  Prompt = CreateSuggestedActions(menu),
                        //        //                  Style = ListStyle.SuggestedAction
                        //        //              },
                        //        //              cancellationToken
                        //        //           );

                        //        var titleMenu = menu.FirstOrDefault();
                        //        menu.RemoveAt(0);

                        //        var card = new HeroCard
                        //        {
                        //            Text = titleMenu.Replace("-", "").Replace("\n", ""),
                        //            Buttons = menu.Select(choice => new CardAction(ActionTypes.ImBack, choice, value: choice)).ToList(),
                        //        };

                        //        List<Choice> choices = (ChoiceFactory.ToChoices(menu)).ToList();

                        //        await innerDc.PromptAsync(nameof(TextPrompt), new PromptOptions
                        //        {
                        //            Prompt = (Activity)MessageFactory.Attachment(card.ToAttachment()),
                        //            Choices = choices,
                        //            Style = ListStyle.None
                        //        }, cancellationToken);


                        //        userProfile.groupQuestion = false;
                        //        //return EndOfTurn;
                        //    }

                        //}

                    }

                    var interrupted = await CallInterupAsync(innerDc, cancellationToken);


                    if (interrupted)
                    {
                        return EndOfTurn;
                    }
                }
            }
            else
            {
                var text = "";
                if (innerDc.Context.Activity.Value != null)
                {
                    Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(innerDc.Context.Activity.Value.ToString());
                    text = response["boton"];
                }
                else
                {
                    if (Regex.IsMatch(innerDc.Context.Activity.Text, @"^\d+$") && userProfile.SubMenustep == 0)
                    {
                        text = FindQuestion(userProfile.LastMenu, innerDc.Context.Activity.Text);
                    } else
                    {
                        if (Regex.IsMatch(innerDc.Context.Activity.Text, @"^\d+$") && userProfile.SubMenustep > 0)
                        {
                            text = FindQuestion(userProfile.LastSubMenu, innerDc.Context.Activity.Text);
                        }
                        else
                        {
                            text = innerDc.Context.Activity.Text;

                        }
                    }
                }
                //text = Utility.RemoveAccents(text);
                (var menu, var subMenuFind) = GetSubMenuByCompany(text, userProfile.ChannelId, userProfile.Company.ToLower(), userProfile);
                userProfile.Conversations.Last().Questions.Last().OptionMenu = userProfile.Conversations.Last().Questions.Last().OptionMenu == null || userProfile.Conversations.Last().Questions.Last().OptionMenu == "Error" ? menu : userProfile.Conversations.Last().Questions.Last().OptionMenu;
                userProfile.Conversations.Last().Questions.Last().WasFromMenu = true;
                userProfile.SubMenustep += 1;

                if (userProfile.Conversations.Last().Questions.Last().OptionMenu == "Error" && userProfile.Conversations.Last().Questions.Last().OptionSubMenu == null)
                {
                    userProfile.SubMenustep = 0;
                    userProfile.numberErrorMessage += 1;
                    if (userProfile.numberErrorMessage == 3)
                    {

                        innerDc.Context.Activity.Text = "no";
                        var interrupted = await CallInterupAsync(innerDc, cancellationToken);
                        if (interrupted)
                        {
                            return EndOfTurn;
                        }
                    }
                    else
                    {
                        await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ErrorMenu"), cancellationToken);
                    }
                    return new DialogTurnResult(DialogTurnStatus.Waiting);
                }
                else
                {
                    if (userProfile.Conversations.Last().Questions.Last().OptionSubMenu == null && userProfile.SubMenustep == 1)
                    {
                        if (userProfile.Conversations.Last().Questions.Last().OptionMenu == "Salir")
                        {
                            await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage"), cancellationToken);
                            userProfile.SubMenustep = 0;
                            userProfile.MenuStarted = false;
                            return EndOfTurn;
                        }
                        else
                        {
                            var wpMenu = _templateManager.GenerateActivityForLocale(userProfile.Conversations.Last().Questions.Last().OptionMenu);

                            //Eliminar solo el if exterior cuando se desee habilitar la opciones de emergencia en el submenu el interior dejarlo
                            if (userProfile.Conversations.Last().Questions.Last().OptionMenu.Equals(MenuSubMenuEnum.Menu.Emergencias + userProfile.Company.ToUpper()))
                            {                                
                                userProfile.SubMenustep = 0;
                                userProfile.MenuStarted = false;
                                await innerDc.Context.SendActivityAsync(wpMenu, cancellationToken);
                                await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage"), cancellationToken);
                                return EndOfTurn;
                            }
                            else
                            {
                                if (wpMenu.Text != null)
                                {
                                    string[] listSMenu = Regex.Split(wpMenu.Text, @"[\n]");
                                    userProfile.LastSubMenu = listSMenu;
                                }
                            }                            

                            await innerDc.Context.SendActivityAsync(wpMenu, cancellationToken);
                            return EndOfTurn;
                        }
                    }
                    else
                    {
                        string subMenu;
                        if (userProfile.ChannelId == Channels.Directline)
                        {
                            subMenu = GetSubMenuItemWhatsapp(userProfile.Conversations.Last().Questions.Last().OptionMenu, text, userProfile.Company.ToUpper());
                        }
                        else
                        {
                            subMenu = GetSubMenuItem(userProfile.Conversations.Last().Questions.Last().OptionMenu, text, userProfile.Company.ToUpper());
                        }

                        if (subMenu == "Error")
                        {
                            userProfile.numberErrorMessage += 1;
                            if (userProfile.numberErrorMessage == 3)
                            {

                                innerDc.Context.Activity.Text = "no";
                                var interrupted = await CallInterupAsync(innerDc, cancellationToken);
                                if (interrupted)
                                {
                                    return EndOfTurn;
                                }
                            }
                            else
                            {
                                await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ErrorMenu"), cancellationToken);
                                return new DialogTurnResult(DialogTurnStatus.Waiting);
                            }
                        }
                        else
                        {

                            innerDc.Context.Activity.Text = subMenu;
                            userProfile.SubMenustep = 0;
                            var interrupted = await CallInterupAsync(innerDc, cancellationToken);
                            if (interrupted)
                            {

                                return EndOfTurn;
                            }
                            userProfile.MenuStarted = false;
                        }
                    }
                }
            }
            innerDc.Context.OnSendActivities(StoreOutgoingActivitiesAsync);
            if (innerDc.ActiveDialog.Id == FaqDialogId)
            {

                (companyId, Channel) = Utility.ValidateCompany(innerDc.Context.Activity, userProfile);
                var qnaDialog = TryCreateQnADialog(FaqDialogId, localizedServices, companyId);
                if (qnaDialog != null)
                {
                    Dialogs.Add(qnaDialog);
                }
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<bool> CallInterupAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            var userProfile = await _userProfileState.GetAsync(innerDc.Context, () => new UserProfileState());
            string menu = string.Empty;
            var activity = innerDc.Context.Activity;
            if (activity.Value != null)
            {
                if (userProfile.MenuStarted)
                {
                    activity.Text = innerDc.Context.Activity.Text;
                }
                else
                {
                    Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(innerDc.Context.Activity.Value.ToString());
                    activity.Text = response["boton"];

                    if (response.TryGetValue("id", out menu))
                    {
                        return true;
                    }
                }                                
            }

            //activity.Text = Utility.RemoveAccents(activity.Text);
            var localizedServices = _services.GetCognitiveModels();
            if (activity.Type == ActivityTypes.Message && !string.IsNullOrEmpty(activity.Text))
            {
                var dispatchResult = await localizedServices.DispatchService.RecognizeAsync<DispatchLuis>(innerDc.Context, cancellationToken: default);
                innerDc.Context.TurnState.Add(StateProperties.DispatchResult, dispatchResult);

                if (dispatchResult.TopIntent().intent == DispatchLuis.Intent.l_General)
                {
                    var generalResult = await localizedServices.LuisServices["General"].RecognizeAsync<GeneralLuis>(innerDc.Context, cancellationToken: default);
                    innerDc.Context.TurnState.Add(StateProperties.GeneralResult, generalResult);
                }

                var interrupted = await InterruptDialogAsync(innerDc, cancellationToken: default);

                return interrupted;

            }
            else
            {
                return false;
            }
        }
        protected virtual QnAMakerDialog TryCreateQnADialog(string knowledgebaseId, CognitiveModelSet cognitiveModels, string id)
        {
            if (!cognitiveModels.QnAConfiguration.TryGetValue(knowledgebaseId, out QnAMakerEndpoint qnaEndpoint)
                || qnaEndpoint == null)
            {
                throw new Exception($"Could not find QnA Maker knowledge base configuration with id: {knowledgebaseId}.");
            }
            Metadata[] MetadataFilter = null;
            MetadataFilter = new Metadata[]
                       {
                                new Metadata()
                                {
                                    Name = "kb",
                                    Value = id
                                }
                       };

            if (Dialogs.Find(knowledgebaseId) == null)
            {
                return new QnAMakerDialog(
                    knowledgeBaseId: qnaEndpoint.KnowledgeBaseId,
                    endpointKey: qnaEndpoint.EndpointKey,
                    hostName: qnaEndpoint.Host,
                    strictFilters: MetadataFilter,
                    threshold: 0.7F,
                    noAnswer: _templateManager.GenerateActivityForLocale("FirstErrorMessage"),
                    activeLearningCardTitle: _templateManager.GenerateActivityForLocale("QnaMakerAdaptiveLearningCardTitle").Text,
                    cardNoMatchText: _templateManager.GenerateActivityForLocale("QnaMakerNoMatchText").Text)
                {
                    Id = knowledgebaseId
                };
            }
            else
            {
                return null;
            }
        }

        private async Task<bool> InterruptDialogAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            var interrupted = false;
            var activity = innerDc.Context.Activity;
            var userProfile = await _userProfileState.GetAsync(innerDc.Context, () => new UserProfileState(), cancellationToken);
            var dialog = innerDc.ActiveDialog?.Id != null ? innerDc.FindDialog(innerDc.ActiveDialog?.Id) : null;
            if (activity.Type == ActivityTypes.Message && !string.IsNullOrEmpty(activity.Text))
            {
                var isSkill = dialog is SkillDialog;
                var dispatchResult = innerDc.Context.TurnState.Get<DispatchLuis>(StateProperties.DispatchResult);
                (var dispatchIntent, var dispatchScore) = dispatchResult.TopIntent();

                if (isSkill && IsSkillIntent(dispatchIntent) && dispatchIntent.ToString() != dialog.Id && dispatchScore > 0.9)
                {
                    if (_skillsConfig.Skills.TryGetValue(dispatchIntent.ToString(), out var identifiedSkill))
                    {
                        var prompt = _templateManager.GenerateActivityForLocale("SkillSwitchPrompt", new { Skill = identifiedSkill.Name });
                        await innerDc.BeginDialogAsync(_switchSkillDialog.Id, new SwitchSkillDialogOptions(prompt, identifiedSkill), cancellationToken);
                        interrupted = true;
                    }
                    else
                    {
                        throw new ArgumentException($"{dispatchIntent.ToString()} is not in the skills configuration");
                    }
                }

                Conversation lastConversation = null;
                if (userProfile.Conversations.Last().Questions == null)
                {
                    userProfile.Conversations.Last().Questions = new List<Question>();
                }
                if (userProfile.Conversations != null && userProfile.Conversations.Last().Questions.Any())
                {
                    lastConversation = userProfile.Conversations.Last();
                }
                (companyId, Channel) = Utility.ValidateCompany(innerDc.Context.Activity, userProfile);
                Question question = new Question();
                question.Answers = new List<string>();
                question.Text = activity.Text;
                question.DateTime = DateTime.Now;
                question.DispatchIntent = dispatchIntent.ToString();
                question.DispatchScore = dispatchScore;
                question.CompanyId = companyId;
                question.CompanyName = companyId;
                question.QuestionId = lastConversation != null ? lastConversation.Questions.Last().QuestionId + 1 : 1;
                question.QuestionType = MenuSubMenuEnum.QuestionType.Transaccionales.ToString();


                if (dispatchIntent == DispatchLuis.Intent.l_General)
                {
                    var generalResult = innerDc.Context.TurnState.Get<GeneralLuis>(StateProperties.GeneralResult);
                    (var generalIntent, var generalScore) = generalResult.TopIntent();


                    if (generalScore > 0.5)
                    {
                        switch (generalIntent)
                        {
                            case GeneralLuis.Intent.Menu:
                                {
                                    userProfile.numberErrorMessage = 0;
                                    userProfile.MenuStarted = true;

                                    var listMenu = _templateManager.GenerateActivityForLocale(GetMenuPrincipalByCompany(userProfile, userProfile.ChannelId, userProfile.Company.ToUpper()));

                                    if (listMenu.Text != null)
                                    {
                                        string[] listSMenu = Regex.Split(listMenu.Text, @"[\n]");
                                        userProfile.LastMenu = listSMenu;
                                    }

                                    await innerDc.Context.SendActivityAsync(listMenu);
                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Abona_a_tu_factura:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;
                                    question.Topic = MenuSubMenuEnum.Topic.Pagos.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Abona_a_tu_factura.ToString().Replace("_", " ").ToLower();

                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CertificadodetusfacturasTemporalMessage", userProfile), cancellationToken);
                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage", userProfile), cancellationToken);



                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Adquiere_tu_seguro:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;

                                    question.Topic = MenuSubMenuEnum.Topic.Brilla_y_seguros.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Adquiere_tu_seguro.ToString().Replace("_", " ").ToLower();

                                    userProfile.Conversations.Last().Questions.Add(question);

                                    if (userProfile.ChannelId == Channels.Webchat)
                                    {
                                        userProfile.PrincipalDialog = "VentaSegurosBrillaDialogCard";
                                        await innerDc.BeginDialogAsync(_ventaSegurosBrillaDialogCard.Id);
                                    }
                                    else
                                    {
                                        userProfile.PrincipalDialog = "VentaSegurosBrillaDialog";
                                        await innerDc.BeginDialogAsync(_ventaSegurosBrillaDialog.Id);
                                    }

                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Cambio_de_nombre:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;
                                    question.Topic = MenuSubMenuEnum.Topic.Informacion_general.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Cambio_de_nombre.ToString().Replace("_", " ").ToLower();

                                    innerDc.Context.TurnState.Add("TipodeDialogo", "CambioNombreDialog");
                                    await innerDc.BeginDialogAsync(_cambioNombreDialog.Id);

                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Certificado_de_tus_facturas:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;
                                    question.Topic = MenuSubMenuEnum.Topic.Tu_Factura.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Certificado_de_tus_facturas.ToString().Replace("_", " ").ToLower();

                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CertificadodetusfacturasTemporalMessage", userProfile), cancellationToken);
                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage", userProfile), cancellationToken);
                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Certificado_deuda_diferida:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;
                                    question.Topic = MenuSubMenuEnum.Topic.Tu_Factura.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Certificado_deuda_diferida.ToString().Replace("_", " ").ToLower();

                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CertificadodeudadiferidaTemporalMessage", userProfile), cancellationToken);
                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage", userProfile), cancellationToken);
                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Constancia_de_pago:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;
                                    question.Topic = MenuSubMenuEnum.Topic.Pagos.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Constancia_de_pago.ToString().Replace("_", " ").ToLower();

                                    innerDc.Context.TurnState.Add("TipodeDialogo", "ConstaciaPagoDialog");
                                    await innerDc.BeginDialogAsync(_constaciaPagoDialog.Id);

                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Consulta_y_generacion_de_factura:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;
                                    question.Topic = MenuSubMenuEnum.Topic.Tu_Factura.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Consulta_y_generacion_de_factura.ToString().Replace("_", " ").ToLower();

                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ConsultaygeneraciondefacturaTemporalMessage", userProfile), cancellationToken);
                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage", userProfile), cancellationToken);

                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Consumo_facturado:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;

                                    question.Topic = MenuSubMenuEnum.Topic.Tu_Factura.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Consumo_facturado.ToString().Replace("_", " ").ToLower();
                                    userProfile.Conversations.Last().Questions.Add(question);

                                    if (userProfile.ChannelId == Channels.Webchat)
                                    {
                                        userProfile.PrincipalDialog = "ConsumoFacturadoDialogCard";
                                        await innerDc.BeginDialogAsync(_consumoFacturadoCard.Id);
                                    }
                                    else
                                    {
                                        userProfile.PrincipalDialog = "ConsumoFacturadoDialog";
                                        await innerDc.BeginDialogAsync(_consumoFacturadoDialog.Id);
                                    }
                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Cupo_de_brilla:
                                {

                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;

                                    question.Topic = MenuSubMenuEnum.Topic.Brilla_y_seguros.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Cupo_de_brilla.ToString().Replace("_", " ").ToLower();
                                    userProfile.Conversations.Last().Questions.Add(question);

                                    if (userProfile.ChannelId == Channels.Webchat)
                                    {
                                        userProfile.PrincipalDialog = "ConsultaCupoBrillaDialogCard";
                                        await innerDc.BeginDialogAsync(_consultaCupoBrillaDialogCard.Id);
                                    }
                                    else
                                    {
                                        userProfile.PrincipalDialog = "ConsultaCupoBrillaDialog";
                                        await innerDc.BeginDialogAsync(_consultaCupoBrillaDialog.Id);
                                    }
                                    interrupted = true;
                                    break;

                                    //userProfile.MenuStarted = false;
                                    //userProfile.numberErrorMessage = 0;
                                    //question.Topic = MenuSubMenuEnum.Topic.Brilla_y_seguros.ToString().Replace("_", " ").ToLower();
                                    //question.SubTopic = MenuSubMenuEnum.SubMenu.Cupo_de_brilla.ToString().Replace("_", " ").ToLower();

                                    //await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("", userProfile), cancellationToken);
                                    //await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage", userProfile), cancellationToken);

                                    //interrupted = true;
                                    //break;
                                }
                            case GeneralLuis.Intent.Fecha_proxima_revision_periodica:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;
                                    question.Topic = MenuSubMenuEnum.Topic.Revision_periodica.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Fecha_proxima_revision_periodica.ToString().Replace("_", " ").ToLower();

                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("FechaRevisionPeriodicaTemporalMessage", userProfile), cancellationToken);
                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage", userProfile), cancellationToken);



                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Solicita_tu_cupon_de_pago_anticipado:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;
                                    question.Topic = MenuSubMenuEnum.Topic.Pagos.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Solicita_tu_cupon_de_pago_anticipado.ToString().Replace("_", " ").ToLower();

                                    userProfile.Conversations.Last().Questions.Add(question);

                                    if (userProfile.ChannelId == Channels.Webchat)
                                    {
                                        await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("PagoParcialTemporalMessage", userProfile), cancellationToken);
                                        await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage", userProfile), cancellationToken);
                                        interrupted = true;
                                        break;
                                    }
                                    else
                                    {
                                        await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("PagoParcialTemporalMessage", userProfile), cancellationToken);
                                        await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage", userProfile), cancellationToken);
                                        interrupted = true;
                                        break;
                                    }

                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Deuda_actual_y_diferida:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;
                                    question.Topic = MenuSubMenuEnum.Topic.Tu_Factura.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Deuda_actual_y_diferida.ToString().Replace("_", " ").ToLower();

                                    userProfile.Conversations.Last().Questions.Add(question);

                                    if (userProfile.ChannelId == Channels.Webchat)
                                    {
                                        userProfile.PrincipalDialog = "EstadoCuentaDialogCard";
                                        await innerDc.BeginDialogAsync(_estadoCuentaDialogCard.Id);
                                    }
                                    else
                                    {
                                        userProfile.PrincipalDialog = "EstadoCuentaDialog";
                                        await innerDc.BeginDialogAsync(_estadoCuentaDialog.Id);
                                    }




                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Emergencias:
                                {                                  
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;

                                    question.Topic = MenuSubMenuEnum.Topic.Emergencias.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Emergencia.ToString().Replace("_", " ").ToLower();

                                    userProfile.Conversations.Last().Questions.Add(question);

                                    switch (companyId.ToLower())
                                    {
                                        case MenuSubMenuEnum.CompanyName.Gdo: 
                                            { 
                                                await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("EmergenciaMessageGDO"), cancellationToken: cancellationToken);
                                                break;
                                            }
                                        case MenuSubMenuEnum.CompanyName.Ceo:
                                            {
                                                await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("EmergenciaMessageCEO"), cancellationToken: cancellationToken);
                                                break;
                                            }
                                        case MenuSubMenuEnum.CompanyName.Stg:
                                            {
                                                await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("EmergenciaMessageSTG"), cancellationToken: cancellationToken);
                                                break;
                                            }
                                        case MenuSubMenuEnum.CompanyName.Quavii:
                                            {
                                                await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("EmergenciaMessageQUAVII"), cancellationToken: cancellationToken);
                                                break;
                                            }
                                        default:
                                            break;
                                    }

                                    //if (userProfile.ChannelId == Channels.Webchat)
                                    //{
                                    //    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("EmergenciaMessageGDO"), cancellationToken: cancellationToken);
                                    //    userProfile.PrincipalDialog = "ConsultaPQRDialogCard";
                                    //    await innerDc.BeginDialogAsync(_consultaPQRDialogCard.Id);
                                    //}
                                    //else
                                    //{
                                    //    userProfile.PrincipalDialog = "ConsultaPQRdialog";
                                    //    await innerDc.BeginDialogAsync(_consultaPQRdialog.Id);
                                    //}



                                    //await innerDc.BeginDialogAsync(_emergenciasDialog.Id);

                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Estado_reconexion_por_pago:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;

                                    question.Topic = MenuSubMenuEnum.Topic.Pagos.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Estado_reconexión_por_pago.ToString().Replace("_", " ").ToLower();
                                    userProfile.Conversations.Last().Questions.Add(question);

                                    if (userProfile.ChannelId == Channels.Webchat)
                                    {
                                        userProfile.PrincipalDialog = "ReconexionPorPagoDialogCard";
                                        await innerDc.BeginDialogAsync(_reconexionPorPagoCard.Id);
                                    }
                                    else
                                    {
                                        userProfile.PrincipalDialog = "ReconexionPorPagoDialog";
                                        await innerDc.BeginDialogAsync(_reconexionPorPago.Id);
                                    }
                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Estado_de_solicitud_PQRs:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;
                                    question.Topic = MenuSubMenuEnum.Topic.Informacion_general.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Estado_de_solicitud_PQRS.ToString().Replace("_", " ").ToLower();

                                    userProfile.Conversations.Last().Questions.Add(question);

                                    if (userProfile.ChannelId == Channels.Webchat)
                                    {
                                        userProfile.PrincipalDialog = "ConsultaPQRDialogCard";
                                        await innerDc.BeginDialogAsync(_consultaPQRDialogCard.Id);
                                    }
                                    else
                                    {
                                        userProfile.PrincipalDialog = "ConsultaPQRdialog";
                                        await innerDc.BeginDialogAsync(_consultaPQRdialog.Id);
                                    }

                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Exencion_de_contribucion:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;

                                    question.Topic = MenuSubMenuEnum.Topic.Tu_Factura.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Exencion_de_contribucion.ToString().Replace("_", " ").ToLower();

                                    userProfile.Conversations.Last().Questions.Add(question);

                                    innerDc.Context.TurnState.Add("TipodeDialogo", "ExencionContribucionDialog");

                                    await innerDc.BeginDialogAsync(_exencionContribucionDialog.Id);
                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Identidad_de_funcionario:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;
                                    question.Topic = MenuSubMenuEnum.Topic.Informacion_general.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Identidad_de_funcionario.ToString().Replace("_", " ").ToLower();

                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("", userProfile), cancellationToken);
                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage", userProfile), cancellationToken);

                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Terminacion_de_contrato:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;

                                    question.Topic = MenuSubMenuEnum.Topic.Informacion_general.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Terminacion_de_contrato.ToString().Replace("_", " ").ToLower();

                                    userProfile.Conversations.Last().Questions.Add(question);

                                    innerDc.Context.TurnState.Add("TipodeDialogo", "TerminacionContratoDialog");
                                    await innerDc.BeginDialogAsync(_terminacionContratoDialog.Id);

                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Traslado_de_deuda_o_desmonte:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;

                                    question.Topic = MenuSubMenuEnum.Topic.Gestion_deuda.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Traslado_de_deuda_o_desmonte.ToString().Replace("_", " ").ToLower();

                                    userProfile.Conversations.Last().Questions.Add(question);

                                    innerDc.Context.TurnState.Add("TipodeDialogo", "TrasladoDeudaDialog");
                                    await innerDc.BeginDialogAsync(_trasladoDeudaDialog.Id);
                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Ultima_lectura:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;

                                    question.Topic = MenuSubMenuEnum.Topic.Tu_Factura.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Ultima_lectura.ToString().Replace("_", " ").ToLower();

                                    userProfile.Conversations.Last().Questions.Add(question);

                                    if (userProfile.ChannelId == Channels.Webchat)
                                    {
                                        userProfile.PrincipalDialog = "TomalecturaDialogCard";
                                        await innerDc.BeginDialogAsync(_tomalecturaDialogCard.Id);
                                    }
                                    else
                                    {
                                        userProfile.PrincipalDialog = "TomaLecturaDialog";
                                        await innerDc.BeginDialogAsync(_tomaLecturaDialog.Id);
                                    }

                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Utiliza_tu_cupo:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;

                                    question.Topic = MenuSubMenuEnum.Topic.Brilla_y_seguros.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Utiliza_tu_cupo.ToString().Replace("_", " ").ToLower();

                                    //userProfile.Conversations.Last().Questions.Add(question);

                                    //innerDc.Context.TurnState.Add("TipodeDialogo", "TrasladoDeudaDialog");

                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("UtilizatucupoTemporalMessage", userProfile), cancellationToken);
                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage", userProfile), cancellationToken);
                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Valor_fecha_limite_y_referencia_pago:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;

                                    question.Topic = MenuSubMenuEnum.Topic.Tu_Factura.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Valor_fecha_limite_y_referencia_pago.ToString().Replace("_", " ").ToLower();

                                    userProfile.Conversations.Last().Questions.Add(question);

                                    if (userProfile.ChannelId == Channels.Webchat)
                                    {
                                        userProfile.PrincipalDialog = "ConceptoFacturaDialogCard";
                                        await innerDc.BeginDialogAsync(_conceptoFacturaCard.Id);
                                    }
                                    else
                                    {
                                        userProfile.PrincipalDialog = "ConceptoFacturaDialog";
                                        await innerDc.BeginDialogAsync(_conceptoFacturaDialog.Id);
                                    }

                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Visita_de_financiacion_no_bancaria:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;

                                    question.Topic = MenuSubMenuEnum.Topic.Tu_Factura.ToString().Replace("_", " ").ToLower();
                                    question.SubTopic = MenuSubMenuEnum.SubMenu.Consumo_facturado.ToString().Replace("_", " ").ToLower();
                                    userProfile.Conversations.Last().Questions.Add(question);

                                    if (userProfile.ChannelId == Channels.Webchat)
                                    {
                                        userProfile.PrincipalDialog = "VisitaFinanciacionDialogCard";
                                        await innerDc.BeginDialogAsync(_visitaFinanciacionDialogCard.Id);
                                    }
                                    else
                                    {
                                        userProfile.PrincipalDialog = "VisitaFinanciacionDialog";
                                        await innerDc.BeginDialogAsync(_visitaFinanciacionDialog.Id);
                                    }
                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Confirm:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;
                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("FirstPromptMessage", userProfile), cancellationToken);
                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Salir:
                                {
                                    userProfile.MenuStarted = false;
                                    userProfile.numberErrorMessage = 0;
                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage", userProfile), cancellationToken);
                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.Reject:
                                {


                                    if (userProfile.Conversations.Last().Questions != null && userProfile.Conversations.Last().Questions.Count != 0 && userProfile.numberErrorMessage == 0)
                                    {
                                        if (userProfile.Conversations.Last().Questions.Last().DispatchIntent != DispatchLuis.Intent.q_Chitchat.ToString())
                                        {
                                            await innerDc.CancelAllDialogsAsync(cancellationToken);
                                            innerDc.Context.TurnState.Add("TipodeDialogo", "FeedbackDialogo");
                                            await innerDc.BeginDialogAsync(_feedBack.Id);
                                            userProfile.numberErrorMessage = 0;
                                        }
                                        else
                                        {
                                            userProfile.numberErrorMessage += 1;
                                            if (!userProfile.MenuStarted)
                                            {
                                                if (userProfile.numberErrorMessage == 3)
                                                {
                                                    userProfile.numberErrorMessage = 0;
                                                    userProfile.MenuStarted = true;
                                                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(GetMenuByCompany(userProfile, userProfile.ChannelId)));
                                                }
                                                else
                                                {
                                                    await innerDc.Context.SendActivityAsync(ValidatErrorMessage(userProfile));
                                                }
                                            }
                                        }

                                    }
                                    else
                                    {
                                        userProfile.numberErrorMessage += 1;
                                        if (!userProfile.MenuStarted)
                                        {
                                            if (userProfile.numberErrorMessage == 3)
                                            {
                                                userProfile.numberErrorMessage = 0;
                                                userProfile.MenuStarted = true;
                                                await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(GetMenuByCompany(userProfile, userProfile.ChannelId)));
                                            }
                                            else
                                            {
                                                await innerDc.Context.SendActivityAsync(ValidatErrorMessage(userProfile));
                                            }
                                        }
                                        else
                                        {
                                            await innerDc.CancelAllDialogsAsync(cancellationToken);
                                            await innerDc.BeginDialogAsync(_feedBack.Id);
                                            userProfile.numberErrorMessage = 0;
                                            userProfile.MenuStarted = false;
                                        }
                                    }

                                    interrupted = true;
                                    break;
                                }
                            case GeneralLuis.Intent.None:
                                {
                                    userProfile.numberErrorMessage += 1;
                                    if (!userProfile.MenuStarted)
                                    {
                                        if (userProfile.numberErrorMessage == 3)
                                        {
                                            userProfile.numberErrorMessage = 0;
                                            userProfile.MenuStarted = true;
                                            await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(GetMenuByCompany(userProfile, userProfile.ChannelId)));
                                        }
                                        else
                                        {
                                            await innerDc.Context.SendActivityAsync(ValidatErrorMessage(userProfile));
                                        }
                                    }
                                    interrupted = true;
                                    break;
                                }
                            default:
                                {
                                    if (!userProfile.MenuStarted)
                                    {
                                        if (userProfile.numberErrorMessage == 3)
                                        {
                                            userProfile.numberErrorMessage = 0;
                                            userProfile.MenuStarted = true;
                                            await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(GetMenuByCompany(userProfile, userProfile.ChannelId)));
                                        }
                                        else
                                        {
                                            await innerDc.Context.SendActivityAsync(ValidatErrorMessage(userProfile));
                                        }
                                    }
                                    userProfile.numberErrorMessage += 1;
                                    interrupted = true;
                                    break;
                                }
                        }

                        userProfile.Conversations.Last().Questions.Add(question);
                        userProfile.groupQuestion = true;
                    }
                    else
                    {

                        userProfile.numberErrorMessage += 1;
                        if (!userProfile.MenuStarted)
                        {
                            if (userProfile.numberErrorMessage == 3)
                            {
                                userProfile.numberErrorMessage = 0;
                                userProfile.MenuStarted = true;
                                await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(GetMenuByCompany(userProfile, userProfile.ChannelId)));
                            }
                            else
                            {
                                await innerDc.Context.SendActivityAsync(ValidatErrorMessage(userProfile));
                            }
                        }

                        userProfile.Conversations.Last().Questions.Add(question);
                        interrupted = true;
                    }

                }
                else if (dispatchIntent == DispatchLuis.Intent.None)
                {
                    userProfile.numberErrorMessage += 1;
                    if (!userProfile.MenuStarted)
                    {
                        if (userProfile.numberErrorMessage == 3)
                        {
                            userProfile.numberErrorMessage = 0;
                            userProfile.MenuStarted = true;
                            await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(GetMenuByCompany(userProfile, userProfile.ChannelId)));
                        }
                        else
                        {
                            await innerDc.Context.SendActivityAsync(ValidatErrorMessage(userProfile));
                        }
                    }

                    userProfile.Conversations.Last().Questions.Add(question);
                    interrupted = true;

                }
            }


            return interrupted;
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _userProfileState.GetAsync(stepContext.Context, () => new UserProfileState());
            Activity activity = new Activity();
            var prompt = new PromptOptions();

            if (stepContext.SuppressCompletionMessage() && userProfile.numberErrorMessage == 0 && !userProfile.MenuStarted)
            {
                await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage"), cancellationToken);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions(), cancellationToken);
            }
            if ((userProfile.DidBotWelcomeUser || userProfile.ChannelId == Channels.Directline) && userProfile.Conversations != null && !stepContext.SuppressCompletionMessage()
                && (userProfile.Conversations.Last().Questions.Last().DispatchIntent != DispatchLuis.Intent.q_Chitchat.ToString() || userProfile.TerminateWork))
            {
                activity = stepContext.Context.Activity;
                Company company = new Company();
                (company.Name, Channel) = ValidateCompany(activity, userProfile);
                userProfile.Company = company.Name;//Cambiar a upper

                if (userProfile.Conversations == null)
                {
                    userProfile.Conversations = new List<Conversation>
                    {
                        new Conversation() { ConversationId = 1 }
                    };

                }
                else
                {
                    userProfile.Conversations.Add(new Conversation() { ConversationId = userProfile.Conversations.Last().ConversationId + 1 });
                }
                userProfile.TerminateWork = false;
                userProfile.WorkStarted = false;
                userProfile.Dialogturn = null;
                userProfile.MenuStarted = false;
                userProfile.SubMenustep = 0;
                userProfile.CompanyName = Utility.GetCompanyName(company.Name);

                var displayMessage = "";

                if (company.Name.ToLower().Equals(MenuSubMenuEnum.CompanyName.Gdo))
                {
                    displayMessage = "NewUserMessageGDO";
                }
                else
                {
                    displayMessage = "NewUserMessage";
                }                

                await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(displayMessage, userProfile), cancellationToken);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions(), cancellationToken);
            }
            else
            {
                if (userProfile.Conversations == null)
                {
                    userProfile.Conversations = new List<Conversation>();
                    userProfile.Conversations.Add(new Conversation() { ConversationId = 1 });
                }
                return await stepContext.PromptAsync(nameof(TextPrompt), prompt, cancellationToken);
            }

        }

        private async Task<DialogTurnResult> RouteStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var activity = stepContext.Context.Activity.AsMessageActivity();
            var userProfile = await _userProfileState.GetAsync(stepContext.Context, () => new UserProfileState(), cancellationToken);
            (companyId, Channel) = Utility.ValidateCompany(stepContext.Context.Activity, userProfile);
            Question question = new Question();
            if (!string.IsNullOrEmpty(activity.Text))
            {
                if (userProfile.Conversations.Last().Questions == null)
                {
                    userProfile.Conversations.Last().Questions = new List<Question>();
                }
                Conversation lastConversation = null;
                if (userProfile.Conversations != null && userProfile.Conversations.Last().Questions.Any())
                {
                    lastConversation = userProfile.Conversations.Last();
                }

                question.Answers = new List<string>();
                question.Text = activity.Text;
                question.DateTime = DateTime.Now;

                var localizedServices = _services.GetCognitiveModels();

                var dispatchResult = stepContext.Context.TurnState.Get<DispatchLuis>(StateProperties.DispatchResult);
                (var dispatchIntent, var dispatchScore) = dispatchResult.TopIntent();

                question.DispatchIntent = dispatchIntent.ToString();
                question.DispatchScore = dispatchScore;
                question.CompanyId = companyId;
                question.CompanyName = companyId;
                question.OptionMenu = lastConversation?.Questions.Last().OptionMenu;
                question.WasFromMenu = lastConversation != null && lastConversation.Questions.Last().WasFromMenu;
                question.OptionSubMenu = lastConversation?.Questions.Last().OptionSubMenu;
                question.QuestionId = lastConversation != null ? lastConversation.Questions.Last().QuestionId + 1 : 1;


                if (IsSkillIntent(dispatchIntent))
                {
                    var dispatchIntentSkill = dispatchIntent.ToString();
                    var skillDialogArgs = new BeginSkillDialogOptions { Activity = (Activity)activity };
                    userProfile.numberErrorMessage = 0;
                    var selectedSkill = _skillsConfig.Skills[dispatchIntentSkill];
                    await _activeSkillProperty.SetAsync(stepContext.Context, selectedSkill, cancellationToken);

                    return await stepContext.BeginDialogAsync(dispatchIntentSkill, skillDialogArgs, cancellationToken);
                }

                if (dispatchIntent == DispatchLuis.Intent.q_Faq)
                {
                    stepContext.SuppressCompletionMessage(true);

                    question.QuestionType = MenuSubMenuEnum.QuestionType.Informativas.ToString();
                    userProfile.Conversations.Last().Questions.Add(question);
                    var knowledgebaseId = FaqDialogId;

                    var qnaDialog = TryCreateQnADialog(knowledgebaseId, localizedServices, companyId);
                    if (qnaDialog != null)
                    {
                        Dialogs.Add(qnaDialog);
                    }
                    userProfile.groupQuestion = true;
                    return await stepContext.BeginDialogAsync(knowledgebaseId, cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("FinishedMessage", userProfile), cancellationToken);

                    return await stepContext.NextAsync();
                }

            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var userProfile = await _userProfileState.GetAsync(stepContext.Context, () => new UserProfileState());
            if (userProfile.Conversations.Last().Questions.Last().DispatchIntent != DispatchLuis.Intent.q_Chitchat.ToString())
            {
                await Task.Delay(1500);
            }
            return await stepContext.ReplaceDialogAsync(InitialDialogId, cancellationToken: cancellationToken);
        }

        private async Task<ResourceResponse[]> StoreOutgoingActivitiesAsync(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {

            var messageActivities = activities
                 .Where(a => a.Type == ActivityTypes.Message)
                 .ToList();

            var traceActivities = activities
                .Where(a => a.Type == ActivityTypes.Trace)
                .ToList();
            var userProfile = await _userProfileState.GetAsync(turnContext, () => new UserProfileState());

            if (traceActivities.Any())
            {
                Conversation conversation = null;
                if (userProfile.Conversations != null)
                {

                    conversation = userProfile.Conversations.Last();
                    if (activities.Any())
                    {
                        var queryResults = ((Microsoft.Bot.Builder.AI.QnA.QnAMakerTraceInfo)activities[0].Value).QueryResults;

                        if (queryResults.Any())
                        {
                            var queryResult = queryResults.FirstOrDefault();
                            if (queryResult.Metadata.Any())
                            {
                                conversation.Questions.Last().CompanyName = queryResult.Metadata[0].Value;
                                if (ValidateMetadata(queryResult.Metadata))
                                {
                                    conversation.Questions.Last().Topic = queryResult.Metadata[1].Value;
                                    conversation.Questions.Last().SubTopic = queryResult.Metadata[2].Value;
                                }
                            }
                        }
                    }


                }
            }
            if (messageActivities.Any())
            {
                var botResponse = await _previousResponseAccessor.GetAsync(turnContext, () => new List<Activity>());
                Conversation conversation = new Conversation();
                userProfile.Name = turnContext.Activity.From.Name;
                if (userProfile.Conversations != null)
                {
                    conversation = userProfile.Conversations.Last();

                }
                List<string> answers = new List<string>();
                if (userProfile.Dialogturn == null)
                {
                    botResponse = botResponse
                    .Concat(messageActivities)
                    .Where(a => a.ReplyToId == turnContext.Activity.Id)
                    .ToList();
                }
                if (botResponse.Count > 0 && botResponse?[0].Text != null)
                {
                    string errorMessage = null;
                    if (botResponse[0].Text.Equals(_templateManager.GenerateActivityForLocale("FirstErrorMessage", userProfile).Text))
                    {
                        if (userProfile.Conversations.Last().Questions.Last().QuestionType != "Transaccionales")
                        {
                            userProfile.numberErrorMessage = userProfile.numberErrorMessage + 1;
                            errorMessage = ValidatErrorMessage(userProfile);
                        }

                    }
                    else
                    {
                        userProfile.numberErrorMessage = 0;
                    }

                    if (errorMessage != null)
                    {
                        botResponse[0].Text = errorMessage;
                    }

                    if (conversation.Questions != null)
                    {
                        answers.Add(botResponse.Last().Text);
                    }
                    if (userProfile.numberErrorMessage >= 3)
                    {
                        userProfile.numberErrorMessage = 0;
                        if (userProfile.ChannelId == Channels.Directline)
                        {
                            userProfile.MenuStarted = true;
                            botResponse[0].Text = _templateManager.GenerateActivityForLocale(GetMenuByCompany(userProfile, userProfile.ChannelId)).Text;
                        }
                        else
                        {
                            botResponse[0].Text = string.Empty;
                            botResponse[0].Attachments = _templateManager.GenerateActivityForLocale(GetMenuByCompany(userProfile, userProfile.ChannelId)).Attachments;
                        }
                    }

                    if (conversation != null && conversation.Questions != null)
                    {
                        conversation.Questions.Last().Answers.AddRange(answers);
                    }
                }
                if (userProfile.Conversations != null)
                {
                    if (userProfile.Conversations.Count() > 1 && userProfile.Conversations[userProfile.Conversations.Count() - 2].AnalyticScore != null)
                    {
                        botResponse.Clear();
                    }
                }
                await _previousResponseAccessor.SetAsync(turnContext, botResponse);
            }
            return await next();
        }



        private bool IsSkillIntent(DispatchLuis.Intent dispatchIntent)
        {
            if (dispatchIntent.ToString().Equals(DispatchLuis.Intent.l_General.ToString(), StringComparison.InvariantCultureIgnoreCase) ||
                dispatchIntent.ToString().Equals(DispatchLuis.Intent.q_Faq.ToString(), StringComparison.InvariantCultureIgnoreCase) ||
                dispatchIntent.ToString().Equals(DispatchLuis.Intent.None.ToString(), StringComparison.InvariantCultureIgnoreCase) ||
                dispatchIntent.ToString().Equals(DispatchLuis.Intent.q_Chitchat.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            return true;
        }


        //private static Activity CreateSuggestedActions(List<string> menu)
        //{
        //    var reply = MessageFactory.Text(menu[0].Replace("\n", "").ToString());
        //    var list = new List<CardAction>();

        //    for (int i = 0; i < menu.Count(); i++)
        //    {
        //        if (i == 0)
        //        {
        //            continue;
        //        }
        //        list.Add(new CardAction() { Title = menu[i], Value = menu[i], Type = ActionTypes.ImBack });
        //    }

        //    reply.SuggestedActions = new SuggestedActions()
        //    {
        //        Actions = list
        //    };

        //    return reply;
        //}

        private string ValidatErrorMessage(UserProfileState user)
        {
            string error = "";
            switch (user.numberErrorMessage)
            {
                case 1:
                    error = _templateManager.GenerateActivityForLocale("FirstErrorMessage").Text;
                    return error;
                case 2:
                    error = _templateManager.GenerateActivityForLocale("SecondErrorMessage").Text;
                    return error;
                case 3:
                    error = _templateManager.GenerateActivityForLocale("SecondErrorMessage").Text;
                    user.MenuStarted = true;
                    return error;
                default:
                    error = _templateManager.GenerateActivityForLocale("SecondErrorMessage").Text;
                    return error;
            }
        }
        private bool ValidateMetadata(Metadata[] metadata)
        {
            if (metadata.Count() < 3)
            {
                return false;
            }
            return true;
        }


    }
}
