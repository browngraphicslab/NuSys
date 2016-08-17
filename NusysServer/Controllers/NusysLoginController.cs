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
using NusysIntermediate;

namespace NusysServer
{
    public class NusysLoginController : ApiController
    {
        /*
        public static RSACryptoServiceProvider RSA;*/
        // GET api/<controller>
        public long Get()
        {
            var timestamp = DateTime.UtcNow.Ticks;
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

            //must contain user and passwrod for login attempt
            if (!dict.ContainsKey("user") || !dict.ContainsKey("pass"))
            {
                throw new Exception("invalid login dictionary");
            }
            var returnDict = new Dictionary<string, string>();

            Tuple<bool, NusysClient> returnTuple;
            if (dict.ContainsKey("new_user"))
            {
                try
                { 
                    var client = NusysLogins.CreateNewUser(dict["user"], dict["pass"], dict["display_name"]);
                    if (client != null)
                    {
                        returnTuple = new Tuple<bool, NusysClient>(true, client);
                    }
                    else
                    {
                        returnDict[Constants.VALID_CREDENTIALS_BOOLEAN_STRING] = false.ToString();
                        returnDict["error_message"] = "Couldn't create a new user";
                        return JsonConvert.SerializeObject(returnDict, settings);
                    }
                }
                catch (Exception e)
                {
                    returnDict[Constants.VALID_CREDENTIALS_BOOLEAN_STRING] = false.ToString();
                    returnDict["error_message"] = e.Message;
                    return JsonConvert.SerializeObject(returnDict, settings);
                }
            }
            else
            {

                try
                {
                    returnTuple = NusysLogins.Validate(dict["user"], dict["pass"]);
                }
                catch (Exception e)
                {
                    returnDict[Constants.VALID_CREDENTIALS_BOOLEAN_STRING] = false.ToString();
                    returnDict["error_message"] = e.Message;
                    return JsonConvert.SerializeObject(returnDict, settings);
                }
            }

            //add to the returning dictionary whether it was a valid attmept to login
            returnDict[Constants.VALID_CREDENTIALS_BOOLEAN_STRING] = returnTuple.Item1.ToString();
            if (returnTuple.Item1)
            {
                //if it was valid, add the session id
                var sessionString = NusysConstants.GenerateId();
                returnDict[Constants.SERVER_SESSION_ID_STRING] = sessionString;
                returnDict["user_id"] = returnTuple.Item2.UserID;
                NusysClient.WaitForClient(sessionString, returnTuple.Item2);
            }
            return JsonConvert.SerializeObject(returnDict, settings);
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