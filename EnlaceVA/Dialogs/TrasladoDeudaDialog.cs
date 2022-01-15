
namespace EnlaceVA.Dialogs
{
    using EnlaceVA.Models;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Solutions.Responses;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Bot.Connector;
    using Newtonsoft.Json;
    using System.Globalization;

    public class TrasladoDeudaDialog : ComponentDialog
    {

        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;


        public TrasladoDeudaDialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(TrasladoDeudaDialog))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));

            var ConstaciaPagoSteps = new WaterfallStep[]
            {
                StarDialogAsync,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(ConstaciaPagoDialog), ConstaciaPagoSteps));            
        }

        private async Task<DialogTurnResult> StarDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "TrasladoDeudaDialog";
            await _userState.SaveChangesAsync(stepContext.Context, true);

            GeneralInformation information = new GeneralInformation();
            information.service = "gas";

            if (userProfile.Company.ToLower(CultureInfo.CurrentCulture) == Utilities.MenuSubMenuEnum.CompanyName.Ceo)
            {
                information.service = "energia";
            }

            if (userProfile.ChannelId == Channels.Directline)
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(MessagesTrasladoDeuda.WrDeuda,information), cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(MessagesTrasladoDeuda.ReturnDeuda,information), cancellationToken: cancellationToken);
            }
            return new DialogTurnResult(DialogTurnStatus.Waiting);

        }

        private async Task<DialogTurnResult> FinishDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = null;
            await _userState.SaveChangesAsync(stepContext.Context, true);
            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(MessagesTrasladoDeuda.CompletedMessage), cancellationToken:cancellationToken);
            return await stepContext.EndDialogAsync();
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            if (innerDc != null)
            {
                var userProfile = await _accessor.GetAsync(innerDc.Context, () => new UserProfileState());
                string text;
                if (innerDc.Context.Activity.Value != null)
                {
                    Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(innerDc.Context.Activity.Value.ToString());
                    text = response["boton"];
                }
                else
                {
                    text = innerDc.Context.Activity.Text;
                }

                string validation = ValidateData(text, userProfile.Company);
                if (validation != Utilities.MenuSubMenuEnum.Menu.Error)
                {
                    if (validation == "salir")
                    {
                        return await innerDc.ContinueDialogAsync();
                    }
                    await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(validation));
                    return await innerDc.ContinueDialogAsync();
                }
                else
                {
                    await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ErrorMenu"));
                    return new DialogTurnResult(DialogTurnStatus.Waiting);
                }
            }
            else
            {
                return EndOfTurn;
            }
        }

        public static string ValidateData(string text, string company)
        {
            string SendMessage = "";
            if (text != null)
            {
                var question = text.ToLower(CultureInfo.CurrentCulture);
                
                if (question.Contains(MessagesTrasladoDeuda.DesDeudaBrilla, StringComparison.OrdinalIgnoreCase) || question.Equals(MessagesTrasladoDeuda.Option1)
                    || question.Contains(MessagesTrasladoDeuda.DesmonteBrilla, StringComparison.OrdinalIgnoreCase))
                {
                    SendMessage = ResponseDesmontedeudaBrilla(company);
                    return SendMessage;
                }
                else if (question.Contains(MessagesTrasladoDeuda.TrasladoDeudaBrilla, StringComparison.OrdinalIgnoreCase) || question.Equals(MessagesTrasladoDeuda.Option2)
                    || question.Contains(MessagesTrasladoDeuda.TrasladoBrilla, StringComparison.OrdinalIgnoreCase))
                {
                    SendMessage = DesmonteBrilla(company);
                    return SendMessage;
                }
                else if (question.Contains(MessagesTrasladoDeuda.TrasladoDeudaGas, StringComparison.OrdinalIgnoreCase) || question.Equals(MessagesTrasladoDeuda.Option3)
                  || question.Contains(MessagesTrasladoDeuda.TrasladoGas, StringComparison.OrdinalIgnoreCase))
                {
                    SendMessage = TrasladoBrilla(company);
                    return SendMessage;
                }
                else if (question.ToLower().Equals("salir"))
                {
                    return "salir";
                }
                else
                {
                    return MessagesTrasladoDeuda.ErrorMessage;
                }
            }
            else
            {
                return MessagesTrasladoDeuda.ErrorMessage;
            }
        }

        public static string ResponseDesmontedeudaBrilla(string companyvalidate)
        {
            string message;
            companyvalidate = companyvalidate.ToLower(CultureInfo.CurrentCulture);
            message = companyvalidate switch
            {
                Utilities.MenuSubMenuEnum.CompanyName.Gdo => MessagesTrasladoDeuda.DesmonteDeudaBrillaGdo,
                Utilities.MenuSubMenuEnum.CompanyName.Ceo => MessagesTrasladoDeuda.DesmonteDeudaBrillaCeo,
                Utilities.MenuSubMenuEnum.CompanyName.Stg => MessagesTrasladoDeuda.DesmonteDeudaBrillaStg,
                Utilities.MenuSubMenuEnum.CompanyName.Quavii => MessagesTrasladoDeuda.DesmonteDeudaBrillaQuavii,
                _ => MessagesTrasladoDeuda.ErrorMessage,
            };

            return message;
        }
        public static string DesmonteBrilla(string companyvalidate)
        {
            string message;
            companyvalidate = companyvalidate.ToLower(CultureInfo.CurrentCulture);
            message = companyvalidate switch
            {
                Utilities.MenuSubMenuEnum.CompanyName.Gdo => MessagesTrasladoDeuda.DesmonteBrillaGdo,
                Utilities.MenuSubMenuEnum.CompanyName.Ceo => MessagesTrasladoDeuda.DesmonteBrillaCeo,
                Utilities.MenuSubMenuEnum.CompanyName.Stg => MessagesTrasladoDeuda.DesmonteBrillaStg,
                Utilities.MenuSubMenuEnum.CompanyName.Quavii => MessagesTrasladoDeuda.DesmonteBrillaQuavii,
                _ => MessagesTrasladoDeuda.ErrorMessage,
            };

            return message;
        }
        public static string TrasladoBrilla(string companyvalidate)
        {
            string message;
            companyvalidate = companyvalidate.ToLower(CultureInfo.CurrentCulture);
            message = companyvalidate switch
            {
                Utilities.MenuSubMenuEnum.CompanyName.Gdo => MessagesTrasladoDeuda.TrasladoBrillaGdo,
                Utilities.MenuSubMenuEnum.CompanyName.Ceo => MessagesTrasladoDeuda.TrasladoBrillaCeo,
                Utilities.MenuSubMenuEnum.CompanyName.Stg => MessagesTrasladoDeuda.TrasladoBrillaStg,
                Utilities.MenuSubMenuEnum.CompanyName.Quavii => MessagesTrasladoDeuda.TrasladoBrillaQuavii,
                _ => MessagesTrasladoDeuda.ErrorMessage,
            };

            return message;
        }

        public class MessagesTrasladoDeuda
        {
            public static readonly string WrDeuda = "WhatsappReturningTraslado_Deuda";
            public static readonly string ReturnDeuda = "ReturningTraslado_Deuda";
            public static readonly string CompletedMessage = "CompletedMessage";
            public static readonly string Option1 = "1";
            public static readonly string Option2 = "2";
            public static readonly string Option3 = "3";
            public static readonly string DesDeudaBrilla = "desmonte de deuda brilla";
            public static readonly string DesmonteBrilla = "desmonte brilla";
            public static readonly string TrasladoDeudaBrilla = "traslado deuda brilla";
            public static readonly string TrasladoBrilla = "traslado brilla";
            public static readonly string TrasladoDeudaGas = "traslado deuda gas";
            public static readonly string TrasladoGas = "traslado gas";
            public static readonly string DesmonteDeudaBrillaGdo = "ResponseDesmonte_de_deuda_brillaGdo";
            public static readonly string DesmonteDeudaBrillaCeo = "ResponseDesmonte_de_deuda_brillaCeo";
            public static readonly string DesmonteDeudaBrillaStg = "ResponseDesmonte_de_deuda_brillaStg";
            public static readonly string DesmonteDeudaBrillaQuavii = "ResponseDesmonte_de_deuda_brillaQuavii";
            public static readonly string DesmonteBrillaGdo = "ResponseTraslado_deuda_brillaGdo";
            public static readonly string DesmonteBrillaCeo = "ResponseDesmonte_de_deuda_brillaCeo";
            public static readonly string DesmonteBrillaStg = "ResponseDesmonte_de_deuda_brillaStg";
            public static readonly string DesmonteBrillaQuavii = "ResponseDesmonte_de_deuda_brillaQuavii";
            public static readonly string TrasladoBrillaGdo = "ResponseTraslado_deuda_gasGdo";
            public static readonly string TrasladoBrillaCeo = "ResponseTraslado_deuda_gasCeo";
            public static readonly string TrasladoBrillaStg = "ResponseTraslado_deuda_gasStg";
            public static readonly string TrasladoBrillaQuavii = "ResponseTraslado_deuda_gasQuavii";
            public static readonly string ErrorMessage = "Error";


            protected MessagesTrasladoDeuda()
            {

            }
        };
    }
}