using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnlaceVA.Models;
using EnlaceVA.Tests;

namespace EnlaceVA.Tests
{
    [TestClass]
    [TestCategory("FunctionalTests")]
    public class DirectLineClientTests : BotTestBase
    {
        private static readonly string TestName = "gdo";

        private static string _directLineSecret = string.Empty;
        private static string _botId = string.Empty;
        private static DirectLineClient _client;

        [TestInitialize]
        public void Test_Initialize()
        {
            GetEnvironmentVars();

            _client = new DirectLineClient(_directLineSecret);
        }

        [TestMethod]
        public async Task Test_Greeting()
        {
            string fromUser = Guid.NewGuid().ToString();

            await Assert_New_User_Greeting(fromUser);
        }

        [TestMethod]
        public async Task Test_QnAMaker()
        {
            string fromUser = Guid.NewGuid().ToString();

            await Assert_QnA_ChitChat_Responses(fromUser);
        }

        [TestMethod]
        public async Task Test_TrataMientoDatosDialog()
        {
            string fromUser = Guid.NewGuid().ToString();

            await Assert_TratamientoDatos_Responses(fromUser);
        }

        public async Task Assert_New_User_Greeting(string fromUser)
        {
            var profileState = new UserProfileState { Name = TestName.ToUpper() };

            var allNamePromptVariations = AllResponsesTemplates.ExpandTemplate("NewUserMessage", profileState);
            var conversation = await StartBotConversationAsync();

            var responses = await SendActivityAsync(conversation, CreateStartConversationEvent(fromUser));

            Assert.AreEqual(allNamePromptVariations.FirstOrDefault().ToString(), responses[0].Text);
        }

        public async Task Assert_Returning_User_Greeting(string fromUser)
        {
            var conversation = await StartBotConversationAsync();

            var responses = await SendActivityAsync(conversation, CreateStartConversationEvent(fromUser));

            Assert.AreEqual(2, responses.Count());

            Assert.AreEqual(ActivityTypes.Message, responses[0].GetActivityType());
            Assert.AreEqual(ActivityTypes.Message, responses[1].GetActivityType());

            Assert.AreEqual(1, responses[0].Attachments.Count);
            Assert.AreEqual("application/vnd.microsoft.card.adaptive", responses[0].Attachments[0].ContentType);
        }

