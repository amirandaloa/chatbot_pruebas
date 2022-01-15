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

    public class TerminacionContratoDialog : ComponentDialog
    {
        private readonly BotState _userStateTerminacionContrato;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessorTerminacionContrato;


        public TerminacionContratoDialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(TerminacionContratoDialog))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();
            _userStateTerminacionContrato = serviceProvider.GetService<UserState>();
            _accessorTerminacionContrato = _userStateTerminacionContrato.CreateProperty<UserProfileState>(nameof(UserProfileState));

            var TerminacionContratoSteps = new WaterfallStep[]
            {
                StarDialogContractAsync,
                FinishDialogContractAsync
            };
            AddDialog(new WaterfallDialog(nameof(TerminacionContratoDialog), TerminacionContratoSteps));
            AddDialog(new TextPrompt(DialogIds.NamePrompt));
        }

        private async Task<DialogTurnResult> StarDialogContractAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessorTerminacionContrato.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "TerminacionContratoDialog";
            await _userStateTerminacionContrato.SaveChangesAsync(stepContext.Context, true);
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);
            switch (company)
            {
                case Utilities.MenuSubMenuEnum.CompanyName.Gdo:
                    {
                        if (userProfile.ChannelId == Channels.Directline)
                        {
                            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("WhatsappReturningTerminacion_contrato"), cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ReturningTerminacion_contrato"));
                        }
                        return new DialogTurnResult(DialogTurnStatus.Waiting);
                    }
                case Utilities.MenuSubMenuEnum.CompanyName.Ceo:
                    {
                        await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ResponseTerminacion_contratoComercialCeo"));
                        return await stepContext.NextAsync();
                    }
                case Utilities.MenuSubMenuEnum.CompanyName.Stg:
                    {
                        await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ResponseTerminacion_contratoComercialStg"));
                        return await stepContext.NextAsync();
                    }
                case Utilities.MenuSubMenuEnum.CompanyName.Quavii:
                    {
                        await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ResponseTerminacion_contratoComercialQuavii"));
                        return await stepContext.NextAsync();
                    }
                default:
                    return await stepContext.NextAsync();
            }
        }

        private async Task<DialogTurnResult> FinishDialogContractAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessorTerminacionContrato.GetAsync(stepContext.Context, () => new UserProfileState());
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
                if (question.Contains("residencial", StringComparison.OrdinalIgnoreCase))
                {
                    return "ResponseTerminacion_contratoResidencialGdo";
                }
                else if (question.Contains("comercial", StringComparison.OrdinalIgnoreCase))
                {
                    return "ResponseTerminacion_contratoComercialGdo";
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
            else
            {
                return "Error";
            }
        }

        private class DialogIds
        {
            public const string NamePrompt = "NamePrompt";
            protected DialogIds()
            {
            }
        }
    }
}