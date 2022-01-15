
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
    public class ConsultaPQRDialogCard : ComponentDialog
    {
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly TransactionalServices _transactionalServices;
        private readonly LocaleTemplateManager _templateManager;

        public ConsultaPQRDialogCard(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(ConsultaPQRDialogCard))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            _transactionalServices = serviceProvider.GetService<TransactionalServices>();
            _templateManager = serviceProvider.GetService<LocaleTemplateManager>();

            var ConsultaPQRSteps = new WaterfallStep[]
            {
                StartDialogAsync,                
                SendData,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(ConsultaPQRDialogCard), ConsultaPQRSteps));
        }

        private async Task<DialogTurnResult> StartDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "ConsultaPQRDialogCard";

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ReturningSolicitud"), cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
        
        private async Task<DialogTurnResult> SendData(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            await _userState.SaveChangesAsync(stepContext.Context, true);

            var information = userProfile.Conversations.Last().Questions.Last().generalInformation;
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("Startservice"), cancellationToken: cancellationToken);
            DatosEntradaSolicitud conceptoFacturaEntrada = null;
            //Se ajusta ya que la empresa CEO tiene un tipo de identificacion diferente al que se está enviando.
            if (company ==  "ceo")
            {
                conceptoFacturaEntrada = new DatosEntradaSolicitud()
                {
                    solicitud = information.solicitud,
                    tipoIdentificacion = "111",
                    identificacion = Utilities.Utility.GetIdCompany(company),
                    cliente = information.cliente,
                    correo = information.correo,
                    telefono = information.telefono
                };
            }
            else
            {
                conceptoFacturaEntrada = new DatosEntradaSolicitud()
                {
                    solicitud = information.solicitud,
                    tipoIdentificacion = "1",
                    identificacion = Utilities.Utility.GetIdCompany(company),
                    cliente = information.cliente,
                    correo = information.correo,
                    telefono = information.telefono
                };
            }

            var result = _transactionalServices.GetConsultaPQR(company, conceptoFacturaEntrada);
            result.solicitud = information.solicitud;
            if (result.codError == "0")
            {
                if (result.descEstado.FirstOrDefault().codigo_estado_sol == "36")
                {
                    await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(String.Format("ConsultaPQRanulado{0}", company.ToUpper()), result.descEstado.FirstOrDefault()), cancellationToken: cancellationToken);
                } else if (result.descEstado.FirstOrDefault().codigo_estado_sol == "32")
                {
                    await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(String.Format("ConsultaPQRanulado{0}", company.ToUpper()), result.descEstado.FirstOrDefault()), cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ConsultaPQRSinAtenderResultGDO", result.descEstado.FirstOrDefault()), cancellationToken: cancellationToken);                    
                }
            }
            else if (result.codError == "-7")
            {
                await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale(String.Format("ConsultaPQRNoEncontrado{0}", company.ToUpper()), result), cancellationToken: cancellationToken);
            }
            else
            {
                if (result.codError == "Server Error")
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
            await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("CompletedMessage"));
            userProfile.Dialogturn = null;
            await _userState.SaveChangesAsync(stepContext.Context, true);

            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var userProfile = await _accessor.GetAsync(innerDc.Context, () => new UserProfileState());
            var text = "";

            if (userProfile.Dialogturn == "ConsultaPQRDialogCard")
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

                    if (text == "EnviarRequest")
                        Validation = ValidateCardRequest(cardData);
                    else
                    {
                        userProfile.Dialogturn = null;
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