        public async Task Assert_QnA_ChitChat_Responses(string fromUser)
        {
            string testChitChatMessage = "como te llamas";
            string testFaqMessage = "How do I raise a bug?";

            string expectedChitChatMessage = "Soy Clari asesora virtual";
            string expectedFaqMessage = "Lo siento, no encuentro respuesta para tu pregunta. Por favor, intenta preguntar de una manera diferente.";

            var conversation = await StartBotConversationAsync();

            var responses = await SendActivityAsync(conversation, CreateMessageActivity(fromUser, testChitChatMessage));
            Assert.AreEqual(responses[1].Text, expectedChitChatMessage);

            responses = await SendActivityAsync(conversation, CreateMessageActivity(fromUser, testFaqMessage));
            Assert.AreEqual(responses[2].Text, expectedFaqMessage);
        }
        public async Task Assert_TratamientoDatos_Responses(string fromUser)
        {
            string firtsMessage = "Abono a tu factura";
            string positiveMessage = "1";
            string negativeMessage = "2";
            string contractMessage = "5656265";
            string nameMessage = "juan";
            string emailMessage = "juangmail.com";
            string phoneMessage = "625649865";
            string billValueMessage = "634586416";

            string expectedTratamientoDatos = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageGasGDO").FirstOrDefault().ToString();
            string expectedTratamientodatos2 = AllResponsesTemplates.ExpandTemplate("TratamientoDatosMessageBrillaGDO").FirstOrDefault().ToString();
            string expectedContractMessage = "Si necesitas realizar un pago anticipado/prepago de tu factura, ingresa el número de contrato";
            string expectedNameMessage = "Por favor digite su nombre";
            string expectedEmailMessage = "Por favor digite su correo electronico";
            string expectedPhoneMessage = "Por favor digite un numero celular";
            string expectedBillValueMessage = "Por favor digite el valor para el cupon";
            string expectedAbonaatufactura = AllResponsesTemplates.ExpandTemplate("AbonaatufacturaTemporalMessage").FirstOrDefault().ToString();
            string expectedCompletedMessage = AllResponsesTemplates.ExpandTemplate("CompletedMessage").FirstOrDefault().ToString();

            var conversation = await StartBotConversationAsync();

            var responses = await SendActivityAsync(conversation, CreateMessageActivity(fromUser, firtsMessage));
            Assert.AreEqual(responses[1].Text, expectedTratamientoDatos);

            responses = await SendActivityAsync(conversation, CreateMessageActivity(fromUser, positiveMessage));
            Assert.AreEqual(responses[2].Text, expectedTratamientodatos2);

            responses = await SendActivityAsync(conversation, CreateMessageActivity(fromUser, negativeMessage));
            Assert.AreEqual(responses[3].Text, expectedContractMessage);

            responses = await SendActivityAsync(conversation, CreateMessageActivity(fromUser, contractMessage));
            Assert.AreEqual(responses[4].Text, expectedNameMessage);

            responses = await SendActivityAsync(conversation, CreateMessageActivity(fromUser, nameMessage));
            Assert.AreEqual(responses[5].Text, expectedEmailMessage);

            responses = await SendActivityAsync(conversation, CreateMessageActivity(fromUser, emailMessage));
            Assert.AreEqual(responses[6].Text, expectedPhoneMessage);

            responses = await SendActivityAsync(conversation, CreateMessageActivity(fromUser, phoneMessage));
            Assert.AreEqual(responses[7].Text, expectedBillValueMessage);

            responses = await SendActivityAsync(conversation, CreateMessageActivity(fromUser, billValueMessage));
            Assert.AreEqual(responses[8].Text, expectedAbonaatufactura);
            Assert.AreEqual(responses[9].Text, expectedCompletedMessage);

        }
        private static Activity CreateStartConversationEvent(string fromUser)
        {
            return new Activity
            {
                From = new ChannelAccount(fromUser, TestName),
                Name = "startConversation",
                Type = ActivityTypes.Event
            };
        }

        private static Activity CreateMessageActivity(string fromUser, string activityText)
        {
            return new Activity
            {
                From = new ChannelAccount(fromUser, TestName),
                Text = activityText,
                Type = ActivityTypes.Message
            };
        }

        private static async Task<Microsoft.Bot.Connector.DirectLine.Conversation> StartBotConversationAsync()
        {
            return await _client.Conversations.StartConversationAsync();
        }

        private static async Task<List<Activity>> SendActivityAsync(Microsoft.Bot.Connector.DirectLine.Conversation conversation, Activity activity)
        {
            await _client.Conversations.PostActivityAsync(conversation.ConversationId, activity);

            var responses = await ReadBotMessagesAsync(_client, conversation.ConversationId);

            return responses;
        }

        private static async Task<List<Activity>> ReadBotMessagesAsync(DirectLineClient client, string conversationId)
        {
            string watermark = null;
            List<Activity> botResponses = null;

            while (botResponses == null)
            {
                var activitySet = await client.Conversations.GetActivitiesAsync(conversationId, watermark);
                watermark = activitySet?.Watermark;

                var activities = from x in activitySet.Activities
                                 where x.From.Id == _botId
                                 select x;

                if (activities.Any())
                {
                    botResponses = activities.ToList();
                }

                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                return botResponses;
            }

            return botResponses;
        }

        private void GetEnvironmentVars()
        {
            if (string.IsNullOrWhiteSpace(_directLineSecret) || string.IsNullOrWhiteSpace(_botId))
            {
                _directLineSecret = "VoCocwgqU5k.uYv03jJtB_i25auxsrjW5DDMmBhSS9PcTd6Hck32FoY";
                if (string.IsNullOrWhiteSpace(_directLineSecret))
                {
                    Assert.Inconclusive("Environment variable 'DIRECTLINE' not found.");
                }

                _botId = "VAEnlacePromDev-WepAppBot";
                if (string.IsNullOrWhiteSpace(_botId))
                {
                    Assert.Inconclusive("Environment variable 'BOTID' not found.");
                }
            }
        }
    }
}