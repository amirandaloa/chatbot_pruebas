// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using EnlaceVA.Tests.Utterances;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnlaceVA.Tests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class InterruptionTests : BotTestBase
    {
        [TestMethod]
        public async Task Test_EmergenciasDialog_Dialog()
        {
            var responseEmergenciasGdoPromptVariations = AllResponsesTemplates.ExpandTemplate("ResponseEmergenciasGdo");
            var completePromptVariations = AllResponsesTemplates.ExpandTemplate("CompletedMessage");

            await GetTestFlow()
                .Send(new Activity()
                {
                    Type = ActivityTypes.ConversationUpdate,
                    MembersAdded = new List<ChannelAccount>() { new ChannelAccount(GeneralUtterances.CompanyTest) }
                })
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.Question)
                .AssertReply(activity => Assert.AreEqual(GeneralUtterances.QnAAnswer, activity.AsMessageActivity().Text))
                .AssertReply(activity => Assert.AreEqual(completePromptVariations.FirstOrDefault().ToString(), activity.AsMessageActivity().Text))
                .Send(GeneralUtterances.Emergency)
                .AssertReply(activity => Assert.IsTrue(responseEmergenciasGdoPromptVariations.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                .AssertReply(activity => Assert.AreEqual(completePromptVariations.FirstOrDefault().ToString(), activity.AsMessageActivity().Text))
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_ExencionContribucionDialog_Dialog()
        {
            var responseIndustria_no_reguladaGdoPromptVariations = AllResponsesTemplates.ExpandTemplate("ResponseIndustria_no_reguladaGdo");
            var completePromptVariations = AllResponsesTemplates.ExpandTemplate("CompletedMessage");
            
            await GetTestFlow()
                .Send(new Activity()
                {
                    Type = ActivityTypes.ConversationUpdate,
                    MembersAdded = new List<ChannelAccount>() { new ChannelAccount(GeneralUtterances.CompanyTest) }
                })
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.Question)
                .AssertReply(activity => Assert.AreEqual(GeneralUtterances.QnAAnswer, activity.AsMessageActivity().Text))
                .AssertReply(activity => Assert.AreEqual(completePromptVariations.FirstOrDefault().ToString(), activity.AsMessageActivity().Text))
                .Send(GeneralUtterances.Exemption)
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.ExemptionSecond)
                .AssertReply(activity => Assert.IsTrue(responseIndustria_no_reguladaGdoPromptVariations.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                .AssertReply(activity => Assert.AreEqual(completePromptVariations.FirstOrDefault().ToString(), activity.AsMessageActivity().Text))
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_TrasladoDeudaDialog_Dialog()
        {
            var responseTraslado_deuda_brillaGdoPromptVariations = AllResponsesTemplates.ExpandTemplate("ResponseTraslado_deuda_brillaGdo");
            var completePromptVariations = AllResponsesTemplates.ExpandTemplate("CompletedMessage");

            await GetTestFlow()
                .Send(new Activity()
                {
                    Type = ActivityTypes.ConversationUpdate,
                    MembersAdded = new List<ChannelAccount>() { new ChannelAccount(GeneralUtterances.CompanyTest) }
                })
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.Question)
                .AssertReply(activity => Assert.AreEqual(GeneralUtterances.QnAAnswer, activity.AsMessageActivity().Text))
                .AssertReply(activity => Assert.AreEqual(completePromptVariations.FirstOrDefault().ToString(), activity.AsMessageActivity().Text))
                .Send(GeneralUtterances.DebtTransfer)
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.DebtTransferSecond)
                .AssertReply(activity => Assert.IsTrue(responseTraslado_deuda_brillaGdoPromptVariations.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                .AssertReply(activity => Assert.AreEqual(completePromptVariations.FirstOrDefault().ToString(), activity.AsMessageActivity().Text))
                .StartTestAsync();
        }        

        [TestMethod]
        public async Task Test_Constancia_de_pago_Intent()
        {

            var expectedTemporalMessage = AllResponsesTemplates.ExpandTemplate("AbonaatufacturaTemporalMessage");
            var expectedCompletedMessage = AllResponsesTemplates.ExpandTemplate("CompletedMessage");

            string expectedContractMessage = "Si necesitas realizar un pago anticipado/prepago de tu factura, ingresa el número de contrato";
            string expectedNameMessage = "Por favor digite su nombre";
            string expectedEmailMessage = "Por favor digite su correo electronico";
            string expectedPhoneMessage = "Por favor digite un numero celular";
            string expectedBillValueMessage = "Por favor digite el valor para el cupon";

            string contractMessage = "646841";
            string nameMessage = "Cristian";
            string emailMessage = "crishotmail.com";
            string phoneMessage = "654134";
            string billValueMessage = "6498793";

            await GetTestFlow()
                .Send(new Activity()
                {
                    Type = ActivityTypes.ConversationUpdate,
                    MembersAdded = new List<ChannelAccount>() { new ChannelAccount("gdo") }
                })
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.Constancia_de_pago)
                .AssertReply(activity => Assert.AreEqual(expectedContractMessage, activity.AsMessageActivity().Text))
                .Send(contractMessage)
                .AssertReply(activity => Assert.AreEqual(expectedNameMessage, activity.AsMessageActivity().Text))
                .Send(nameMessage)
                .AssertReply(activity => Assert.AreEqual(expectedEmailMessage, activity.AsMessageActivity().Text))
                .Send(emailMessage)
                .AssertReply(activity => Assert.AreEqual(expectedPhoneMessage, activity.AsMessageActivity().Text))
                .Send(phoneMessage)
                .AssertReply(activity => Assert.AreEqual(expectedBillValueMessage, activity.AsMessageActivity().Text))
                .Send(billValueMessage)
                .AssertReply(activity => Assert.AreEqual(expectedTemporalMessage.FirstOrDefault().ToString(), activity.AsMessageActivity().Text))
                .AssertReply(activity => Assert.AreEqual(expectedCompletedMessage.FirstOrDefault().ToString(), activity.AsMessageActivity().Text))
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_Cambio_de_nombre_Intent()
        {
            var expectedResidencialGdoMessage = AllResponsesTemplates.ExpandTemplate("ResponseCambio_Nombre_ResidencialGdo");
            var expectedCompletedMessage = AllResponsesTemplates.ExpandTemplate("CompletedMessage");

            string expectedMessage = "Residencial";

            await GetTestFlow()
                .Send(new Activity()
                {
                    Type = ActivityTypes.ConversationUpdate,
                    MembersAdded = new List<ChannelAccount>() { new ChannelAccount("gdo") }
                })
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.Cambio_de_nombre)
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(expectedMessage)
                .AssertReply(activity => Assert.IsTrue(expectedResidencialGdoMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                .AssertReply(activity => Assert.AreEqual(expectedCompletedMessage.FirstOrDefault().ToString(), activity.AsMessageActivity().Text))
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_Terminación_de_contrato_Intent()
        {
            var expectedcontratoResidencialGdoMessage = AllResponsesTemplates.ExpandTemplate("ResponseTerminacion_contratoResidencialGdo");
            var expectedCompletedMessage = AllResponsesTemplates.ExpandTemplate("CompletedMessage");

            string expectedMessage = "Residencial";

            await GetTestFlow()
                .Send(new Activity()
                {
                    Type = ActivityTypes.ConversationUpdate,
                    MembersAdded = new List<ChannelAccount>() { new ChannelAccount("gdo") }
                })
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.Terminación_de_contrato)
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(expectedMessage)
                .AssertReply(activity => Assert.IsTrue(expectedcontratoResidencialGdoMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                .AssertReply(activity => Assert.AreEqual(expectedCompletedMessage.FirstOrDefault().ToString(), activity.AsMessageActivity().Text))
                .StartTestAsync();
        }
    }
}
