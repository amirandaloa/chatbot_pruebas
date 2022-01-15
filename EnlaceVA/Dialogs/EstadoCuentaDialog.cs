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
using System.Linq;
using static EnlaceVA.Utilities.DataValidations;
using static EnlaceVA.Utilities.Utility;
using System.Globalization;

namespace EnlaceVA.Dialogs
{
    public class EstadoCuentaDialog : ComponentDialog
    {
        private readonly BotState _userState;        
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly LocaleTemplateManager _templateManager;
        private readonly TransactionalServices _transactionalServices;


        public EstadoCuentaDialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(EstadoCuentaDialog))
        {
            _templateManager = serviceProvider.GetService<LocaleTemplateManager>();
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            
            _transactionalServices = serviceProvider.GetService<TransactionalServices>();

            var EstadoCuentaSteps = new WaterfallStep[]
            {
                InitGetContractAsync,
                ValidateContractAsync,
                GetTypeIdAsync,
                GetIdNumberAsync,
                GetFullNameAsync,
                GetEmailAsync,
                GetContactPhoneAsync,
                ProcessDataAsync,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(EstadoCuentaDialog), EstadoCuentaSteps));
        }

        private async Task<DialogTurnResult> InitGetContractAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "EstadoCuentaDialog";
            userProfile.Conversations.Last().Questions.Last().generalInformation = new GeneralInformation();

            await _userState.SaveChangesAsync(stepContext.Context, true);

            string url = ValidateCompany(userProfile.Company);

            Activity message = _templateManager.GenerateActivityForLocale("Obtaincontract");

            message.Attachments = new List<Attachment> {
            new Attachment(){
                    ContentUrl = url,
                    ContentType = "image/jpg",
                    Name = "datauri" }};


            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("Introductorymessage"), cancellationToken: cancellationToken);
            await Task.Delay(3000).ContinueWith(t =>
            {
                stepContext.Context.SendActivityAsync(message, cancellationToken: cancellationToken);
            });

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> ValidateContractAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
                return await stepContext.BeginDialogAsync(nameof(TratamientoDialog), null, cancellationToken);
            }
            else
            {
                userProfile.countFailContract += 1;
                if (userProfile.countFailContract == 2)
                {
                    userProfile.Dialogturn = null;
                    userProfile.countFailContract = 0;                    
                    await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ErrorContratoNoEncontrado"), cancellationToken: cancellationToken);
                    await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage"), cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync();
                }
                await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ErrorContrato"), cancellationToken: cancellationToken);

                return await stepContext.ReplaceDialogAsync("EstadoCuentaDialog");
            }
        }

        private async Task<DialogTurnResult> GetTypeIdAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());

            if (userProfile.Dialogturn != "Salir")
            {
                userProfile.Dialogturn = "EstadoCuentaDialog";
                await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("Introductorymessage"), cancellationToken: cancellationToken);
                await _userState.SaveChangesAsync(stepContext.Context, true);

                var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

                await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(Utilities.Utility.GetTypeIdByCompany(company)), cancellationToken: cancellationToken);

                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage"));
                userProfile.Dialogturn = null;
                return await stepContext.EndDialogAsync();
            }
        }

        private async Task<DialogTurnResult> GetIdNumberAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await _userState.SaveChangesAsync(stepContext.Context, true);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("GetId"), cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> GetFullNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await _userState.SaveChangesAsync(stepContext.Context, true);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("GetName"), cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> GetEmailAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await _userState.SaveChangesAsync(stepContext.Context, true);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("GetEmail"), cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> GetContactPhoneAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await _userState.SaveChangesAsync(stepContext.Context, true);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("GetPhone"), cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> ProcessDataAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            await _userState.SaveChangesAsync(stepContext.Context, true);

            var information = userProfile.Conversations.Last().Questions.Last().generalInformation;
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("Startservice"), cancellationToken: cancellationToken);

            DatosEntradaValidarCliente datosEntrada = new DatosEntradaValidarCliente
            {
                tipoIdentificacion = information.tipoIdentificacion,
                identificacion = information.identificacion
            };

            var resultCliente = _transactionalServices.MTValidarcliente(company, datosEntrada);

            DatosEntrada conceptoFacturaEntrada;
            if (resultCliente.estado == "1844")
            {
                conceptoFacturaEntrada = new DatosEntrada()
                {
                    contrato = information.contrato,
                    tipoIdentificacion = "1",
                    identificacion = GetIdCompany(company),
                    cliente = information.cliente,
                    correo = information.correo,
                    telefono = information.telefono
                };
                userProfile.Conversations.Last().Questions.Last().generalInformation.tipoIdentificacion = "1";
                userProfile.Conversations.Last().Questions.Last().generalInformation.identificacion = GetIdCompany(company);
            }
            else
            {
                conceptoFacturaEntrada = new DatosEntrada()
                {
                    contrato = information.contrato,
                    tipoIdentificacion = information.tipoIdentificacion,
                    identificacion = information.identificacion,
                    cliente = information.cliente,
                    correo = information.correo,
                    telefono = information.telefono
                };
            }

            ValidateTreatmentData(userProfile);

            var result = _transactionalServices.GetEstadoDeuda(company, conceptoFacturaEntrada);
            result.contrato = information.contrato;            
            //Los xml no concuerdan
            if (result.mensajes.CodMsj == "0")
            {
                await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("EstadoCuentaResult", result), cancellationToken: cancellationToken); 
            }
            else
            {
                if (result.mensajes.CodMsj == "-12")
                {
                    await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("EstadoCuentaSinSaldoResult", result), cancellationToken: cancellationToken);
                }
                else
                {
                    if (result.mensajes.CodMsj == "Server Error")
                    {
                        await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ServerError"), cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(userProfile.ContactMessage), cancellationToken: cancellationToken);
                    }
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
            if (userProfile.Dialogturn == "EstadoCuentaDialog")
            {
                text = innerDc.Context.Activity.Text.ToLower(CultureInfo.CurrentCulture);

                if (text.ToLower(CultureInfo.CurrentCulture).Trim() == "salir")
                {
                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage"));
                    userProfile.Dialogturn = null;
                    userProfile.countFailContract = 0;
                    return await innerDc.EndDialogAsync();
                }

                var generalInformation = userProfile.Conversations.Last().Questions.Last().generalInformation;
                generalInformation.company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

                validation = ValidateData(text, generalInformation);

                switch (validation)
                {
                    case MessagesCertificadoFacturaDialog.TextContrato:
                        generalInformation.contrato = text;
                        return await innerDc.ContinueDialogAsync();
                    case MessagesCertificadoFacturaDialog.TextTipoId:
                        generalInformation.tipoIdentificacion = ValidateCompanyForTypeId(Convert.ToInt32(text), generalInformation.company);
                        return await innerDc.ContinueDialogAsync();
                    case MessagesCertificadoFacturaDialog.TextId:
                        generalInformation.identificacion = text;
                        return await innerDc.ContinueDialogAsync();
                    case MessagesCertificadoFacturaDialog.TextName:
                        generalInformation.cliente = text;
                        return await innerDc.ContinueDialogAsync();
                    case MessagesCertificadoFacturaDialog.TextEmail:
                        generalInformation.correo = text;
                        return await innerDc.ContinueDialogAsync();
                    case MessagesCertificadoFacturaDialog.TextPhone:
                        generalInformation.telefono = text;
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
            string message = "";

            if (String.IsNullOrEmpty(information.contrato))
            {
                message = ValidateContract(data);
            }
            else if (String.IsNullOrEmpty(information.tipoIdentificacion))
            {
                message = ValidateTypeId(data,information.company);
            }
            else if (String.IsNullOrEmpty(information.identificacion))
            {
                message = ValidateId(data);
            }
            else if (String.IsNullOrEmpty(information.cliente))
            {
                message = Validatename(data);
            }
            else if (String.IsNullOrEmpty(information.correo))
            {
                message = ValidateEmail(data);
            }
            else if (String.IsNullOrEmpty(information.telefono))
            {
                message = ValidatePhone(data);
            }

            return message;
        }

        public void ValidateTreatmentData(UserProfileState userProfile)
        {
            var information = userProfile.Conversations.Last().Questions.Last().generalInformation;
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

            DatosEntradaTratamientoDatos datosEntradaTratamientoGas = new DatosEntradaTratamientoDatos
            {
                comentario = "",
                estadoManejo = ValidatedataTreatment(userProfile.Conversations.Last().Questions.Last().DataTreatmentBrilla),
                tipoIdentificacion = information.tipoIdentificacion,
                identificacion = information.identificacion,
                tipoProducto = "7055"

            };

            var resultTratamientoGas = _transactionalServices.MTValidarTratamientoDatos(company, datosEntradaTratamientoGas);


            DatosEntradaTratamientoDatos datosEntradaTratamientoBrilla = new DatosEntradaTratamientoDatos
            {
                comentario = "",
                estadoManejo = ValidatedataTreatment(userProfile.Conversations.Last().Questions.Last().DataTreatmentBrilla),
                tipoIdentificacion = information.tipoIdentificacion,
                identificacion = information.identificacion,
                tipoProducto = "7055"

            };

            var resultTratamientoBrilla = _transactionalServices.MTValidarTratamientoDatos(company, datosEntradaTratamientoBrilla);

        }

    }
}
