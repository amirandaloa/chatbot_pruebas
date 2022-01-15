
using EnlaceVA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;



namespace EnlaceVA.Services
{
    public class SoapClient : IDisposable
    {

        private readonly HttpClient _client;
        private readonly string _serviceUrl;
        private readonly string _serviceNamespace;

        public SoapClient(string serviceUrl, string serviceNamespace)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("text/xml"));
            _serviceUrl = serviceUrl;
            _serviceNamespace = serviceNamespace;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes("GRGDOCHATBOT:3nl4c3CSC2o19$")));
        }

        private void SetSoapAction(string method)
        {
            const string header = "SOAPAction";
            if (_client.DefaultRequestHeaders.Contains(header))
            {
                _client.DefaultRequestHeaders.Remove(header);
            }
            _client.DefaultRequestHeaders.Add(header,
                $"{_serviceNamespace}{(_serviceNamespace.EndsWith("/") ? "" : "/")}{method}");
        }

        private IEnumerable<XElement> SerializeData(object data)
        {
            XElement xml = data?.ToXml();
            return xml?.Elements();
        }

        public async Task<HttpResponseMessage> PostAsync(string method, string methodResult, object data = null)
        {

            SetSoapAction(method);
            XNamespace xmlns = _serviceNamespace;
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";

            var xmlRequest = new XDocument(
                new XDeclaration("1.1", "utf-8", null),
                new XElement(soap + "Envelope",
                        new XAttribute(XNamespace.Xmlns + "soapenv", soap),
                        new XAttribute(XNamespace.Xmlns + "urn", _serviceNamespace),

                new XElement(soap + "Header"),
                    new XElement(soap + "Body",
                     new XElement(xmlns + method,
                        new XElement("datoEntrada",
                            SerializeData(data)
                        )
                    )
                )
                )

            ).ToString();
            HttpResponseMessage response;
            try
            {
                response = await _client.PostAsync(_serviceUrl,
                    new StringContent(xmlRequest, Encoding.UTF8, "text/xml"));

                return response;
            }
            catch (System.OperationCanceledException)
            {
                response = new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.RequestTimeout
                };
            }
            catch (HttpRequestException)
            {
                response = new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                };
            }

            return response;
        }


        public void Post(string method, string methodResult, object data = null)
        {
            PostAsync(method, methodResult, data).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
