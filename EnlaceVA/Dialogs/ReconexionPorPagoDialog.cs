
using EnlaceVA.Models;
using EnlaceVA.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Extensions.DependencyInjection;
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
    public class ReconexionPorPagoDialog : ComponentDialog
    {
        private readonly BotState _userState;
        private readonly LocaleTemplateManager _templateEngine;
        private readonly IStatePropertyAccessor<UserProfileState> _accessor;
        private readonly TransactionalServices _transactionalServices;
        private readonly LocaleTemplateManager _templateManager;

        public ReconexionPorPagoDialog(
            IServiceProvider serviceProvider,
            IBotTelemetryClient telemetryClient)
            : base(nameof(ReconexionPorPagoDialog))
        {
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();
            _userState = serviceProvider.GetService<UserState>();
            _accessor = _userState.CreateProperty<UserProfileState>(nameof(UserProfileState));
            _transactionalServices = serviceProvider.GetService<TransactionalServices>();
            _templateManager = serviceProvider.GetService<LocaleTemplateManager>();
          
            var ReconexionPorPagoSteps = new WaterfallStep[]
            {
                GetContractAsync,
                CheckContract,
                SendData,
                FinishDialogAsync
            };

            TelemetryClient = telemetryClient;
            AddDialog(new WaterfallDialog(nameof(ReconexionPorPagoDialog), ReconexionPorPagoSteps));
        }
        
        private async Task<DialogTurnResult> GetContractAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            userProfile.Dialogturn = "ReconexionPorPagoDialog";
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
                    await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("CompletedMessage"), cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync();

                }
                else
                {
                    if (company == "gdo")
                    {
                        await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ErrorContratoGDO"), cancellationToken: cancellationToken);
                        return await stepContext.ReplaceDialogAsync("ReconexionPorPagoDialog");

                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("ErrorContrato"), cancellationToken: cancellationToken);
                        return await stepContext.ReplaceDialogAsync("ReconexionPorPagoDialog");
                    }
                }
            }
        }

        private async Task<DialogTurnResult> SendData(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessor.GetAsync(stepContext.Context, () => new UserProfileState());
            await _userState.SaveChangesAsync(stepContext.Context, true);

            var information = userProfile.Conversations.Last().Questions.Last().generalInformation;
            var company = userProfile.Company.ToLower(CultureInfo.CurrentCulture);

            await stepContext.Context.SendActivityAsync(_templateManager.GenerateActivityForLocale("Startservice"), cancellationToken: cancellationToken);

            DatosEntradaReconexionPorPago ReconexionPorPagoEntrada;
          
            ReconexionPorPagoEntrada = new DatosEntradaReconexionPorPago()
             {
                 contrato = information.contrato,
                 tipoIdentificacion = null,
                 identificacion = null,
                 cliente = null,
                 correo = null,
                 telefono = null,
            };
            

            var result = _transactionalServices.GetReconexionPorPago(company, ReconexionPorPagoEntrada);

            if (result.estado == "-1")
            {
                await stepContext.Context.SendActivityAsync(_templateEngine.GenerateActivityForLocale("ReconexionPorPagoResult"), cancellationToken: cancellationToken);
            }
            else if(result.estado == "0")
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

            var validation = "";
            var text = "";

            if (userProfile.Dialogturn == "ReconexionPorPagoDialog")
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
        
        public static string ValidateData(string data,GeneralInformation information)
        {            
            string message;

            if (information.contrato == null)
            {                
                message = ValidateContract(data);
            }
            else
            {
                message = "Error";
            }

            return message;
        }

    }
}
