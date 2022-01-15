using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Azure.Cosmos;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace TwilioIntegrationFunction
{
    public static class TwilioIntegrationFunction
    {
        [FunctionName("TwilioIntegrationFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            Activity message = new Activity();
            TwilioClient.Init(GetEnvironmentVariable("TWILIO_ACCOUNTSID"), GetEnvironmentVariable("TWILIO_AUTHTOKEN"));
            CosmosClient cliente = new CosmosClient(GetEnvironmentVariable("COSMOS_ACCOUNT"), GetEnvironmentVariable("COSMOS_KEY"));
            var container = cliente.GetContainer(GetEnvironmentVariable("COSMOS_DATABASENAME"), GetEnvironmentVariable("COSMOS_CONTAINERMAE"));
            DatabaseResponse database = await cliente.CreateDatabaseIfNotExistsAsync(GetEnvironmentVariable("COSMOS_DATABASENAME"));
            await database.Database.CreateContainerIfNotExistsAsync(GetEnvironmentVariable("COSMOS_CONTAINERMAE"), "/id");

            string token = "";
            string conversationId = "";

            var data = await req.Content.ReadAsStringAsync();
            var formValues = data.Split('&')
                 .Select(value => value.Split('='))
                .ToDictionary(pair => Uri.UnescapeDataString(pair[0]).Replace("+", " "),
                             pair => Uri.UnescapeDataString(pair[1]).Replace("+", " "));

            string body = formValues["Body"];
            string celphone = formValues["From"].Replace("whatsapp: ", "");

            ResponseMessage response = await container.ReadItemStreamAsync(celphone, new PartitionKey(celphone));
            UsersTwilio userSearch = null;


            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                userSearch = await container.ReadItemAsync<UsersTwilio>(celphone, new PartitionKey(celphone));
            }


            var client = new DirectLineClient(GetEnvironmentVariable("DIRECTLINE_SECRETKEY"));

            if (userSearch != null)
            {
                message.From = new ChannelAccount(userSearch.ConversationId, "stg");
                message.Text = body;
                message.Type = ActivityTypes.Message;
                
                
                await client.Conversations.PostActivityAsync(userSearch.ConversationId, message);
               
                var messageOldUser = await client.Conversations.GetActivitiesAsync(userSearch.ConversationId);

                userSearch.Interactions = userSearch.Interactions + 1;
                await container.UpsertItemAsync<UsersTwilio>(userSearch, new PartitionKey(userSearch.Id));
                string uniqueResonse = "";


                List<string> responseOldUser = new List<string>();
                if (userSearch.Interactions >= 2)
                {
                    for (int i = messageOldUser.Activities.Count - 2; i <= messageOldUser.Activities.Count - 1; i++)
                    {
                        if (messageOldUser.Activities[i].From.Name == GetEnvironmentVariable("BOT_NAME"))
                        {
                            log.LogInformation("Mensaje " + messageOldUser.Activities[i].Text);
                            if (!responseOldUser.Any(item => item == messageOldUser.Activities[i].Text))
                            {
                                responseOldUser.Add(messageOldUser.Activities[i].Text);
                                log.LogInformation("Mensaje " + "Prueba 2");
                                List<Uri> mediaUrl = null;
                                if (messageOldUser.Activities[i].Attachments != null)
                                {
                                    if (messageOldUser.Activities[i].Attachments.Any())
                                    {
                                        Attachment attachment = messageOldUser.Activities[i].Attachments[0];
                                        mediaUrl = new[] { new Uri(attachment.ContentUrl) }.ToList();
                                    }
                                }

                                var responseOldmessage = MessageResource.Create(
                            body: messageOldUser.Activities[i].Text,
                            from: new Twilio.Types.PhoneNumber("whatsapp:+14155238886"),
                            mediaUrl: mediaUrl,
                            to: new Twilio.Types.PhoneNumber("whatsapp:+" + userSearch.Phone));
                                await Task.Delay(2000);
                            
                        }
                        }
                    }
                }
                else
                {
                    for (int i = messageOldUser.Activities.Count - 1; i > -1; i--)
                    {
                        if (messageOldUser.Activities[i].From.Name == GetEnvironmentVariable("BOT_NAME"))
                        {
                            uniqueResonse = messageOldUser.Activities[i].Text;
                        }
                        else
                        {
                            break;
                        }
                    }
                }


                return new OkObjectResult(uniqueResonse);
            }
            else
            {
                List<string> responseNewUser = new List<string>();
                Conversation conversation = new Conversation();
                conversation = client.Conversations.StartConversation();
                
                conversationId = conversation.ConversationId;
                token = conversation.Token;
                UsersTwilio user = new UsersTwilio(celphone, "Juan", celphone, conversationId, token, 1);
                message.From = new ChannelAccount(conversationId, "stg");
                message.Text = null;
                message.Type = ActivityTypes.Message;
                
                await client.Conversations.PostActivityAsync(conversationId, message);
                await container.CreateItemAsync<UsersTwilio>(user, new PartitionKey(user.Id));

                var messagesNewUser = await client.Conversations.GetActivitiesAsync(conversationId);

                for (int i = 0; i <= messagesNewUser.Activities.Count - 1; i++)
                {
                    if (messagesNewUser.Activities[i].From.Name == GetEnvironmentVariable("BOT_NAME"))
                    {
                        log.LogInformation("Mensaje " + messagesNewUser.Activities[i].Text);
                        if (!responseNewUser.Any(item => item == messagesNewUser.Activities[i].Text))
                        {
                            responseNewUser.Add(messagesNewUser.Activities[i].Text);
                            var responsemessage = MessageResource.Create(
                            body: messagesNewUser.Activities[i].Text,
                            from: new Twilio.Types.PhoneNumber("whatsapp:+14155238886"),
                            to: new Twilio.Types.PhoneNumber("whatsapp:+"+celphone));
                        }
                    }

                }
                return new OkObjectResult(null);
            }


        }
        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }

    public class UsersTwilio
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string ConversationId { get; set; }
        public string Token { get; set; }
        public int Interactions { get; set; }


        public UsersTwilio(string id, string name, string phone, string conversationId, string token, int interactions)
        {
            Id = id;
            Name = name;
            Phone = phone;
            ConversationId = conversationId;
            Token = token;
            Interactions = interactions;
        }
    }


}

