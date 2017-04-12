using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace NusysServer.Controllers
{
    public class UploadWordDocController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public async Task<string> Post(HttpRequestMessage value)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            return await Task.Run(async delegate
            {
                if (value == null)
                {
                    return "FAILURE: could not read any data uploaded";
                }
                try
                {
                    var data = await value.Content.ReadAsByteArrayAsync();
                    try
                    {
                        var base64 = Encoding.UTF8.GetString(data);
                        try
                        {
                            return FileHelper.UpdateWordDoc(base64);
                        }
                        catch (Exception e)
                        {
                            return "FAILURE: " + e.Message + "   Byte count in string length: " + base64.Length;
                        }
                    }
                    catch (Exception e)
                    {
                        return "FAILURE: data could not be made into byte array from base 64 string";
                    }
                }
                catch (Exception e)
                {
                    return "FAILURE: data could not be read as a string";
                }
            });
        }
        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}