using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnlaceVA.Models;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Rest;

namespace EnlaceVA.Controllers
{
    public static class TextAnaliticsController
    {
        public static async Task<string> RunAsync(List<Question> question)
        {
            try
            {
                double resultSentiment = 0;
                int numberQuestion = 0;
                string analysis = "";
                List<MultiLanguageInput> questionSentiment = new List<MultiLanguageInput>();


                const string endPoint = "https://cursotextanalytics.cognitiveservices.azure.com/";
                const string secretkey = "a7ba028561294a1fb18c4779ea997dfc";

                var credentials = new ApiKeyServiceClientCredentials(secretkey);
                var client = new TextAnalyticsClient(credentials)
                {
                    Endpoint = endPoint
                };
                int id = 0;
                if (question!=null)
                {
                    foreach (var item in question)
                    {
                        if (item.Text != "No")
                        {
                            id += 1;
                            questionSentiment.Add(new MultiLanguageInput(id.ToString(CultureInfo.CurrentCulture), item.Text, "es"));
                        }
                    }

                    var inputDocuments = new MultiLanguageBatchInput(
                    questionSentiment);

                    var result = await client.SentimentBatchAsync(inputDocuments);

                    foreach (var document in result.Documents)
                    {
                        resultSentiment += document.Score.Value;
                        numberQuestion += 1;
                    }
                    resultSentiment = resultSentiment / numberQuestion;

                    const double minimum = 0.333;
                    const double medium = 0.666;

                    if (resultSentiment <= minimum)
                    {
                        analysis = SentimentScale.Negativo.ToString();
                    }
                    else if (resultSentiment <= medium )
                    {
                        analysis = SentimentScale.Neutral.ToString();
                    }
                    else
                    {
                        analysis = SentimentScale.Positivo.ToString();
                    }
                }
                

                
                return analysis;
            }
            catch (Exception)
            {
                // 2 is a identifier to error.
                return null;
            }
        }
        public enum SentimentScale
        {
            Positivo,
            Negativo,
            Neutral
        }
    }
    class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private readonly string subscriptionKey;

        /// <summary>
        /// Creates a new instance of the ApiKeyServiceClientCredentails class
        /// </summary>
        /// <param name="subscriptionKey">The subscription key to authenticate and authorize as</param>
        public ApiKeyServiceClientCredentials(string subscriptionKey)
        {
            this.subscriptionKey = subscriptionKey;
        }

        /// <summary>
        /// Add the Basic Authentication Header to each outgoing request
        /// </summary>
        /// <param name="request">The outgoing request</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            request.Headers.Add("Ocp-Apim-Subscription-Key", this.subscriptionKey);

            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
        
    }
    
}
