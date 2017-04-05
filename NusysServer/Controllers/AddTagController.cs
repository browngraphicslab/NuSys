using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    public class AddTagController : ApiController
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

            try
            {
                var s = await value.Content.ReadAsStringAsync();
                var tagDoc = JsonConvert.DeserializeObject<AddTagModel>(s);

                var sqlRequest = new SQLSelectQuery(new SingleTable(Constants.SQLTableType.LibraryElement, Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY)),
                    new SqlQueryEquals(Constants.SQLTableType.LibraryElement,
                        NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY, tagDoc.selectionId));
                var results = sqlRequest.ExecuteCommand();


                var first = results.First();

                var keywords = first.GetList<Keyword>(first.GetKeys().First()) ?? new List<Keyword>();

                keywords.Add(new Keyword(tagDoc.data));

                var m = new Message();
                m[NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY] = JsonConvert.SerializeObject(keywords);
                m[NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_SAVE_TO_SERVER_BOOLEAN] = true.ToString();
                m[NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID] = tagDoc.selectionId;
                m[NusysConstants.REQUEST_TYPE_STRING_KEY] = NusysConstants.RequestType.UpdateLibraryElementModelRequest.ToString();

                var handler = new UpdateLibraryElementRequestHandler();
                handler.HandleRequest(new Request(m), null);
                return "Success!";

            }
            catch (Exception e)
            {
                return e.Message+"   "+e.StackTrace;
            }
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