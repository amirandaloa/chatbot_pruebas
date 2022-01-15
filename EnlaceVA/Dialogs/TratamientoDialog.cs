using EnlaceVA.Models;
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
using Newtonsoft.Json;
using Microsoft.Bot.Connector;
using System.Globalization;
using static EnlaceVA.Utilities.Utility;

namespace EnlaceVA.Dialogs
{
    public class TratamientoDialog : ComponentDialog
    {               
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;        
        private readonly ConstaciaPagoDialog _constaciaPagoDialog;


        public TratamientoDialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(TratamientoDialog))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));                        
            _constaciaPagoDialog = serviceProvider.GetService<ConstaciaPagoDialog>();
                       
            var ConstaciaPagoSteps = new WaterfallStep[]
            {
                DatosGasAsync,
                DatosBrillaAsync,                
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;            
            AddDialog(new WaterfallDialog(nameof(TratamientoDialog), ConstaciaPagoSteps));            
            AddDialog(_constaciaPagoDialog);
        }

        private async Task<DialogTurnResult> DatosGasAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "TratamientoDialog";
            await _userState.SaveChangesAsync(stepContext.Context, true);

            string channel = userProfile.ChannelId;
            var ValidationGas = "";

            if (Channels.Directline == channel)
            {
                ValidationGas = ValidateMessage(userProfile.Company.ToLower(CultureInfo.CurrentCulture), MessagesTratamientodialog.TipeCarGas);
            }
            else
            {
                ValidationGas = ValidateCard(userProfile.Company.ToLower(CultureInfo.CurrentCulture), MessagesTratamientodialog.TipeCarGas);
            }
            
            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(ValidationGas), cancellationToken: cancellationToken);
                   
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> DatosBrillaAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());            
            await _userState.SaveChangesAsync(stepContext.Context, true);

            var validationBrilla = "";
            string channel = userProfile.ChannelId;

            if (channel == Channels.Directline)
            {
                validationBrilla = ValidateMessage(userProfile.Company.ToLower(CultureInfo.CurrentCulture), MessagesTratamientodialog.TipeCardBrilla);
            }
            else
            {
                validationBrilla = ValidateCard(userProfile.Company.ToLower(CultureInfo.CurrentCulture), MessagesTratamientodialog.TipeCardBrilla);
            }          

            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(validationBrilla), cancellationToken: cancellationToken);
              
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
        
        private async Task<DialogTurnResult> FinishDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = null;
            await _userState.SaveChangesAsync(stepContext.Context, true);

            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var userProfile = await _accessor.GetAsync(innerDc.Context, () => new UserProfileState());            
            await _userState.SaveChangesAsync(innerDc.Context, true);

            var text = "";

            if (innerDc.Context.Activity.Value == null)
            {
                text = innerDc.Context.Activity.Text.ToLower(CultureInfo.CurrentCulture);
            }
            else
            {
                text = innerDc.Context.Activity.Value.ToString();
            }
            
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);
            var channel = userProfile.ChannelId;
            var datagas = userProfile.Conversations.Last().Questions.Last().DataTreatmentGas;

            if (userProfile.Dialogturn == "TratamientoDialog")
            {
                if (text.ToLower(CultureInfo.CurrentCulture).Trim() == "salir")
                {                    
                    userProfile.Dialogturn = "Salir";
                    return await innerDc.EndDialogAsync();
                }

                var validations = ValidateFlow(channel, text, datagas, innerDc.Context.Activity.Value);                    

                var validation = "";
                var button = "";                

                switch (validations)
                {
                    case "ContinueGas":
                        userProfile.Conversations.Last().Questions.Last().DataTreatmentGas = text;
                        return await innerDc.ContinueDialogAsync();
                    case "ContinueBrilla":
                        userProfile.Conversations.Last().Questions.Last().DataTreatmentBrilla = text;
                        return await innerDc.ContinueDialogAsync();
                    case "MessageGas":
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(MessagesTratamientodialog.RmGas), cancellationToken);
                        validation = ValidateMessage(company, MessagesTratamientodialog.TipeCarGas);
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(validation), cancellationToken: cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);
                    case "MessageBrilla":
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(MessagesTratamientodialog.RmBrilla), cancellationToken);
                        validation = ValidateMessage(company, MessagesTratamientodialog.TipeCardBrilla);
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(validation), cancellationToken: cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);
                    case "CardGas":
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(MessagesTratamientodialog.RcGas), cancellationToken);
                        validation = ValidateCard(company, MessagesTratamientodialog.TipeCarGas);
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(validation), cancellationToken: cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);                            
                    case "CardBrilla":
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(MessagesTratamientodialog.RcBrilla), cancellationToken);
                        validation = ValidateCard(company, MessagesTratamientodialog.TipeCardBrilla);
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(validation), cancellationToken: cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);
                    case "ContinueGasCard":
                        Dictionary<string, string> responseG = JsonConvert.DeserializeObject<Dictionary<string, string>>(innerDc.Context.Activity.Value.ToString());
                        button = responseG["boton"];
                        userProfile.Conversations.Last().Questions.Last().DataTreatmentGas = button.ToLower(CultureInfo.CurrentCulture);
                        return await innerDc.ContinueDialogAsync();                            
                    case "ContinueBrillaCard":
                        Dictionary<string, string> responseB = JsonConvert.DeserializeObject<Dictionary<string, string>>(innerDc.Context.Activity.Value.ToString());
                        button = responseB["boton"];
                        userProfile.Conversations.Last().Questions.Last().DataTreatmentBrilla = button.ToLower(CultureInfo.CurrentCulture);
                        return await innerDc.ContinueDialogAsync();                            
                    default:
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(MessagesTratamientodialog.ErrorTratamentData), cancellationToken: cancellationToken);                            
                        break;
                }                    
            }
                return await base.OnContinueDialogAsync(innerDc, cancellationToken);            
        }       

        public static string ValidateMessage (string company, string tipo)
        {
            string Message;
            
                if (tipo.Equals(MessagesTratamientodialog.TipeCarGas))
                {
                    Message = company switch
                    {
                        Utilities.MenuSubMenuEnum.CompanyName.Gdo => MessagesTratamientodialog.TratamientoDatosMessageGasGDO,
                        Utilities.MenuSubMenuEnum.CompanyName.Ceo => MessagesTratamientodialog.TratamientoDatosMessageEnergiaCEO,
                        Utilities.MenuSubMenuEnum.CompanyName.Stg => MessagesTratamientodialog.TratamientoDatosMessageGasSTG,
                        Utilities.MenuSubMenuEnum.CompanyName.Quavii => MessagesTratamientodialog.TratamientoDatosMessageGasQUAVII,
                        _ => MessagesTratamientodialog.ErrorTratamentData,
                    };
                }
                else
                {
                    Message = company switch
                    {
                        Utilities.MenuSubMenuEnum.CompanyName.Gdo => MessagesTratamientodialog.TratamientoDatosMessageBrillaGDO,
                        Utilities.MenuSubMenuEnum.CompanyName.Ceo => MessagesTratamientodialog.TratamientoDatosMessageBrillaCEO,
                        Utilities.MenuSubMenuEnum.CompanyName.Stg => MessagesTratamientodialog.TratamientoDatosMessageBrillaSTG,
                        Utilities.MenuSubMenuEnum.CompanyName.Quavii => MessagesTratamientodialog.TratamientoDatosMessageBrillaQUAVII,
                        _ => MessagesTratamientodialog.ErrorTratamentData,
                    };
                }
                
                return Message;                                                              
        }

        public static string ValidateCard(string company, string tipo)
        {
            string Message;

            if (tipo.Equals(MessagesTratamientodialog.TipeCarGas))
            {
                Message = company switch
                {
                    Utilities.MenuSubMenuEnum.CompanyName.Gdo => MessagesTratamientodialog.UseTratamientocardGasGDO,
                    Utilities.MenuSubMenuEnum.CompanyName.Ceo => MessagesTratamientodialog.UseTratamientocardEnergiaCEO,
                    Utilities.MenuSubMenuEnum.CompanyName.Stg => MessagesTratamientodialog.UseTratamientocardGasSTG,
                    Utilities.MenuSubMenuEnum.CompanyName.Quavii => MessagesTratamientodialog.UseTratamientocardGasQUAVII,
                    _ => MessagesTratamientodialog.ErrorTratamentData,
                };
            }
            else
            {
                Message = company switch
                {
                    Utilities.MenuSubMenuEnum.CompanyName.Gdo => MessagesTratamientodialog.UseTratamientocardBrillaGDO,
                    Utilities.MenuSubMenuEnum.CompanyName.Ceo => MessagesTratamientodialog.UseTratamientocardBrillaCEO,
                    Utilities.MenuSubMenuEnum.CompanyName.Stg => MessagesTratamientodialog.UseTratamientocardBrillaSTG,
                    Utilities.MenuSubMenuEnum.CompanyName.Quavii => MessagesTratamientodialog.UseTratamientocardBrillaQUAVII,
                    _ => MessagesTratamientodialog.ErrorTratamentData,
                };
            }

            return Message;
        }

        public static string ValidateFlow(string channel, string text, string datagas, object cardvalue)
        {
            string rta;

            if(text != null)            
                text = text.ToLower(CultureInfo.CurrentCulture);                       
            else           
                text = "";            
            

            if (Channels.Directline == channel)
            {
                rta = ValidateDirectline(text,datagas);
                return rta;
            }
            else
            {
                rta = ValidateRestofChannels(text,datagas,cardvalue);
                return rta;
            }                                   
        }

        public static string ValidateDirectline(string text, string datagas)
        {
            string rta;
            if (text == "1" || text == "2")
            {
                if (datagas == null)
                {
                    rta = "ContinueGas";
                    return rta;
                }
                else
                {
                    rta = "ContinueBrilla";
                    return rta;
                }
            }
            else
            {
                if (datagas == null)
                {
                    rta = "MessageGas";
                    return rta;
                }
                else
                {
                    rta = "MessageBrilla";
                    return rta;
                }
            }
        }

        public static string ValidateRestofChannels(string text, string datagas, object cardvalue)
        {
            string rta;
            if (cardvalue == null)
            {
                rta = ValidateMessageCard(text, datagas);
                return rta;
            }
            else
            {
                if (datagas == null)
                {
                    rta = "ContinueGasCard";
                    return rta;
                }
                else
                {
                    rta = "ContinueBrillaCard";
                    return rta;
                }
            }
        }

        public static string ValidateMessageCard(string text, string datagas)
        {
            string rta;
            if (text == "si" || text == "no")
            {
                if (datagas == null)
                {
                    rta = "ContinueGas";
                    return rta;
                }
                else
                {
                    rta = "ContinueBrilla";
                    return rta;
                }
            }
            else
            {
                if (datagas == null)
                {
                    rta = "CardGas";
                    return rta;
                }
                else
                {
                    rta = "CardBrilla";
                    return rta;
                }
            }
        }

        public static class MessagesTratamientodialog
        {
            public const string RmGas = "RetryMessagesTratamientoDatosGas";
            public const string RmBrilla = "RetryMessagesTratamientoDatosBrilla";
            public const string RcGas = "RetryCardTratamientoDatosGas";
            public const string RcBrilla = "RetryCardTratamientoDatosBrilla";
            public const string TipeCarGas = "Gas";
            public const string TipeCardBrilla = "Brilla";
            public const string TratamientoDatosMessageGasGDO = "TratamientoDatosMessageGasGDO";
            public const string TratamientoDatosMessageEnergiaCEO = "TratamientoDatosMessageEnergiaCEO";
            public const string TratamientoDatosMessageGasSTG = "TratamientoDatosMessageGasSTG";
            public const string TratamientoDatosMessageGasQUAVII = "TratamientoDatosMessageGasQUAVII";
            public const string TratamientoDatosMessageBrillaGDO = "TratamientoDatosMessageBrillaGDO";
            public const string TratamientoDatosMessageBrillaCEO = "TratamientoDatosMessageBrillaCEO";
            public const string TratamientoDatosMessageBrillaSTG = "TratamientoDatosMessageBrillaSTG";
            public const string TratamientoDatosMessageBrillaQUAVII = "TratamientoDatosMessageBrillaQUAVII";
            public const string UseTratamientocardGasGDO = "UseTratamientocardGasGDO";
            public const string UseTratamientocardEnergiaCEO = "UseTratamientocardEnergiaCEO";
            public const string UseTratamientocardGasSTG = "UseTratamientocardGasSTG";
            public const string UseTratamientocardGasQUAVII = "UseTratamientocardGasQUAVII";
            public const string UseTratamientocardBrillaGDO = "UseTratamientocardBrillaGDO";
            public const string UseTratamientocardBrillaCEO = "UseTratamientocardBrillaCEO";
            public const string UseTratamientocardBrillaSTG = "UseTratamientocardBrillaSTG";
            public const string UseTratamientocardBrillaQUAVII = "UseTratamientocardBrillaQUAVII";
            public const string ErrorTratamentData = "ErrorTratamentData";                        
        };
    }
}
