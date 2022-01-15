

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

    public class ExencionContribucionDialog : ComponentDialog
    {
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;


        public ExencionContribucionDialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(ExencionContribucionDialog))
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
            userProfile.Dialogturn = "ExencionContribucionDialog";
            await _userState.SaveChangesAsync(stepContext.Context, true);
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

            var validation = ValidateMessage(company, userProfile.ChannelId);
           
            if (validation == "WhatsappReturningExencion_Contribucion" || validation == "ReturningExencion_Contribucion")
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(validation), cancellationToken: cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(validation));
                return await stepContext.NextAsync();
            }            
        }

        private async Task<DialogTurnResult> FinishDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = null;

            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"), cancellationToken: cancellationToken);
            return await stepContext.EndDialogAsync();
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            if (innerDc != null)
            {
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

                string validation = ValidateData(text);
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

        public static string ValidateData(string text)
        {
            if (text != null)
            {
                var question = text.ToLower(CultureInfo.CurrentCulture);

                if (question.Contains("comercial e industria regulada", StringComparison.OrdinalIgnoreCase) || question.Equals("1")
                || question.Contains("Comercial e industria"))
                {
                    return "ResponseComercial_e_industria_reguladaGdo";
                }
                else if (question.Contains("industria no regulada") || question.Equals("2"))
                {
                    return "ResponseIndustria_no_reguladaGdo";
                }
                else if (question.Contains("zonas comunes") || question.Equals("3"))
                {
                    return "ResponseZonas_comunesGdo";
                }
                else if (text.ToLower().Equals("salir"))
                {
                    return "salir";
                }
                else
                {
                    return "Error";
                }
            }
            else {
                return "Error";
            }
        }

        public static string ValidateMessage(string company, string channel)
        {
            string Message;
            if (Channels.Directline == channel)
            {
                Message = "WhatsappReturningExencion_Contribucion";                
                return Message;
            }
            else
            {
                Message = company switch
                {
                    Utilities.MenuSubMenuEnum.CompanyName.Gdo => "ReturningExencion_Contribucion",
                    Utilities.MenuSubMenuEnum.CompanyName.Ceo => "ResponseExencion_de_contribucionCeo",
                    Utilities.MenuSubMenuEnum.CompanyName.Stg => "ResponseExencion_de_contribucionStg",
                    Utilities.MenuSubMenuEnum.CompanyName.Quavii => "ResponseExencion_de_contribucionQuavii",
                    _ => ""
                };
                return Message;                
            }
                
        }
    }
}