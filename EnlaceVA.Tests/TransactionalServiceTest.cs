using System;
using System.Collections.Generic;
using System.Text;
using EnlaceVA.Tests.Utterances;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Microsoft.Bot.Solutions.Responses;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using System.Runtime.CompilerServices;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.LanguageGeneration;
using System.Globalization;
using EnlaceVA.Models;
using EnlaceVA.Services;

namespace EnlaceVA.Tests
{
    class TransactionalServiceTest
    {        
        [TestClass]
        [TestCategory("UnitTests")]
        public class InterruptionTests : BotTestBase
        {            
            protected Templates AllResponsesTemplates2
            {
                get
                {
                    var path = CultureInfo.CurrentUICulture.Name.ToLower() == "en-us" ?
                        Path.Combine(".", "Responses", $"AllResponses.lg") :
                        Path.Combine(".", "Responses", $"AllResponses.{CultureInfo.CurrentUICulture.Name.ToLower()}.lg");
                    return Templates.ParseFile(path);
                }
            }           

            [TestMethod]
            public async Task Test_ConceptoFactura_Intent()
            {
                var StartserviceMessage = AllResponsesTemplates.ExpandTemplate("Startservice");
                var botmessagecontrat = AllResponsesTemplates.ExpandTemplate("Obtaincontract");
                var MessageGasGDO1 = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageGasGDO");
                var MessageGasGDO2 = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageBrillaGDO");
                var introductorymessage = AllResponsesTemplates.ExpandTemplate("Introductorymessage");
                var typeIdGDOSTG = AllResponsesTemplates.ExpandTemplate("GetTypeIdGDOSTG");
                var id = AllResponsesTemplates.ExpandTemplate("GetId");
                var name = AllResponsesTemplates.ExpandTemplate("GetName");
                var correo = AllResponsesTemplates.ExpandTemplate("GetEmail");
                var telefono = AllResponsesTemplates.ExpandTemplate("GetPhone");
                var completedMessage = AllResponsesTemplates.ExpandTemplate("CompletedMessage");

                string contractmessage = "500";
                string positiveMessage = "1";
                string negativeMessage = "2";
                string typeIdMessage = "1";
                string idMessage = "6349";
                string nameMessage = "Cristian Alvarez";
                string emailMessage = "Prueba@prueba.com";
                string phoneMessage = "654196";

                //var serviceResult = "Te comparto la información sobre tu factura:\nContrato: 500\nÚltimo pago realizado: $27,648\nValor total a pagar: $29,488\nFecha límite de pago: 13-02-2021\nNúmero de cupón para pago: 212835670\nPara realizar el pago puedes acceder al siguiente enlace:\nwww.gdo.com.co/Paginas/Pago.aspx";

                //MTConceptoFactura mTConceptoFactura = new MTConceptoFactura {
                //    contrato = contractmessage,
                //    ultimoPagoRealizado  = "$27,648",
                //    valorTotalPagar  = "$29,488",
                //    fechaLimPago  = "13-02-2021",
                //    cupon  = "212835670",
                //    url = "www.gdo.com.co/Paginas/Pago.aspx"
                //};

                //var serviceResult = AllResponsesTemplates.ExpandTemplate("ConceptoFacturaResult", mTConceptoFactura);

                await GetTestFlow()
                    .Send(new Activity()
                    {
                        Type = ActivityTypes.ConversationUpdate,
                        MembersAdded = new List<ChannelAccount>() { new ChannelAccount("gdo")}
                    })
                    .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                    .Send(GeneralUtterances.Valor_fecha_limite_y_referencia_pago)
                    .AssertReply(activity => Assert.IsTrue(botmessagecontrat.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(contractmessage)
                    .AssertReply(activity => Assert.IsTrue(MessageGasGDO1.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(positiveMessage)
                    .AssertReply(activity => Assert.IsTrue(MessageGasGDO2.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(negativeMessage)
                    .AssertReply(activity => Assert.IsTrue(introductorymessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(typeIdGDOSTG.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(typeIdMessage)
                    .AssertReply(activity => Assert.IsTrue(id.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(idMessage)
                    .AssertReply(activity => Assert.IsTrue(name.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(nameMessage)
                    .AssertReply(activity => Assert.IsTrue(correo.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(emailMessage)
                    .AssertReply(activity => Assert.IsTrue(telefono.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(phoneMessage)
                    .AssertReply(activity => Assert.IsTrue(StartserviceMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    //.AssertReply(activity => Assert.IsTrue(serviceResult.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    ////.AssertReply(activity => Assert.AreEqual(serviceResult, activity.AsMessageActivity().Text))
                    //.AssertReply(activity => Assert.AreEqual(completedMessage, activity.AsMessageActivity().Text))
                    .StartTestAsync();
            }

            [TestMethod]
            public async Task Test_ConceptoFacturaFail_Intent()
            {
                var StartserviceMessage = AllResponsesTemplates.ExpandTemplate("Startservice");
                var botmessagecontrat = AllResponsesTemplates.ExpandTemplate("Obtaincontract");
                var MessageGasGDO1 = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageGasGDO");
                var MessageGasGDO2 = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageBrillaGDO");
                var introductorymessage = AllResponsesTemplates.ExpandTemplate("Introductorymessage");
                var typeIdGDOSTG = AllResponsesTemplates.ExpandTemplate("GetTypeIdGDOSTG");
                var id = AllResponsesTemplates.ExpandTemplate("GetId");
                var name = AllResponsesTemplates.ExpandTemplate("GetName");
                var correo = AllResponsesTemplates.ExpandTemplate("GetEmail");
                var telefono = AllResponsesTemplates.ExpandTemplate("GetPhone");
                var errorContrato = AllResponsesTemplates.ExpandTemplate("ErrorContrato");

                string contractmessage = "1";
                string secondcontractmessage = "500";
                string positiveMessage = "1";
                string negativeMessage = "2";
                string typeIdMessage = "1";
                string idMessage = "6349";
                string nameMessage = "Cristian Alvarez";
                string emailMessage = "Prueba@prueba.com";
                string phoneMessage = "654196";                

                await GetTestFlow()
                    .Send(new Activity()
                    {
                        Type = ActivityTypes.ConversationUpdate,
                        MembersAdded = new List<ChannelAccount>() { new ChannelAccount("gdo") }
                    })
                    .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                    .Send(GeneralUtterances.Valor_fecha_limite_y_referencia_pago)
                    .AssertReply(activity => Assert.IsTrue(botmessagecontrat.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(contractmessage)
                    .AssertReply(activity => Assert.IsTrue(errorContrato.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(botmessagecontrat.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(secondcontractmessage)
                    .AssertReply(activity => Assert.IsTrue(MessageGasGDO1.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(positiveMessage)
                    .AssertReply(activity => Assert.IsTrue(MessageGasGDO2.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(negativeMessage)
                    .AssertReply(activity => Assert.IsTrue(introductorymessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(typeIdGDOSTG.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(typeIdMessage)
                    .AssertReply(activity => Assert.IsTrue(id.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(idMessage)
                    .AssertReply(activity => Assert.IsTrue(name.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(nameMessage)
                    .AssertReply(activity => Assert.IsTrue(correo.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(emailMessage)
                    .AssertReply(activity => Assert.IsTrue(telefono.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(phoneMessage)
                    .AssertReply(activity => Assert.IsTrue(StartserviceMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))                    
                    .StartTestAsync();
            }

            [TestMethod]
            public async Task Test_ConsultaPQR_Intent()
            {
                var obtainRequest = AllResponsesTemplates.ExpandTemplate("ObtainRequest");                
                var StartserviceMessage = AllResponsesTemplates.ExpandTemplate("Startservice");
                var completedMessage = AllResponsesTemplates.ExpandTemplate("CompletedMessage");
                
                string requestmessage = "95457978";                

                MTConsultaEstado mTConsulta = new MTConsultaEstado
                {
                    solicitud = requestmessage
                };

                var consultaPQRSinAtenderResult = AllResponsesTemplates.ExpandTemplate("ConsultaPQRSinAtenderResult", mTConsulta);

                await GetTestFlow()
                    .Send(new Activity()
                    {
                        Type = ActivityTypes.ConversationUpdate,
                        MembersAdded = new List<ChannelAccount>() { new ChannelAccount("gdo") }
                    })
                    .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                    .Send(GeneralUtterances.Estado_de_solicitud_PQRs)
                    .AssertReply(activity => Assert.IsTrue(obtainRequest.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(requestmessage)                                                           
                    .AssertReply(activity => Assert.IsTrue(StartserviceMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(consultaPQRSinAtenderResult.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(completedMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))                    
                    .StartTestAsync();
            }

            [TestMethod]
            public async Task Test_ConsumoFacturado_Intent()
            {
                var StartserviceMessage = AllResponsesTemplates.ExpandTemplate("Startservice");
                var botmessagecontrat = AllResponsesTemplates.ExpandTemplate("Obtaincontract");
                var MessageGasGDO1 = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageGasGDO");
                var MessageGasGDO2 = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageBrillaGDO");
                var introductorymessage = AllResponsesTemplates.ExpandTemplate("Introductorymessage");
                var typeIdGDOSTG = AllResponsesTemplates.ExpandTemplate("GetTypeIdGDOSTG");
                var id = AllResponsesTemplates.ExpandTemplate("GetId");
                var name = AllResponsesTemplates.ExpandTemplate("GetName");
                var correo = AllResponsesTemplates.ExpandTemplate("GetEmail");
                var telefono = AllResponsesTemplates.ExpandTemplate("GetPhone");                
                var aplicaDesviacionGDO = AllResponsesTemplates.ExpandTemplate("AplicaDesviacionGDO");
                var completedMessage = AllResponsesTemplates.ExpandTemplate("CompletedMessage");
                var errorResult = AllResponsesTemplates.ExpandTemplate("ErrorResult");

                string contractmessage = "1349573";                
                string positiveMessage = "1";
                string negativeMessage = "2";
                string typeIdMessage = "1";
                string idMessage = "8593764";
                string nameMessage = "Cristian Alvarez";
                string emailMessage = "Prueba@prueba.com";
                string phoneMessage = "654196";


                await GetTestFlow()
                    .Send(new Activity()
                    {
                        Type = ActivityTypes.ConversationUpdate,
                        MembersAdded = new List<ChannelAccount>() { new ChannelAccount("gdo") }
                    })
                    .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                    .Send(GeneralUtterances.Consumo_facturado)
                    .AssertReply(activity => Assert.IsTrue(botmessagecontrat.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(contractmessage)
                    .AssertReply(activity => Assert.IsTrue(MessageGasGDO1.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(positiveMessage)
                    .AssertReply(activity => Assert.IsTrue(MessageGasGDO2.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(negativeMessage)
                    .AssertReply(activity => Assert.IsTrue(introductorymessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(typeIdGDOSTG.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(typeIdMessage)
                    .AssertReply(activity => Assert.IsTrue(id.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(idMessage)
                    .AssertReply(activity => Assert.IsTrue(name.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(nameMessage)
                    .AssertReply(activity => Assert.IsTrue(correo.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(emailMessage)
                    .AssertReply(activity => Assert.IsTrue(telefono.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(phoneMessage)
                    .AssertReply(activity => Assert.IsTrue(StartserviceMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(errorResult.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(completedMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .StartTestAsync();
            }

            [TestMethod]
            public async Task Test_ConsumoFacturadoFail_Intent()
            {
                var StartserviceMessage = AllResponsesTemplates.ExpandTemplate("Startservice");
                var botmessagecontrat = AllResponsesTemplates.ExpandTemplate("Obtaincontract");
                var MessageGasGDO1 = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageGasGDO");
                var MessageGasGDO2 = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageBrillaGDO");
                var introductorymessage = AllResponsesTemplates.ExpandTemplate("Introductorymessage");
                var typeIdGDOSTG = AllResponsesTemplates.ExpandTemplate("GetTypeIdGDOSTG");
                var id = AllResponsesTemplates.ExpandTemplate("GetId");
                var name = AllResponsesTemplates.ExpandTemplate("GetName");
                var correo = AllResponsesTemplates.ExpandTemplate("GetEmail");
                var telefono = AllResponsesTemplates.ExpandTemplate("GetPhone");
                var aplicaDesviacionGDO = AllResponsesTemplates.ExpandTemplate("AplicaDesviacionGDO");
                var completedMessage = AllResponsesTemplates.ExpandTemplate("CompletedMessage");
                var errorResult = AllResponsesTemplates.ExpandTemplate("ErrorResult");
                var errorContrato = AllResponsesTemplates.ExpandTemplate("ErrorContrato");

                string contractmessage = "1";
                string secondcontractmessage = "1349573";               
                string positiveMessage = "1";
                string negativeMessage = "2";
                string typeIdMessage = "1";
                string idMessage = "8593764";
                string nameMessage = "Cristian Alvarez";
                string emailMessage = "Prueba@prueba.com";
                string phoneMessage = "654196";


                await GetTestFlow()
                    .Send(new Activity()
                    {
                        Type = ActivityTypes.ConversationUpdate,
                        MembersAdded = new List<ChannelAccount>() { new ChannelAccount("gdo") }
                    })
                    .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                    .Send(GeneralUtterances.Consumo_facturado)
                    .AssertReply(activity => Assert.IsTrue(botmessagecontrat.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(contractmessage)
                    .AssertReply(activity => Assert.IsTrue(errorContrato.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(botmessagecontrat.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(secondcontractmessage)
                    .AssertReply(activity => Assert.IsTrue(MessageGasGDO1.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(positiveMessage)
                    .AssertReply(activity => Assert.IsTrue(MessageGasGDO2.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(negativeMessage)
                    .AssertReply(activity => Assert.IsTrue(introductorymessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(typeIdGDOSTG.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(typeIdMessage)
                    .AssertReply(activity => Assert.IsTrue(id.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(idMessage)
                    .AssertReply(activity => Assert.IsTrue(name.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(nameMessage)
                    .AssertReply(activity => Assert.IsTrue(correo.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(emailMessage)
                    .AssertReply(activity => Assert.IsTrue(telefono.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(phoneMessage)
                    .AssertReply(activity => Assert.IsTrue(StartserviceMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(errorResult.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(completedMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .StartTestAsync();
            }

            [TestMethod]
            public async Task Test_VentaSeguros_Intent()
            {
                var StartserviceMessage = AllResponsesTemplates.ExpandTemplate("Startservice");
                var botmessagecontrat = AllResponsesTemplates.ExpandTemplate("Obtaincontract");
                var MessageGasGDO1 = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageGasGDO");
                var MessageGasGDO2 = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageBrillaGDO");
                var introductorymessage = AllResponsesTemplates.ExpandTemplate("Introductorymessage");
                var typeIdGDOSTG = AllResponsesTemplates.ExpandTemplate("GetTypeIdGDOSTG");
                var id = AllResponsesTemplates.ExpandTemplate("GetId");
                var name = AllResponsesTemplates.ExpandTemplate("GetName");
                var correo = AllResponsesTemplates.ExpandTemplate("GetEmail");
                var telefono = AllResponsesTemplates.ExpandTemplate("GetPhone");
                var ventaSegurosResult = AllResponsesTemplates.ExpandTemplate("VentaSegurosResult");
                var completedMessage = AllResponsesTemplates.ExpandTemplate("CompletedMessage");
                var observationmessage = AllResponsesTemplates.ExpandTemplate("GetObservationText");
                var errorResult = AllResponsesTemplates.ExpandTemplate("ErrorResult");

                string contractmessage = "1349573";
                string positiveMessage = "1";
                string negativeMessage = "2";
                string typeIdMessage = "1";
                string idMessage = "8593764";
                string nameMessage = "Cristian Alvarez";
                string emailMessage = "Prueba@prueba.com";
                string phoneMessage = "654196";
                string observation = "Seguro de vida";

                await GetTestFlow()
                    .Send(new Activity()
                    {
                        Type = ActivityTypes.ConversationUpdate,
                        MembersAdded = new List<ChannelAccount>() { new ChannelAccount("gdo") }
                    })
                    .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                    .Send(GeneralUtterances.Adquiere_tu_seguro)
                    .AssertReply(activity => Assert.IsTrue(botmessagecontrat.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(contractmessage)
                    .AssertReply(activity => Assert.IsTrue(MessageGasGDO1.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(positiveMessage)
                    .AssertReply(activity => Assert.IsTrue(MessageGasGDO2.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(negativeMessage)
                    .AssertReply(activity => Assert.IsTrue(introductorymessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(observationmessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(observation)
                    .AssertReply(activity => Assert.IsTrue(typeIdGDOSTG.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(typeIdMessage)
                    .AssertReply(activity => Assert.IsTrue(id.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(idMessage)
                    .AssertReply(activity => Assert.IsTrue(name.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(nameMessage)
                    .AssertReply(activity => Assert.IsTrue(correo.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(emailMessage)
                    .AssertReply(activity => Assert.IsTrue(telefono.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(phoneMessage)
                    .AssertReply(activity => Assert.IsTrue(StartserviceMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(errorResult.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(completedMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .StartTestAsync();
            }

            [TestMethod]
            public async Task Test_VentaSegurosFail_Intent()
            {
                var StartserviceMessage = AllResponsesTemplates.ExpandTemplate("Startservice");
                var botmessagecontrat = AllResponsesTemplates.ExpandTemplate("Obtaincontract");
                var MessageGasGDO1 = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageGasGDO");
                var MessageGasGDO2 = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageBrillaGDO");
                var introductorymessage = AllResponsesTemplates.ExpandTemplate("Introductorymessage");
                var typeIdGDOSTG = AllResponsesTemplates.ExpandTemplate("GetTypeIdGDOSTG");
                var id = AllResponsesTemplates.ExpandTemplate("GetId");
                var name = AllResponsesTemplates.ExpandTemplate("GetName");
                var correo = AllResponsesTemplates.ExpandTemplate("GetEmail");
                var telefono = AllResponsesTemplates.ExpandTemplate("GetPhone");
                var ventaSegurosResult = AllResponsesTemplates.ExpandTemplate("VentaSegurosResult");
                var completedMessage = AllResponsesTemplates.ExpandTemplate("CompletedMessage");
                var observationmessage = AllResponsesTemplates.ExpandTemplate("GetObservationText");
                var errorResult = AllResponsesTemplates.ExpandTemplate("ErrorResult");
                var errorContrato = AllResponsesTemplates.ExpandTemplate("ErrorContrato");

                string contractmessage = "1";
                string secondcontractmessage = "1349573";                
                string positiveMessage = "1";
                string negativeMessage = "2";
                string typeIdMessage = "1";
                string idMessage = "8593764";
                string nameMessage = "Cristian Alvarez";
                string emailMessage = "Prueba@prueba.com";
                string phoneMessage = "654196";
                string observation = "Seguro de vida";

                await GetTestFlow()
                    .Send(new Activity()
                    {
                        Type = ActivityTypes.ConversationUpdate,
                        MembersAdded = new List<ChannelAccount>() { new ChannelAccount("gdo") }
                    })
                    .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                    .Send(GeneralUtterances.Adquiere_tu_seguro)
                    .AssertReply(activity => Assert.IsTrue(botmessagecontrat.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(contractmessage)
                    .AssertReply(activity => Assert.IsTrue(errorContrato.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(botmessagecontrat.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(secondcontractmessage)
                    .AssertReply(activity => Assert.IsTrue(MessageGasGDO1.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(positiveMessage)
                    .AssertReply(activity => Assert.IsTrue(MessageGasGDO2.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(negativeMessage)
                    .AssertReply(activity => Assert.IsTrue(introductorymessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(observationmessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(observation)
                    .AssertReply(activity => Assert.IsTrue(typeIdGDOSTG.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(typeIdMessage)
                    .AssertReply(activity => Assert.IsTrue(id.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(idMessage)
                    .AssertReply(activity => Assert.IsTrue(name.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(nameMessage)
                    .AssertReply(activity => Assert.IsTrue(correo.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(emailMessage)
                    .AssertReply(activity => Assert.IsTrue(telefono.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .Send(phoneMessage)
                    .AssertReply(activity => Assert.IsTrue(StartserviceMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(errorResult.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .AssertReply(activity => Assert.IsTrue(completedMessage.FirstOrDefault().ToString().Contains(activity.AsMessageActivity().Text.ToString())))
                    .StartTestAsync();
            }

            //public async Task Test_Valor_fecha_limite_y_referencia_pago_Intent()
            //{
            //    string expectedContractMessage = "Cuándo se vence mi factura";

            //    string positiveMessage = "si";
            //    string negativeMessage = "no";


            //    await GetTestFlow()
            //        .Send(new Activity()
            //        {
            //            Type = ActivityTypes.ConversationUpdate,
            //            MembersAdded = new List<ChannelAccount>() { new ChannelAccount("gdo") }
            //        })
            //        .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
            //        .Send(GeneralUtterances.Valor_fecha_limite_y_referencia_pago)
            //        .AssertReply(activity => CheckAttachment(activity.AsMessageActivity()))
            //        .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
            //        .Send(positiveMessage)
            //        .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
            //        .Send(negativeMessage)
            //        .AssertReply(activity => Assert.AreEqual(expectedContractMessage, activity.AsMessageActivity().Text))
            //        .StartTestAsync();
            //}

            //public void CheckAttachment(IMessageActivity messageActivity)
            //{
            //    // Check if content is the same
            //    var messageAttachment = messageActivity.Attachments.First();
            //    // Example attachment
            //    var adaptiveCardJson = File.ReadAllText(@".\Resources\TicketForm.json");

            //    var expected = new Attachment()
            //    {
            //        ContentType = "application/vnd.microsoft.card.adaptive",
            //        Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            //    };

            //    Assert.AreEqual(messageAttachment.Content.ToString(), expected.Content.ToString());
            //}
        }
    }
}
