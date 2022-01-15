using EnlaceVA.Dialogs;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Solutions;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using System;
using System.IO;
using System.Net.Http;

namespace EnlaceVA.Tests.Mocks
{
    public class MockMainDialog : MainDialog
    {
        private readonly MockHttpMessageHandler _mockHttpHandler = new MockHttpMessageHandler();
        private readonly MockHttpMessageHandler _mockHttpHandlerChistchat = new MockHttpMessageHandler();
        private readonly LocaleTemplateManager _templateManager;

        public MockMainDialog(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            // All calls to Generate Answer regardless of host or knowledgebaseId are captured
            _mockHttpHandler.When(HttpMethod.Post, "*/knowledgebases/*/generateanswer")
              .Respond("application/json", GetResponse("QnAMaker_NoAnswer.json"));

            _mockHttpHandlerChistchat.When(HttpMethod.Post, "*/knowledgebases/*/generateanswer")
              .Respond("application/json", GetResponse("ChitChat_NoAnswer.json"));

            _templateManager = serviceProvider.GetService<LocaleTemplateManager>();
        }

        protected override QnAMakerDialog TryCreateQnADialog(string knowledgebaseId, CognitiveModelSet cognitiveModels, string id)
        {
            if (!cognitiveModels.QnAConfiguration.TryGetValue(knowledgebaseId, out QnAMakerEndpoint qnaEndpoint)
                          || qnaEndpoint == null)
            {
                throw new Exception($"Could not find QnA Maker knowledge base configuration with id: {knowledgebaseId}.");
            }

            // QnAMaker dialog already present on the stack?
            if (Dialogs.Find(knowledgebaseId) == null && knowledgebaseId == "Chitchat")
            {
                // Return a QnAMaker dialog using our Http Mock
                return new QnAMakerDialog(
                    knowledgeBaseId: qnaEndpoint.KnowledgeBaseId,
                    endpointKey: qnaEndpoint.EndpointKey,
                    hostName: qnaEndpoint.Host,
                    noAnswer: _templateManager.GenerateActivityForLocale("FirstErrorMessage"),
                    httpClient: new HttpClient(_mockHttpHandlerChistchat))
                {
                    Id = knowledgebaseId
                };
            }
            else
            {
                return new QnAMakerDialog(
                    knowledgeBaseId: qnaEndpoint.KnowledgeBaseId,
                    endpointKey: qnaEndpoint.EndpointKey,
                    hostName: qnaEndpoint.Host,
                    noAnswer: _templateManager.GenerateActivityForLocale("FirstErrorMessage"),
                    httpClient: new HttpClient(_mockHttpHandler))
                {
                    Id = knowledgebaseId
                };
            }
        }

        private Stream GetResponse(string fileName)
        {
            var path = GetFilePath(fileName);
            return File.OpenRead(path);
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(Environment.CurrentDirectory, "TestData", fileName);
        }
    }
}
