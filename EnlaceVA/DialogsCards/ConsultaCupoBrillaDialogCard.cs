
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

namespace EnlaceVA.Dialogs
{
    public class ConsultaCupoBrillaDialogCard : ComponentDialog
    {        
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly TransactionalServices _transactionalServices;
        private readonly LocaleTemplateManager _templateManager;

        public ConsultaCupoBrillaDialogCard(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(ConsultaCupoBrillaDialogCard))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            _transactionalServices = serviceProvider.GetService<TransactionalServices>();
            _templateManager = serviceProvider.GetService<LocaleTemplateManager>();

            var CertificadoFacturaSteps = new WaterfallStep[]
            {
                StartDialogAsync,               
                SendData,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(ConsultaCupoBrillaDialogCard), CertificadoFacturaSteps));
        }        

        private async Task<DialogTurnResult> StartDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "ConsultaCupoBrillaDialogCard";

            GeneralInformation cardData = new GeneralInformation
            {
                company = ValidateCompany(userProfile.Company),
                service = "Consulta Cupo Brilla"
            };

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ReturningContrac", cardData), cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }                

        private async Task<DialogTurnResult> SendData(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            await _userState.SaveChangesAsync(stepContext.Context, true);

            var information = userProfile.Conversations.Last().Questions.Last().generalInformation;
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("Startservice"), cancellationToken: cancellationToken);

            DatosEntrada datosEntrada = new DatosEntrada
            {
                contrato = information.contrato
            };

            DatosEntradaValidarContrato datosEntradaContract = new DatosEntradaValidarContrato
            {
                contrato = information.contrato
            };

            var resultContract = _transactionalServices.MTValidarcontrato(company, datosEntradaContract);

            if (resultContract.mensajes.CodMsj == "0")
            {
                var result = _transactionalServices.GetConsultaCupoBrilla(company, datosEntrada);

                if (result.estado == "0" && result.cupo_disponible != "0")
                {
                    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ConsultaCupoBrilla", result), cancellationToken: cancellationToken);
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
                        await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale(userProfile.ContactMessage), cancellationToken: cancellationToken);
                    }
                }
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

                return await stepContext.ReplaceDialogAsync("ConsultaCupoBrillaDialogCard");
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

            if (userProfile.Dialogturn == "ConsultaCupoBrillaDialogCard")
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
                        Validation = ValidateCardBasic(cardData);
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
                        await innerDc.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ServerError"), cancellationToken: cancellationToken);

                        /*
                         *  Se cambia para que el mensaje de error no sea el resultado de la exception sino el error equivalente a error en el servidor.
                         */
                        //await innerDc.Context.SendActivityAsync("Error:" + e.Message, cancellationToken: cancellationToken);

                        return await innerDc.ContinueDialogAsync();
                    }
                }
                else {
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
