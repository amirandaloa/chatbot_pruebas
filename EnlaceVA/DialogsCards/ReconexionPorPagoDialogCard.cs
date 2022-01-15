
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
    public class ReconexionPorPagoDialogCard : ComponentDialog
    {
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly TransactionalServices _transactionalServices;
        private readonly LocaleTemplateManager _templateManager;

        public ReconexionPorPagoDialogCard(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(ReconexionPorPagoDialogCard))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            _transactionalServices = serviceProvider.GetService<TransactionalServices>();
            _templateManager = serviceProvider.GetService<LocaleTemplateManager>();

            var ReconexionPorPagoSteps = new WaterfallStep[]
            {
                StartDialogAsync,
                DataTreatment,
                SendData,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(ReconexionPorPagoDialogCard), ReconexionPorPagoSteps));
        }

        private async Task<DialogTurnResult> StartDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "ReconexionPorPagoDialogCard";

            GeneralInformation cardData = new GeneralInformation
            {
                company = ValidateCompany(userProfile.Company),
                service = "Reconexion Pago"
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
                return await stepContext.ReplaceDialogAsync("ReconexionPorPagoDialogCard");
            }
        }

        private async Task<DialogTurnResult> SendData(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            await _userState.SaveChangesAsync(stepContext.Context, true);

            var information = userProfile.Conversations.Last().Questions.Last().generalInformation;
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);


            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("Startservice"), cancellationToken: cancellationToken);



            DatosEntradaReconexionPorPago ReconexionPagoEntrada;
            ReconexionPagoEntrada = new DatosEntradaReconexionPorPago()
            {
                contrato = information.contrato,
                tipoIdentificacion = null,
                identificacion = null,
                cliente = null,
                correo = null,
                telefono = null,
            };

            userProfile.Conversations.Last().Questions.Last().generalInformation.tipoIdentificacion = "1";
            userProfile.Conversations.Last().Questions.Last().generalInformation.identificacion = GetIdCompany(company);

            var result = _transactionalServices.GetReconexionPorPago(company, ReconexionPagoEntrada);

            if (result.estado == "-1")
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ReconexionPorPagoResult"), cancellationToken: cancellationToken);
            }
            else if (result.estado == "0")
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ReconexionPorPagoTiempoE", result), cancellationToken: cancellationToken);
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

            if (userProfile.Dialogturn == "ReconexionPorPagoDialogCard")
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
                    {
                        Validation = ValidateCardContract(cardData);
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
    }
}
