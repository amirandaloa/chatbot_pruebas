﻿
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
using static EnlaceVA.Utilities.Utility;
using System.Resources;
using EnlaceVA.Resources;
using EnlaceVA.Utilities;

namespace EnlaceVA.Dialogs
{
    public class VentaSegurosBrillaDialog : ComponentDialog
    {
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly TransactionalServices _transactionalServices;
        private readonly LocaleTemplateManager _templateManager;

        public VentaSegurosBrillaDialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(VentaSegurosBrillaDialog))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            _transactionalServices = serviceProvider.GetService<TransactionalServices>();
            _templateManager = serviceProvider.GetService<LocaleTemplateManager>();

            var VentaSeguroSteps = new WaterfallStep[]
            {
                GetContractAsync,
                CheckContract,
                GetTypePolicy,
                GetObservation,
                GetTypeIdAsync,
                GetIdAsync,
                GetNameAsync,
                GetEmailAsync,
                GetPhoneAsync,
                SendData,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(VentaSegurosBrillaDialog), VentaSeguroSteps));
        }

        private async Task<DialogTurnResult> GetContractAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "VentaSegurosBrillaDialog";
            userProfile.Conversations.Last().Questions.Last().generalInformation = new GeneralInformation();

            await _userState.SaveChangesAsync(stepContext.Context, true);

            string url = ValidateCompany(userProfile.Company);

            Activity message = _templateManager.GenerateActivityForLocale("Obtaincontract");

            message.Attachments = new List<Attachment> {
            new Attachment(){
                    ContentUrl = url,
                    ContentType = "image/jpg",
                    Name = "datauri" }};

            await stepContext.Context.SendActivityAsync(message, cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> CheckContract(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

                return await stepContext.ReplaceDialogAsync("VentaSegurosBrillaDialog");
            }
        }

        private async Task<DialogTurnResult> GetTypePolicy(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());

            if (userProfile.Dialogturn != "Salir")
            {
                userProfile.Dialogturn = "VentaSegurosBrillaDialog";
                await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("Introductorymessage"), cancellationToken: cancellationToken);
                await _userState.SaveChangesAsync(stepContext.Context, true);

                var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

                if (company == MenuSubMenuEnum.CompanyName.Stg)
                    await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("GetTypePolicySTG"), cancellationToken: cancellationToken);
                else
                {
                    userProfile.Conversations.Last().Questions.Last().generalInformation.tipoPoliza = "1";//1 corresponde a SEGURO PROTEGIDO – MODULO I – PLAN I
                    return await stepContext.ContinueDialogAsync();
                }

                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"));
                userProfile.Dialogturn = null;
                return await stepContext.EndDialogAsync();
            }            
        }

        private async Task<DialogTurnResult> GetObservation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await _userState.SaveChangesAsync(stepContext.Context, true);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("GetObservationText"), cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> GetTypeIdAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
           
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(Utilities.Utility.GetTypeIdByCompany(company)), cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> GetIdAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await _userState.SaveChangesAsync(stepContext.Context, true);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("GetId"), cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> GetNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

        private async Task<DialogTurnResult> GetPhoneAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await _userState.SaveChangesAsync(stepContext.Context, true);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("GetPhone"), cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> SendData(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            await _userState.SaveChangesAsync(stepContext.Context, true);

            var information = userProfile.Conversations.Last().Questions.Last().generalInformation;
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("Startservice"), cancellationToken: cancellationToken);

            DatosEntradaValidarCliente datosEntrada = new DatosEntradaValidarCliente
            {
                tipoIdentificacion = information.tipoIdentificacion,
                identificacion = information.identificacion,
                correo = information.correo,
                telefono = information.telefono

            };

            var resultCliente = _transactionalServices.MTValidarcliente(company, datosEntrada);

            DatosEntradaVentaSeguros VentaSegurosEntrada;
            if (resultCliente.estado == "1844")
            {
                VentaSegurosEntrada = new DatosEntradaVentaSeguros()
                {
                    contrato = information.contrato,
                    poliza = information.tipoPoliza,
                    observacion = information.observacionSeguro,
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
                VentaSegurosEntrada = new DatosEntradaVentaSeguros()
                {
                    contrato = information.contrato,
                    poliza = information.tipoPoliza,
                    observacion = information.observacionSeguro,
                    tipoIdentificacion = information.tipoIdentificacion,
                    identificacion = information.tipoIdentificacion,
                    cliente = information.cliente,
                    correo = information.correo,
                    telefono = information.telefono
                };
            }

            ValidateTreatmentData(userProfile);


            var result = _transactionalServices.GetVentaSeguros(company, VentaSegurosEntrada);
            
            if (result.mensajes.CodMsj == "0")
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(String.Format("BrillaSeguroContratoCupo{0}", company.ToUpper()), result), cancellationToken: cancellationToken);
            }
            else if (result.mensajes.CodMsj == "-8")
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(String.Format("BrillaSegurosolicitudvisita{0}", company.ToUpper())), cancellationToken: cancellationToken);
            }
            else if (result.mensajes.CodMsj == "-7")
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(String.Format("BrillaSeguroContratosinCupoCupo{0}", company.ToUpper())), cancellationToken: cancellationToken);
            }
            else
            {
                //Se tiene que tener en cuenta para cuando el contrato no tenga cupo
                //if (result.mensajes.CodMsj == "-11")
                //{
                //    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ConceptoFacturaSinSaldoResult", result), cancellationToken: cancellationToken);
                //}
                //else
                //{
                    
                //}
                if (result.mensajes.CodMsj == "Server Error")
                {
                    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ServerError"), cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(userProfile.ContactMessage), cancellationToken: cancellationToken);
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
            if (userProfile.Dialogturn == "VentaSegurosBrillaDialog")
            {
                text = innerDc.Context.Activity.Text.ToLower(CultureInfo.CurrentCulture);

                if (text.ToLower(CultureInfo.CurrentCulture).Trim() == "salir")
                {
                    await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage"));
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
                    case MessagesCertificadoFacturaDialog.TextTipoPolicy:
                        GetInformation.tipoPoliza = ConvertPolicy(text);
                        return await innerDc.ContinueDialogAsync();
                    case MessagesCertificadoFacturaDialog.TextObservation:
                        GetInformation.observacionSeguro = text;
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
                    case MessagesCertificadoFacturaDialog.ErrorId:
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

            if (information.contrato == null)
            {
                message = ValidateContract(data);
            }
            else if (information.tipoPoliza == null)
            {
                message = ValidateTypePolicy(data);
            }
            else if (information.observacionSeguro == null)
            {
                message = ValidateObservation(data);
            }
            else if (information.tipoIdentificacion == null)
            {
                message = ValidateTypeId(data,information.company);
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

        public static string ConvertPolicy(string data) // convertir el poliza por los numero de sebas
        {
            string message;

            message = data switch
            {
                "1" => "975",
                "2" => "815",
                "3" => "637",
                "4" => "1055",
                _ => ""
            };

            return message;
        }

        public void ValidateTreatmentData(UserProfileState userProfile)
        {
            var information = userProfile.Conversations.Last().Questions.Last().generalInformation;
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);
            if (company == "ceo")
            {
                DatosEntradaTratamientoDatos datosEntradaTratamientoGas = new DatosEntradaTratamientoDatos
                {
                    comentario = "ChatBot: Tratamiento Energia",
                    estadoManejo = ValidatedataTreatment(userProfile.Conversations.Last().Questions.Last().DataTreatmentBrilla),
                    tipoIdentificacion = information.tipoIdentificacion,
                    identificacion = information.identificacion,
                    tipoProducto = "7015"

                };
                var resultTratamientoGas = _transactionalServices.MTValidarTratamientoDatos(company, datosEntradaTratamientoGas);
                DatosEntradaTratamientoDatos datosEntradaTratamientoBrilla = new DatosEntradaTratamientoDatos
                {
                    comentario = "ChatBot: Tratamiento Brilla",
                    estadoManejo = ValidatedataTreatment(userProfile.Conversations.Last().Questions.Last().DataTreatmentBrilla),
                    tipoIdentificacion = information.tipoIdentificacion,
                    identificacion = information.identificacion,
                    tipoProducto = "7051"

                };
                var resultTratamientoBrilla = _transactionalServices.MTValidarTratamientoDatos(company, datosEntradaTratamientoBrilla);

            }
            else if (company == "stg")
            {
                DatosEntradaTratamientoDatos datosEntradaTratamientoGas = new DatosEntradaTratamientoDatos
                {
                    comentario = "ChatBot: Tratamiento Gas",
                    estadoManejo = ValidatedataTreatment(userProfile.Conversations.Last().Questions.Last().DataTreatmentBrilla),
                    tipoIdentificacion = information.tipoIdentificacion,
                    identificacion = information.identificacion,
                    tipoProducto = "7014"

                };
                var resultTratamientoGas = _transactionalServices.MTValidarTratamientoDatos(company, datosEntradaTratamientoGas);
                DatosEntradaTratamientoDatos datosEntradaTratamientoBrilla = new DatosEntradaTratamientoDatos
                {
                    comentario = "ChatBot: Tratamiento Brilla",
                    estadoManejo = ValidatedataTreatment(userProfile.Conversations.Last().Questions.Last().DataTreatmentBrilla),
                    tipoIdentificacion = information.tipoIdentificacion,
                    identificacion = information.identificacion,
                    tipoProducto = "7051"

                };
                var resultTratamientoBrilla = _transactionalServices.MTValidarTratamientoDatos(company, datosEntradaTratamientoBrilla);

            }
            else if (company == "gdo")
            {
                DatosEntradaTratamientoDatos datosEntradaTratamientoGas = new DatosEntradaTratamientoDatos
                {
                    comentario = "ChatBot: Tratamiento Gas",
                    estadoManejo = ValidatedataTreatment(userProfile.Conversations.Last().Questions.Last().DataTreatmentBrilla),
                    tipoIdentificacion = information.tipoIdentificacion,
                    identificacion = information.identificacion,
                    tipoProducto = "7014"

                };
                var resultTratamientoGas = _transactionalServices.MTValidarTratamientoDatos(company, datosEntradaTratamientoGas);
                DatosEntradaTratamientoDatos datosEntradaTratamientoBrilla = new DatosEntradaTratamientoDatos
                {
                    comentario = "ChatBot: Tratamiento Brilla",
                    estadoManejo = ValidatedataTreatment(userProfile.Conversations.Last().Questions.Last().DataTreatmentBrilla),
                    tipoIdentificacion = information.tipoIdentificacion,
                    identificacion = information.identificacion,
                    tipoProducto = "7055"

                };
                var resultTratamientoBrilla = _transactionalServices.MTValidarTratamientoDatos(company, datosEntradaTratamientoBrilla);

            }
            else
            {
                DatosEntradaTratamientoDatos datosEntradaTratamientoGas = new DatosEntradaTratamientoDatos
                {
                    comentario = "ChatBot: Tratamiento Gas",
                    estadoManejo = ValidatedataTreatment(userProfile.Conversations.Last().Questions.Last().DataTreatmentBrilla),
                    tipoIdentificacion = information.tipoIdentificacion,
                    identificacion = information.identificacion,
                    tipoProducto = "2"

                };
                var resultTratamientoGas = _transactionalServices.MTValidarTratamientoDatos(company, datosEntradaTratamientoGas);
                DatosEntradaTratamientoDatos datosEntradaTratamientoBrilla = new DatosEntradaTratamientoDatos
                {
                    comentario = "ChatBot: Tratamiento Brilla",
                    estadoManejo = ValidatedataTreatment(userProfile.Conversations.Last().Questions.Last().DataTreatmentBrilla),
                    tipoIdentificacion = information.tipoIdentificacion,
                    identificacion = information.identificacion,
                    tipoProducto = "7051"

                };
                var resultTratamientoBrilla = _transactionalServices.MTValidarTratamientoDatos(company, datosEntradaTratamientoBrilla);

            }
        }
    }    
}
