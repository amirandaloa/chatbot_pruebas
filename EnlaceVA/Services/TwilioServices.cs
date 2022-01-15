using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EnlaceVA.Services
{
    public class TwilioServices : IDisposable
    {
        private readonly int Minutes = 1;
        private readonly HttpClient _client;
        public TwilioServices(string uri)
        {

            _client = new HttpClient();
            _client.BaseAddress = new Uri(uri);

            _client.Timeout = TimeSpan.FromMinutes(Minutes);

            _client.DefaultRequestHeaders.Accept.Clear();

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        public void Dispose()
        {
            this._client.Dispose();
        }

        public async Task<HttpResponseMessage> Post<T>(string Route, T entity)
        {
            try
            {

                HttpResponseMessage response =
                       await _client.PostAsync(Route, new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json"));
                return response;
            }
            catch (System.OperationCanceledException)
            {
                HttpResponseMessage TmpResponse = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.RequestTimeout

                };
                return TmpResponse;
            }
        }
    }
}
