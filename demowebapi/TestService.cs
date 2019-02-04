using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace WindowsServiceTemplate
{
    /// <summary>
    /// Example service, rename and modify it to suit your needs
    /// </summary>
    public class TestService
    {
        Thread t;
        HttpSelfHostServer _server;
        public TestService()
        {
            // set allowed protocols for WebClient (it does not work with https url otherwise by a reason)
            // https://ru.stackoverflow.com/questions/480370/%D0%9A%D0%B0%D0%BA-%D0%B8%D1%81%D0%BF%D1%80%D0%B0%D0%B2%D0%B8%D1%82%D1%8C-%D0%9D%D0%B5-%D1%83%D0%B4%D0%B0%D0%BB%D0%BE%D1%81%D1%8C-%D1%81%D0%BE%D0%B7%D0%B4%D0%B0%D1%82%D1%8C-%D0%B7%D0%B0%D1%89%D0%B8%D1%89%D0%B5%D0%BD%D0%BD%D1%8B%D0%B9-%D0%BA%D0%B0%D0%BD%D0%B0%D0%BB-ssl-tls
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var config = new HttpSelfHostConfiguration("http://" + Config.EndPoint);

            // set max demo file size
            config.MaxReceivedMessageSize = Config.MaxUploadSize; // 10MB

            config.Routes.MapHttpRoute(
                "DefaultApi", "{controller}/{id}",
                defaults: new { controller = "Index", id = RouteParameter.Optional });

            // set JSON response type by force
            var jsonFormatter = new JsonMediaTypeFormatter();
            //optional: set serializer settings here
            config.Services.Replace(typeof(IContentNegotiator), new JsonContentNegotiator(jsonFormatter));

            _server = new HttpSelfHostServer(config);

            t = new Thread(doWork);
        }

        public void Start()
        {
            t.Start();
        }
        public void Stop()
        {
            Log.Info("Stopping service");
            _server.CloseAsync().Wait();
            _server.Dispose();
            t.Abort();
        }

        private void doWork()
        {
            _server.OpenAsync();
            Log.Info("Running servica at " + Config.EndPoint);
        }


        public class JsonContentNegotiator : IContentNegotiator
        {
            private readonly JsonMediaTypeFormatter _jsonFormatter;

            public JsonContentNegotiator(JsonMediaTypeFormatter formatter)
            {
                _jsonFormatter = formatter;
            }

            public ContentNegotiationResult Negotiate(
                    Type type,
                    HttpRequestMessage request,
                    IEnumerable<MediaTypeFormatter> formatters)
            {
                return new ContentNegotiationResult(
                    _jsonFormatter,
                    new MediaTypeHeaderValue("application/json"));
            }
        }


    }
}