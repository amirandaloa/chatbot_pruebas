
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

namespace EnlaceVA.Dialogs
{
    public class ConstaciaPagoDialog : ComponentDialog
    {      
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly TransactionalServices _transactionalServices;
        


        public ConstaciaPagoDialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(ConstaciaPagoDialog))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();            
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            _transactionalServices = serviceProvider.GetService<TransactionalServices>();
      
            var ConstaciaPagoSteps = new WaterfallStep[]
            {
                StarDialogAsync,
                GetNameAsync,
                GetEmailAsync,
                GetPhoneAsync,
                GetBillValueAsync,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(ConstaciaPagoDialog), ConstaciaPagoSteps));            
        }

        private async Task<DialogTurnResult> StarDialogAsync (WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "ConstaciaPagoDialog";
            await _userState.SaveChangesAsync(stepContext.Context, true);

            await stepContext.Context.SendActivityAsync("Si necesitas realizar un pago anticipado/prepago de tu factura, ingresa el número de contrato", cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);

        }

        private async Task<DialogTurnResult> GetNameAsync (WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            await stepContext.Context.SendActivityAsync("Por favor digite su nombre", cancellationToken: cancellationToken);
            return new DialogTurnResult(DialogTurnStatus.Waiting);

        }
        private async Task<DialogTurnResult> GetEmailAsync (WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            await stepContext.Context.SendActivityAsync("Por favor digite su correo electronico", cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);

        }
        private async Task<DialogTurnResult> GetPhoneAsync (WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            await stepContext.Context.SendActivityAsync("Por favor digite un numero celular", cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);

        }

        private async Task<DialogTurnResult> GetBillValueAsync (WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            await stepContext.Context.SendActivityAsync("Por favor digite el valor para el cupon", cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);

        }


        private async Task<DialogTurnResult> FinishDialogAsync (WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());


            userProfile.Dialogturn = null;
            
            await _userState.SaveChangesAsync(stepContext.Context, true);
            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("AbonaatufacturaTemporalMessage"));
            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"));

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
        
    }
}
