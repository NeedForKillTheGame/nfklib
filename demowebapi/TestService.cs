using System;
using System.Collections.Generic;
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
            Log.Info("Stopping servica");
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