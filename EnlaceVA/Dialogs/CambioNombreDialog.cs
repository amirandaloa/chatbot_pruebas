
using EnlaceVA.Models;
using EnlaceVA.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Responses;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace EnlaceVA.Dialogs
{
    public class CambioNombreDialog : ComponentDialog
    {
        private readonly BotServices _services;
        private readonly BotSettings _settings;
        private readonly BotState _conversationState;
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly IStatePropertyAccessor<List<Activity>> _previousResponseAccessor;

        public CambioNombreDialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(CambioNombreDialog))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();

            _conversationState = serviceProvider.GetService<ConversationState>();
            _userState = serviceProvider.GetService<UserState>();

            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            _services = serviceProvider.GetService<BotServices>();
            _settings = serviceProvider.GetService<BotSettings>();

            var conversationState = serviceProvider.GetService<ConversationState>();
            _previousResponseAccessor = conversationState.CreateProperty<List<Activity>>(StateProperties.PreviousBotResponse);

            var CambioNombreSteps = new WaterfallStep[]
            {
                StarDialog,
                FinishDialog
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(CambioNombreDialog), CambioNombreSteps));
            AddDialog(new TextPrompt(DialogIds.NamePrompt));
        }

        private async Task<DialogTurnResult> StarDialog(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "CambioNombreDialog";
            await _userState.SaveChangesAsync(stepContext.Context, true);

            switch (userProfile.Company.ToLower())
            {
                case Utilities.MenuSubMenuEnum.CompanyName.Gdo:
                    {
                        if (userProfile.ChannelId == Channels.Directline)
                        {
                            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("WhatsappReturningCambio_Nombre"));
                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ReturningCambio_Nombre"), cancellationToken: cancellationToken);
                        }
                        return new DialogTurnResult(DialogTurnStatus.Waiting);
                    }
                case Utilities.MenuSubMenuEnum.CompanyName.Ceo:
                    {
                        if (userProfile.ChannelId == Channels.Directline)
                        {
                            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("WhatsappReturningCambio_Nombre"));
                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ReturningCambio_Nombre"));
                        }
                        return new DialogTurnResult(DialogTurnStatus.Waiting);
                    }
                case Utilities.MenuSubMenuEnum.CompanyName.Stg:
                    {
                        await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ResponseCambio_NombreStg"));
                        return await stepContext.ContinueDialogAsync();
                    }
                case Utilities.MenuSubMenuEnum.CompanyName.Quavii:
                    {
                        await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ResponseCambio_NombreStg"));
                        return await stepContext.ContinueDialogAsync();
                    }
                default:
                    return await stepContext.NextAsync();
            }
        }

        private async Task<DialogTurnResult> FinishDialog(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = null;
            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"));
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var userProfile = await _accessor.GetAsync(innerDc.Context, () => new UserProfileState());
            var text = "";
            if (innerDc.Context.Activity.Value != null)
            {
                Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(innerDc.Context.Activity.Value.ToString());
                text = response["boton"];
            }
            else
            {
                text = innerDc.Context.Activity.Text;
            }

            string validation = ValidateData(text,userProfile.Company.ToLower());
            if (validation != "Error")
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



        public string ValidateData(string text, string company)
        {
            if (company=="gdo")
            {
                if (text.ToLower().Contains("residencial") || text.Equals("1")
                    || text.ToLower().Contains("Comercial e industria"))
                {
                    return "ResponseCambio_Nombre_ResidencialGdo";
                }
                else if (text.ToLower().Contains("comercial") || text.Equals("2"))
                {
                    return "ResponseCambio_Nombre_ComercialGdo";
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
                if (text.ToLower().Contains("residencial") || text.Equals("1")
                   || text.ToLower().Contains("Comercial e industria"))
                {
                    return "ResponseCambio_Nombre_ResidencialCeo";
                }
                else if (text.ToLower().Contains("comercial") || text.Equals("2"))
                {
                    return "ResponseCambio_Nombre_ComercialCeo";
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
            
        }

        private class DialogIds
        {
            public const string NamePrompt = "NamePrompt";
        }
        private class StateProperties
        {
            public const string GeneralResult = "generalResult";
            public const string PreviousBotResponse = "previousBotResponse";
        }
    }
}