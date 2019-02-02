using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net;
using nfklib;
using System.IO;
using System.Net.Http;
using System.Web;
using System.ServiceModel.Channels;
namespace WindowsServiceTemplate
{
    public class DemoController : ApiController
    {
        public async Task<Demo> Post(bool full = false)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            // read file data into a memory stream
            // Create a stream provider for setting up output streams
            var streamProvider = new MultipartMemoryStreamProvider();
            // Read the MIME multipart asynchronously content using the stream provider we just created.
            await Request.Content.ReadAsMultipartAsync(streamProvider);
            var stream = streamProvider.Contents[0].ReadAsStreamAsync().Result;

            Demo data = new Demo();
            nfklib.NDemo.DemoItem demo = null;
            var ndm = new nfklib.NDemo.NFKDemo();
            try
            {
                var fsize = stream.Length;
                demo = ndm.Read(stream);
                if (demo == null)
                    throw new Exception("Bad demo file");

                // fix file name
                data.FileName = (!string.IsNullOrEmpty(streamProvider.Contents[0].Headers.ContentDisposition.FileName))
                    ? Path.GetFileName(streamProvider.Contents[0].Headers.ContentDisposition.FileName.Trim('"'))
                    : "(stream)";


                data.Duration = demo.Duration;
                data.Players = new Demo.PlayerItem[demo.Players.Count];


                for (int i = 0; i < demo.Players.Count; i++)
                {
                    data.Players[i].ID = (byte)(i + 1);
                    data.Players[i].RealName = Helper.GetRealNick(Helper.GetDelphiString(demo.Players[i].netname));
                    data.Players[i].PlayerInfo = demo.Players[i];
                    // fix delphi strings
                    data.Players[i].PlayerInfo.netname = Helper.GetDelphiString(data.Players[i].PlayerInfo.netname);
                    data.Players[i].PlayerInfo.modelname = Helper.GetDelphiString(data.Players[i].PlayerInfo.modelname);

                    foreach (var s in demo.PlayerStats)
                        if (s.DXID == demo.Players[i].DXID)
                            data.Players[i].PlayerStats = s;
                }

                if (full)
                {
                    data.DemoUnits = demo.DemoUnits;
                    data.Map = demo.Map;
                }

                // fix delphi strings
                data.Version = demo.Map.Header.Version;
                data.MapInfo = demo.Map.Header;
                data.MapInfo.MapName = Helper.GetDelphiString(data.MapInfo.MapName);
                data.MapInfo.Author = Helper.GetDelphiString(data.MapInfo.Author);

                object property;
                Request.Properties.TryGetValue(typeof(RemoteEndpointMessageProperty).FullName, out property);
                var remoteProperty = property as RemoteEndpointMessageProperty;
                Log.Info(string.Format("Request from {0} - {1} ({2} bytes)", remoteProperty.Address.ToString(), data.FileName, fsize));
            }
            catch (Exception e)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, e.Message));
            }
            return data;
        }

    }
}
