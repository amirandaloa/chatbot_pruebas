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
using Newtonsoft.Json;
using EnlaceVA.Dialogs;
using EnlaceVA.Utilities;

namespace EnlaceVA.DialogsCards
{
    public class VentaSegurosBrillaDialogCard : ComponentDialog
    {
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly TransactionalServices _transactionalServices;
        private readonly LocaleTemplateManager _templateManager;

        public VentaSegurosBrillaDialogCard(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(VentaSegurosBrillaDialogCard))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            _transactionalServices = serviceProvider.GetService<TransactionalServices>();
            _templateManager = serviceProvider.GetService<LocaleTemplateManager>();

            var VentaSegurosCardSteps = new WaterfallStep[]
            {
                StartDialogAsync,
                DataTreatment,
                BasicData,
                SendData,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(VentaSegurosBrillaDialogCard), VentaSegurosCardSteps));
        }

        private async Task<DialogTurnResult> StartDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "VentaSegurosBrillaDialogCard";

            GeneralInformation cardData = new GeneralInformation
            {
                company = ValidateCompany(userProfile.Company),
                service = "Venta seguros brilla"
            };

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ReturningContrac", cardData), cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> DataTreatment(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
                    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"));
                    return await stepContext.EndDialogAsync();
                }

                await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ErrorContrato"), cancellationToken: cancellationToken);

                return await stepContext.ReplaceDialogAsync("VentaSegurosBrillaDialogCard");
            }
        }

        private async Task<DialogTurnResult> BasicData(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());

            if (userProfile.Dialogturn != "Salir")
            {
                userProfile.Dialogturn = "VentaSegurosBrillaDialogCard";

                var cardData = userProfile.Conversations.Last().Questions.Last().generalInformation;

                cardData.Text = ValidateTypeidCompanyCard(userProfile.Company);

                cardData.visible = userProfile.Company.ToLower(CultureInfo.CurrentCulture) == "stg" ? "true" : "false";

                await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ReturningDataVentaSeguros", cardData), cancellationToken: cancellationToken);

                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"));
                userProfile.Dialogturn = null;
                return await stepContext.EndDialogAsync();
            }            
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
                //if (result.mensajes.CodMsj == "-11")
                //{
                //    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ConceptoFacturaSinSaldoResult", result), cancellationToken: cancellationToken);
                //}
                //else
                //{
                //    if (result.mensajes.CodMsj == "Server Error")
                //    {
                //        await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ServerError", result.mensajes), cancellationToken: cancellationToken);
                //    }
                //    else
                //    {
                //        await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ErrorResult", result.mensajes), cancellationToken: cancellationToken);
                //    }
                //}
                if (result.mensajes.CodMsj == "Server Error" || result.mensajes.CodMsj == "InternalServerError")
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
            var text = "";

            if (userProfile.Dialogturn == "VentaSegurosBrillaDialogCard")
            {               
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                
                var Validation = "";
                if (innerDc.Context.Activity.Value != null)
                {
                    Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(innerDc.Context.Activity.Value.ToString());
                    text = response["boton"];

                    GeneralInformation cardData = JsonConvert.DeserializeObject<GeneralInformation>(innerDc.Context.Activity.Value.ToString(), settings);
                    
                    if (text == "EnviarContract")
                        Validation = ValidateCardContract(cardData);
                    else if (text == "EnviarBasicData")
                    {                        
                        cardData.contrato = userProfile.Conversations.Last().Questions.Last().generalInformation.contrato;
                        cardData.company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);                        
                        if (userProfile.Company != MenuSubMenuEnum.CompanyName.Stg)                                                    
                            cardData.tipoPoliza = "1";//1 corresponde a SEGURO PROTEGIDO – MODULO I – PLAN I                                                   
                        Validation = ValidateVentaSeguroCard(cardData);                        
                    }
                    else
                    {
                        userProfile.Dialogturn = null;
                        userProfile.countFailContract = 0;
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"));
                        return await innerDc.EndDialogAsync();
                    }

                    try
                    {
                        if (Validation != "")
                        {                            
                            await innerDc.Context.SendActivityAsync(Validation, cancellationToken: cancellationToken);
                            return new DialogTurnResult(DialogTurnStatus.Waiting);
                        }
                        else
                        {
                            userProfile.Conversations.Last().Questions.Last().generalInformation = cardData;
                            await _userState.SaveChangesAsync(innerDc.Context, false, cancellationToken);

                            return await innerDc.ContinueDialogAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        await innerDc.Context.SendActivityAsync("Error:" + e.Message, cancellationToken: cancellationToken);
                        return await innerDc.ContinueDialogAsync();
                    }
                }
                else
                {
                    await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ErrorAdaptiveCard"));
                    return new DialogTurnResult(DialogTurnStatus.Waiting);
                }

            }
            else
            {
                return await innerDc.ContinueDialogAsync();
            }

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
