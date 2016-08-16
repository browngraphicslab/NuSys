using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;

namespace NusysServer.Controllers
{
    public class NusysConnectController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        /// <summary>
        /// called to instatiate a new custom websockethandler.
        /// the user passes in valid credentials (session id), a new websockethandler is made
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(string id)
        {
            var currentContext = HttpContext.Current;
            if (currentContext.IsWebSocketRequest || currentContext.IsWebSocketRequestUpgrading)
            {
                currentContext.Items.Add("id", id);
                currentContext.AcceptWebSocketRequest(ProcessWebsocketSession);
            }

            var response = Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
            return response;
        }
        private Task ProcessWebsocketSession(AspNetWebSocketContext context)
        {
            string id = (string)(context.Items["id"]);
            var handler = new NuWebSocketHandler();
            if (NusysClient.FetchAwaitingSession(id, handler))
            {
                var processTask = handler.ProcessWebSocketRequestAsync(context);
                return processTask;
            }
            handler.Close();
            return null;
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
