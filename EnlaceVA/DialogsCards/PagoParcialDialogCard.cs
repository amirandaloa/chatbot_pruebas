using EnlaceVA.Models;
using EnlaceVA.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static EnlaceVA.Utilities.DataValidations;
using static EnlaceVA.Utilities.Utility;

namespace EnlaceVA.Dialogs
{
    public class PagoParcialDialogCard : ComponentDialog
    {
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly TransactionalServices _transactionalServices;
        private readonly LocaleTemplateManager _templateManager;

        public PagoParcialDialogCard(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(PagoParcialDialogCard))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            _transactionalServices = serviceProvider.GetService<TransactionalServices>();
            _templateManager = serviceProvider.GetService<LocaleTemplateManager>();

            var CertificadoFacturaSteps = new WaterfallStep[]
            {
                StartDialogAsync,
                DataTreatment,
                BasicData,
                PayData,
                SendData,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(PagoParcialDialogCard), CertificadoFacturaSteps));
        }

        private async Task<DialogTurnResult> StartDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "PagoParcialDialogCard";

            GeneralInformation cardData = new GeneralInformation
            {
                company = ValidateCompany(userProfile.Company),
                service = "Cupón de pago parcial"
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
                return await stepContext.ReplaceDialogAsync("PagoParcialDialogCard");
            }
        }

        private async Task<DialogTurnResult> BasicData(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());

            if (userProfile.Dialogturn != "Salir")
            {
                userProfile.Dialogturn = "PagoParcialDialogCard";

                var cardData = userProfile.Conversations.Last().Questions.Last().generalInformation;

                cardData.Text = ValidateTypeidCompanyCard(userProfile.Company);

                await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ReturningBasicData", cardData), cancellationToken: cancellationToken);

                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"));
                userProfile.Dialogturn = null;
                return await stepContext.EndDialogAsync();
            }
        }

        private async Task<DialogTurnResult> PayData(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "PagoParcialDialogCard";

            var cardData = userProfile.Conversations.Last().Questions.Last().generalInformation;

            cardData.Text = ValidateTypeidCompanyCard(userProfile.Company);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ReturningGetPay", cardData), cancellationToken: cancellationToken);

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
                identificacion = information.identificacion
            };

            var resultCliente = _transactionalServices.MTValidarcliente(company, datosEntrada);
            DatosEntradaPagoParcial pagoParcialEntrada;
            if (resultCliente.estado == "1844")
            {
                pagoParcialEntrada = new DatosEntradaPagoParcial()
                {
                    contrato = information.contrato,
                    tipoIdentificacion = "1",
                    identificacion = GetIdCompany(company),
                    cliente = information.cliente,
                    correo = information.correo,
                    telefono = information.telefono,
                    pagoParcial = information.pagoParcial
                };
                userProfile.Conversations.Last().Questions.Last().generalInformation.tipoIdentificacion = "1";
                userProfile.Conversations.Last().Questions.Last().generalInformation.identificacion = GetIdCompany(company);
            }
            else
            {
                pagoParcialEntrada = new DatosEntradaPagoParcial()
                {
                    contrato = information.contrato,
                    tipoIdentificacion = information.tipoIdentificacion,
                    identificacion = information.identificacion,
                    cliente = information.cliente,
                    correo = information.correo,
                    telefono = information.telefono,
                    pagoParcial = information.pagoParcial
                };
            }

            ValidateTreatmentData(userProfile);



            var result = _transactionalServices.GetPagoParcial(company, pagoParcialEntrada);

            if (result.mensajes.CodMsj == "0")
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("PagoParcialResult", result), cancellationToken: cancellationToken);
            }
            else
            {
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
            var text = "";

            if (userProfile.Dialogturn == "PagoParcialDialogCard")
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var Validation = "";

                Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(innerDc.Context.Activity.Value.ToString());
                text = response["boton"];

                GeneralInformation cardData = JsonConvert.DeserializeObject<GeneralInformation>(innerDc.Context.Activity.Value.ToString(), settings);
                cardData.company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);
                Validation = ValidateData(text, cardData);





                switch (Validation)
                {
                    case "ReturningConfirmPay":
                        userProfile.Conversations.Last().Questions.Last().generalInformation.pagoParcial = cardData.pago;
                        cardData.pago = Formatmoney(cardData.pago, userProfile.Company);
                        await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ReturningConfirmPay", cardData), cancellationToken: cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);
                    case "ReturningGetPay":
                        await innerDc.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ReturningGetPay", cardData), cancellationToken: cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);
                    case "":
                        if (text == "EnviarContract")
                        {
                            userProfile.Conversations.Last().Questions.Last().generalInformation = cardData;
                        }
                        else if (text == "EnviarBasicData")
                        {
                            string contract = userProfile.Conversations.Last().Questions.Last().generalInformation.contrato;
                            userProfile.Conversations.Last().Questions.Last().generalInformation = cardData;
                            userProfile.Conversations.Last().Questions.Last().generalInformation.contrato = contract;
                        }
                        await _userState.SaveChangesAsync(innerDc.Context, false, cancellationToken);
                        return await innerDc.ContinueDialogAsync();
                    case "Salir":
                        userProfile.Dialogturn = null;
                        userProfile.countFailContract = 0;
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"));
                        return await innerDc.EndDialogAsync();
                    default:
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"));
                        return await innerDc.EndDialogAsync();
                }
            }
            else
            {
                return await innerDc.ContinueDialogAsync();
            }

        }

        public static string ValidateData(string button, GeneralInformation information)
        {
            string message = "";

            switch (button)
            {
                case "EnviarContract":
                    message = ValidateCardContract(information);
                    break;
                case "EnviarBasicData":
                    message = ValidateCardBasic(information);
                    break;
                case "EnviarPayData":
                    message = ValidateCardGetPay(information);
                    break;
                case "EnviarConfirmPay":
                    if (information.confirmarPago != "1")
                    {
                        message = "ReturningGetPay";
                    }
                    break;
                case "Salir":
                    message = "Salir";
                    break;
                default:
                    return "Error";
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
