
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
    public class TomalecturaDialogCard : ComponentDialog
    {
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly TransactionalServices _transactionalServices;
        private readonly LocaleTemplateManager _templateManager;

        public TomalecturaDialogCard(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(TomalecturaDialogCard))
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
                SendData,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(TomalecturaDialogCard), CertificadoFacturaSteps));
        }

        private async Task<DialogTurnResult> StartDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "TomalecturaDialogCard";

            GeneralInformation cardData = new GeneralInformation
            {
                company = ValidateCompany(userProfile.Company),
                service = "Toma de Lectura"
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
                return await stepContext.NextAsync();
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

                return await stepContext.ReplaceDialogAsync("TomalecturaDialogCard");
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
                identificacion = information.identificacion
            };

            var resultCliente = _transactionalServices.MTValidarcliente(company, datosEntrada);
            DatosEntrada tomaLecturaEntrada;
            tomaLecturaEntrada = new DatosEntrada()
            {
                contrato = information.contrato,
                tipoIdentificacion = information.tipoIdentificacion,
                identificacion = information.identificacion,
                cliente = information.cliente,
                correo = information.correo,
                telefono = information.telefono

            };
            var result = _transactionalServices.GetTomalectura(company, tomaLecturaEntrada);
            result.contrato = information.contrato;
            if (result.mensajes.CodMsj == "0")
            {
                // if (information.company == "ceo")
                if (company == "ceo")
                {
                    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("UltimaLecturaResultCEO", result), cancellationToken: cancellationToken);

                }
                else
                {
                    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("UltimaLecturaResult", result), cancellationToken: cancellationToken);

                }
            }
            else
            {
                if (result.mensajes.CodMsj == "-13")
                {
                    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("UltimaLecturaNoResult", result), cancellationToken: cancellationToken);
                }
                else if(result.mensajes.CodMsj == "-3")
                {
                    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("UltimaLecturaPendienteResult", result), cancellationToken: cancellationToken);
                }
                else if (result.mensajes.CodMsj == "-1")
                {
                    if (company == "gdo")
                    {
                        await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("UltimaLecturaNoResultNoActivoSuspendidoGDO", result), cancellationToken: cancellationToken);
                    }
                    else if (company == "stg")
                    {
                        await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("UltimaLecturaNoResultNoActivoSuspendidoSTG", result), cancellationToken: cancellationToken);
                    }
                    else if (company == "ceo")
                    {
                        await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("UltimaLecturaNoResultNoActivoSuspendidoCEO", result), cancellationToken: cancellationToken);
                    }
                    else if (company == "quavii")
                    {
                        await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("UltimaLecturaNoResultNoActivoSuspendidoQUAVII", result), cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ErrorServicesResult", result.mensajes), cancellationToken: cancellationToken);
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

            if (userProfile.Dialogturn == "TomalecturaDialogCard")
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

    }
}
