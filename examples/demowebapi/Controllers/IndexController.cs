using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace WindowsServiceTemplate
{
    public class IndexController : ApiController
    {
        public HttpResponseMessage Get()
        {
            string content = string.Empty;
            if (File.Exists(Config.IndexFile))
                content = File.ReadAllText(Config.IndexFile);
            else
                content = Config.IndexFile + " not found";

            // Create a 200 response.
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            };
            response.Content.Headers.ContentType.MediaType = "text/html";
            return response;
        }


    }
}
