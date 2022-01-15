using EnlaceVA.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Solutions.Responses;
using RichardSzalay.MockHttp;
using System;
using System.IO;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EnlaceVA.Tests.Mocks
{
    public class MockTratamientoDialog: TratamientoDialog
    {
        private readonly MockHttpMessageHandler _mockHttpHandler = new MockHttpMessageHandler();
        private readonly LocaleTemplateManager _templateManager;

        public MockTratamientoDialog(IServiceProvider serviceProvider, IBotTelemetryClient telemetryClient)
            : base(serviceProvider, telemetryClient)
        {
            // All calls to Generate Answer regardless of host or knowledgebaseId are captured
            _mockHttpHandler.When(HttpMethod.Post, "*/knowledgebases/*/generateanswer")
              .Respond("application/json", GetResponse("QnAMaker_NoAnswer.json"));

            _templateManager = serviceProvider.GetService<LocaleTemplateManager>();
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
