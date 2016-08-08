using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Newtonsoft.Json;

namespace NusysServer
{
    public class LoginController : ApiController
    {
        /*
        public static RSACryptoServiceProvider RSA;*/
        // GET api/<controller>
        public long Get()
        {
            var timestamp = DateTime.UtcNow.Ticks;
            //var xmlString = RSA.ToXmlString(false);
            return timestamp;
        }

        // GET api/<controller>/5
        public bool Get(string cred)
        {
            return true;
        }

        // POST api/<controller>
        public async Task<string> Post(HttpRequestMessage value)
        {
            var data = await value.Content.ReadAsStringAsync();
            JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data, settings);
            if (!dict.ContainsKey("user") || !dict.ContainsKey("pass") || !dict.ContainsKey("timestamp"))
            {
                throw new Exception("invalid login dictionary");
            }
            if (long.Parse(dict["timestamp"]) + 30000000 < DateTime.UtcNow.Ticks)
            {
                throw new Exception("timestamp expired");
            }
            else
            {
                var returnDict = new Dictionary<string, string>();
                var tup = NusysLogins.Validate(dict["user"], dict["pass"], dict.ContainsKey("new_user"));
                returnDict[Constants.VALID_CREDENTIALS_BOOLEAN_STRING] = tup.Item1.ToString();
                if (tup.Item1)
                {
                    var sessionString = Guid.NewGuid().ToString();
                    returnDict[Constants.SERVER_SESSION_ID_STRING] = sessionString;
                    returnDict["user_id"] = tup.Item2.ID;
                    ActiveClient.WaitForClient(sessionString, tup.Item2);
                }
                return JsonConvert.SerializeObject(returnDict, settings);
            }
        }

        // PUT api/<controller>/5
        public bool Put(string id, [FromBody]string value)
        {
            return true;
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
        /*
        public static void AssignNewKey()
        {
            const int PROVIDER_RSA_FULL = 1;
            const string CONTAINER_NAME = "KeyContainer";
            CspParameters cspParams;
            cspParams = new CspParameters(PROVIDER_RSA_FULL);
            cspParams.KeyContainerName = CONTAINER_NAME;
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            cspParams.ProviderName = "Microsoft Strong Cryptographic Provider";
            RSA = new RSACryptoServiceProvider(cspParams);
            RSA.PersistKeyInCsp = false;
        }*/
    }
}