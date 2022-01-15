
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
using System.Globalization;

namespace EnlaceVA.Dialogs
{
    public class EmergenciasDialog : ComponentDialog
    {        
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;        

        public EmergenciasDialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(EmergenciasDialog))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();            
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));

            var CambioNombreSteps = new WaterfallStep[]
            {
                StarDialogAsync,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(CambioNombreDialog), CambioNombreSteps));            
        }

        private async Task<DialogTurnResult> StarDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "EmergenciasDialog";
            await _userState.SaveChangesAsync(stepContext.Context, true);

            string company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

            var validation = ValidateMessage(company);

            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(validation), cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync();            
        }

        private async Task<DialogTurnResult> FinishDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = null;
            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"));
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        public static string ValidateMessage(string company)
        {
            string Message;

            Message = company switch
            {
                Utilities.MenuSubMenuEnum.CompanyName.Gdo => "ResponseEmergenciasGdo",
                Utilities.MenuSubMenuEnum.CompanyName.Ceo => "ResponseTemporalEmergenciasQuavii",
                Utilities.MenuSubMenuEnum.CompanyName.Stg => "ResponseEmergenciasStg",
                Utilities.MenuSubMenuEnum.CompanyName.Quavii => "ResponseTemporalEmergenciasQuavii",
                _ => ""
            };
            return Message;
        }
    }
}