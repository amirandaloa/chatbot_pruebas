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
using static EnlaceVA.Utilities.DataValidations;
using System.Linq;
using System.Globalization;

namespace EnlaceVA.Dialogs
{
    public class ConsultaCupoBrillaDialog : ComponentDialog
    {
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly TransactionalServices _transactionalServices;



        public ConsultaCupoBrillaDialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(ConsultaCupoBrillaDialog))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            _transactionalServices = serviceProvider.GetService<TransactionalServices>();

            var ConstaciaPagoSteps = new WaterfallStep[]
            {
                GetContractAsync,
                CheckContractAsync,               
                SendDataAsync,                             
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(ConsultaCupoBrillaDialog), ConstaciaPagoSteps));
        }

        private async Task<DialogTurnResult> GetContractAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
                        
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "ConsultaCupoBrillaDialog";
            userProfile.Conversations.Last().Questions.Last().generalInformation = new GeneralInformation();            

            await _userState.SaveChangesAsync(stepContext.Context, true);

            string url = ValidateCompany(userProfile.Company);

            Activity message = _templateEngine.GenerateActivityForLocale("Obtaincontract");

            message.Attachments = new List<Attachment> {
            new Attachment(){
                    ContentUrl = url,
                    ContentType = "image/jpg",
                    Name = "datauri" }};


            await stepContext.Context.SendActivityAsync(message, cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);

        }

        private async Task<DialogTurnResult> CheckContractAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            var GetInformation = userProfile.Conversations.Last().Questions.Last().generalInformation;
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

            DatosEntradaValidarContrato datosEntrada = new DatosEntradaValidarContrato
            {
                contrato = GetInformation.contrato
            };

            var result = _transactionalServices.MTValidarcontrato(company, datosEntrada);

            if (result.mensajes.CodMsj == "0")
            {
                return await stepContext.NextAsync();
            }
            else
            {
                userProfile.countFailContract += 1;
                if (userProfile.countFailContract == 2)
                {
                    userProfile.Dialogturn = null;
                    userProfile.countFailContract = 0;
                    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ErrorContratoNoEncontrado"), cancellationToken: cancellationToken);
                    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"), cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync();
                }
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ErrorContrato"), cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync("ConsultaCupoBrillaDialog");
            }
        }

        private async Task<DialogTurnResult> SendDataAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            await _userState.SaveChangesAsync(stepContext.Context, true);

            var GetInformation = userProfile.Conversations.Last().Questions.Last().generalInformation;
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("Startservice"), cancellationToken: cancellationToken);

            DatosEntrada datosEntrada = new DatosEntrada
            {
                contrato = GetInformation.contrato
            };
                                                
            var result = _transactionalServices.GetConsultaCupoBrilla(company, datosEntrada);
            
            if (result.estado == "0" && result.cupo_disponible != "0")
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ConsultaCupoBrilla",result), cancellationToken: cancellationToken);
            }
            else if (result.estado == "0" && result.cupo_disponible.Equals("0"))
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ConsultaCupoBrillaError"), cancellationToken: cancellationToken);
            }            
            else
            {
                if (result.estado == "Server Error")
                {
                    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ServerError"), cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ErrorResult", result.mensajes), cancellationToken: cancellationToken);                    
                }
            }

            return await stepContext.NextAsync();
        }


        private async Task<DialogTurnResult> FinishDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {           
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"));
            userProfile.Dialogturn = null;
            await _userState.SaveChangesAsync(stepContext.Context, true);

            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var userProfile = await _accessor.GetAsync(innerDc.Context, () => new UserProfileState());

            var validation = "";
            var text = "";

            if (userProfile.Dialogturn == "ConsultaCupoBrillaDialog")
            {
                text = innerDc.Context.Activity.Text.ToLower(CultureInfo.CurrentCulture);

                if (text.ToLower(CultureInfo.CurrentCulture).Trim() == "salir")
                {
                    await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"));
                    userProfile.Dialogturn = null;
                    userProfile.countFailContract = 0;
                    return await innerDc.EndDialogAsync();
                }

                var GetInformation = userProfile.Conversations.Last().Questions.Last().generalInformation;
                GetInformation.company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

                validation = ValidateData(text, GetInformation);

                switch (validation)
                {
                    case MessagesCertificadoFacturaDialog.TextContrato:
                        GetInformation.contrato = text;
                        return await innerDc.ContinueDialogAsync();
                    case MessagesCertificadoFacturaDialog.TextTipoId:
                        GetInformation.tipoIdentificacion = ValidateCompanyForTypeId(Convert.ToInt32(text), GetInformation.company);
                        return await innerDc.ContinueDialogAsync();
                    case MessagesCertificadoFacturaDialog.TextId:
                        GetInformation.identificacion = text;
                        return await innerDc.ContinueDialogAsync();
                    case MessagesCertificadoFacturaDialog.TextName:
                        GetInformation.cliente = text;
                        return await innerDc.ContinueDialogAsync();
                    case MessagesCertificadoFacturaDialog.TextEmail:
                        GetInformation.correo = text;
                        return await innerDc.ContinueDialogAsync();
                    case MessagesCertificadoFacturaDialog.TextPhone:
                        GetInformation.telefono = text;
                        return await innerDc.ContinueDialogAsync();
                    case MessagesCertificadoFacturaDialog.Error:
                        await innerDc.Context.SendActivityAsync(validation);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);
                    default:
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ErrorMessage"));
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

            if (information.contrato == null)
            {
                message = ValidateContract(data);
            }
            else if (information.tipoIdentificacion == null)
            {
                message = ValidateTypeId(data, information.company);
            }
            else if (information.identificacion == null)
            {
                message = ValidateId(data);
            }
            else if (information.cliente == null)
            {
                message = Validatename(data);
            }
            else if (information.correo == null)
            {
                message = ValidateEmail(data);
            }
            else if (information.telefono == null)
            {
                message = ValidatePhone(data);
            }
            else
            {
                message = "Error";
            }

            return message;
        }        

    }
}
