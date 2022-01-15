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
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using static EnlaceVA.Utilities.DataValidations;
using System.Resources;
using EnlaceVA.Resources;

namespace EnlaceVA.Dialogs
{
    public class ConsultaPQRdialog : ComponentDialog
    {
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateManager;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly TransactionalServices _transactionalServices;

        public ConsultaPQRdialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(ConsultaPQRdialog))
        {
            _templateManager = serviceProvider.GetService<LocaleTemplateManager>();
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            _transactionalServices = serviceProvider.GetService<TransactionalServices>();

            var ConsultaPqrsSteps = new WaterfallStep[]
            {
                GetRequestAsync,                
                SendData,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(ConsultaPQRdialog), ConsultaPqrsSteps));
        }

        private async Task<DialogTurnResult> GetRequestAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "ConsultaPQRdialog";
            userProfile.Conversations.Last().Questions.Last().generalInformation = new GeneralInformation();

            await _userState.SaveChangesAsync(stepContext.Context, true);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ObtainRequest"), cancellationToken: cancellationToken);            

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
        
        private async Task<DialogTurnResult> SendData(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            await _userState.SaveChangesAsync(stepContext.Context, true);

            var information = userProfile.Conversations.Last().Questions.Last().generalInformation;
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("Startservice"), cancellationToken: cancellationToken);

            DatosEntradaSolicitud conceptoFacturaEntrada = new DatosEntradaSolicitud()
            {
                solicitud = information.solicitud,
                tipoIdentificacion = "1",
                identificacion = Utilities.Utility.GetIdCompany(company),
                cliente = information.cliente,
                correo = information.correo,
                telefono = information.telefono
            };

            var result = _transactionalServices.GetConsultaPQR(company, conceptoFacturaEntrada);
            result.solicitud = information.solicitud;
            if (result.codError == "0")
            {
                foreach (var item in result.descEstado)
                {
                    if (item.codigo_estado_sol == "36")
                    {
                        await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(String.Format("ConsultaPQRanulado{0}", company.ToUpper()), item), cancellationToken: cancellationToken);
                    }
                    else if (item.codigo_estado_sol == "32")
                    {
                        await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(String.Format("ConsultaPQRanulado{0}", company.ToUpper()), item), cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ConsultaPQRSinAtenderResultGDO", result.descEstado.FirstOrDefault()), cancellationToken: cancellationToken);
                        //await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(String.Format("ConsultaPQRSinAtenderResult{0}", company.ToUpper()), item), cancellationToken: cancellationToken);
                    }
                }
                
            }
            else if (result.codError == "-7")
            {
                await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(String.Format("ConsultaPQRNoEncontrado{0}", company.ToUpper()), result), cancellationToken: cancellationToken);
            }
            else
            {
                if (result.codError== "Server Error")
                {
                    await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ServerError"), cancellationToken: cancellationToken);
                }
                else 
                {
                    await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(userProfile.ContactMessage), cancellationToken: cancellationToken);
                }
                
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> FinishDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage"));
            userProfile.Dialogturn = null;
            await _userState.SaveChangesAsync(stepContext.Context, true);

            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var userProfile = await _accessor.GetAsync(innerDc.Context, () => new UserProfileState());

            var validation = "";
            var text = "";
            if (userProfile.Dialogturn == "ConsultaPQRdialog")
            {
                text = innerDc.Context.Activity.Text.ToLower(CultureInfo.CurrentCulture);

                if (text.ToLower(CultureInfo.CurrentCulture).Trim() == "salir")
                {
                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage"));
                    userProfile.Dialogturn = null;
                    return await innerDc.EndDialogAsync();
                }

                //El campo contrato debe tomar el valor de la solicitud
                var GetInformation = userProfile.Conversations.Last().Questions.Last().generalInformation;
                GetInformation.company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

                validation = ValidateData(text, GetInformation);

                switch (validation)
                {

                    case MessagesCertificadoFacturaDialog.TextSolicitud:
                        GetInformation.solicitud = text;
                        return await innerDc.ContinueDialogAsync();
                    case MessagesCertificadoFacturaDialog.Error:
                        await innerDc.Context.SendActivityAsync(validation);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);
                    default:
                        await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ErrorMessage"));
                        userProfile.Dialogturn = null;
                        return await innerDc.EndDialogAsync();
                }
            }
            else
            {
                return await innerDc.ContinueDialogAsync();
            }
        }

        public static string ValidateData(string data, GeneralInformation information)
        {
            string message;

            if (information.solicitud == null)
            {
                message = ValidateRequest(data);
            }
            else
            {
                message = "Error";
            }

            return message;
        }
    }
}
