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
    public static class SendWhatsappMessage
    {
        [FunctionName("SendWhatsappMessage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            TwilioClient.Init(GetEnvironmentVariable("TWILIO_ACCOUNTSID"), GetEnvironmentVariable("TWILIO_AUTHTOKEN"));
            CosmosClient cliente = new CosmosClient(GetEnvironmentVariable("COSMOS_ACCOUNT"), GetEnvironmentVariable("COSMOS_KEY"));
            var container = cliente.GetContainer(GetEnvironmentVariable("COSMOS_DATABASENAME"), GetEnvironmentVariable("COSMOS_CONTAINERMAE"));
            DatabaseResponse database = await cliente.CreateDatabaseIfNotExistsAsync(GetEnvironmentVariable("COSMOS_DATABASENAME"));
            await database.Database.CreateContainerIfNotExistsAsync(GetEnvironmentVariable("COSMOS_CONTAINERMAE"), "/id");
            var data = await req.Content.ReadAsStringAsync();

            Activity activity = JsonConvert.DeserializeObject<Activity>(data);

            var sqlQueryText = String.Format("SELECT c.id, c.Name, c.Phone, " +
                        "c.ConversationId, c.Token, c.Interactions  " +
                        "FROM c Where c.ConversationId = '{0}'",activity.From.Id);

            

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<UsersTwilio> queryResultSetIterator = container.GetItemQueryIterator<UsersTwilio>(queryDefinition);

            List<UsersTwilio> usersTwilio = new List<UsersTwilio>();

            while (queryResultSetIterator.HasMoreResults)
            {

                    FeedResponse<UsersTwilio> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (UsersTwilio user in currentResultSet)
                    {
                        usersTwilio.Add(user);
                    }
            }
            if (usersTwilio.Count() != 0)
            {
                var responsemessage = MessageResource.Create(
                            body: activity.Text,
                            from: new Twilio.Types.PhoneNumber("whatsapp:+14155238886"),
                            to: new Twilio.Types.PhoneNumber("whatsapp:+" + usersTwilio.Last().Phone));

            }
            

            return new OkObjectResult(null);
        }
        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
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
    
}
