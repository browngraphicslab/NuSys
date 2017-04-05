using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    public class UploadNewDocController : ApiController
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
                var temp = JsonConvert.DeserializeObject<UploadNewDocModel>(s);

                var m = new Message();
                var contentDataId = NusysConstants.GenerateId();
                string title;

                switch (temp.type)
                {
                    case UploadNewDocModel.SelectionType.Pdf: //download the pdf and then create a pdf content data model from it
                        var client = new HttpClient();
                        var result = await client.GetStreamAsync(new Uri(temp.url));
                        MemoryStream ms = new MemoryStream();
                        result.CopyTo(ms);
                        var data = ms.ToArray();

                        try
                        {
                            title = temp.url.Substring(temp.url.LastIndexOf('/'));
                            title = title.Substring(0, title.LastIndexOf('.'));
                        }
                        catch (Exception e)
                        {
                            title = temp.url ?? "New web imported PDF";
                        }

                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY] = contentDataId;
                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES] = Convert.ToBase64String(data);
                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY] = NusysConstants.ContentType.PDF;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY] = contentDataId;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATOR_USER_ID_KEY] = "web_import";
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_ACCESS_KEY] = NusysConstants.AccessType.Public.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATION_TIMESTAMP_KEY] = DateTime.UtcNow.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY] = temp.selectionId;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY] = NusysConstants.ElementType.PDF.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY] = title;

                        m[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_HEIGHT] = 1;
                        m[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_WIDTH] = 1;
                        m[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_TOP_LEFT_X] = 0;
                        m[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_TOP_LEFT_Y] = 0;

                        break;
                    case UploadNewDocModel.SelectionType.Text:

                        try
                        {
                            title = temp.data.Substring(0,temp.data.IndexOfAny(new char[] {'.',';','!','?'}));
                        }
                        catch (Exception e)
                        {
                            title = "New text import";
                        }


                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY] = contentDataId;
                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES] = temp.data;
                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY] = NusysConstants.ContentType.Text;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY] = contentDataId;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATOR_USER_ID_KEY] = "web_import";
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_ACCESS_KEY] = NusysConstants.AccessType.Public.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATION_TIMESTAMP_KEY] = DateTime.UtcNow.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY] = temp.selectionId;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY] = NusysConstants.ElementType.Text.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY] = title;

                        break;
                    case UploadNewDocModel.SelectionType.Website:

                        try
                        {

                            title = new Uri(temp.url).GetLeftPart(UriPartial.Authority).Replace("/www.", "/").Replace("http://", "").Replace("https://", "");
                            if (title.IndexOf('.') > 0)
                            {
                                title = title.Substring(0, title.LastIndexOf("."));
                            }
                            if (title.IndexOf(".com") > 0)
                            {
                                title = title.Substring(0, title.LastIndexOf("."));
                            }
                        }
                        catch (Exception e)
                        {
                            title = temp.url ?? "New website import";
                        }

                        var websiteImageUrl = await ImageUtil.GetImageUrlFromUrl(temp.url, contentDataId);


                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY] = contentDataId;
                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES] = websiteImageUrl;
                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY] = NusysConstants.ContentType.HTML;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY] = contentDataId;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATOR_USER_ID_KEY] = "web_import";
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_ACCESS_KEY] = NusysConstants.AccessType.Public.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATION_TIMESTAMP_KEY] = DateTime.UtcNow.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY] = temp.selectionId;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY] = NusysConstants.ElementType.HTML.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY] = title;

                        m[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_HEIGHT] = 1;
                        m[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_WIDTH] = 1;
                        m[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_TOP_LEFT_X] = 0;
                        m[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_TOP_LEFT_Y] = 0;

                        break;
                    case UploadNewDocModel.SelectionType.Img:
                        try
                        {
                            title = temp.data.Substring(temp.data.LastIndexOf('/'));
                            title = title.Substring(0, title.LastIndexOf('.'));
                        }
                        catch (Exception e)
                        {
                            title = temp.url ?? "New web imported Image";
                        }

                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY] = contentDataId;
                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES] = temp.data;
                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY] = NusysConstants.ContentType.Image;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY] = contentDataId;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATOR_USER_ID_KEY] = "web_import";
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_ACCESS_KEY] = NusysConstants.AccessType.Public.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATION_TIMESTAMP_KEY] = DateTime.UtcNow.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY] = temp.selectionId;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY] = NusysConstants.ElementType.Image.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY] = title;

                        m[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_HEIGHT] = 1;
                        m[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_WIDTH] = 1;
                        m[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_TOP_LEFT_X] = 0;
                        m[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_TOP_LEFT_Y] = 0;

                        break;
                    case UploadNewDocModel.SelectionType.Video:
                        try
                        {

                            title = new Uri(temp.url).GetLeftPart(UriPartial.Authority).Replace("/www.", "/").Replace("http://", "").Replace("https://", "");
                            if (title.IndexOf('.') > 0)
                            {
                                title = title.Substring(0, title.LastIndexOf("."));
                            }
                            if (title.IndexOf(".com") > 0)
                            {
                                title = title.Substring(0, title.LastIndexOf("."));
                            }
                            title += " video";
                        }
                        catch (Exception e)
                        {
                            title = temp.url ?? "New imported video";
                        }

                        string videoString;

                        if (temp.data.Substring(0,25).Contains("youtube"))
                        {
                            videoString = temp.data.Substring(temp.data.LastIndexOf("v=") + 2);
                        }
                        else
                        {
                            videoString = temp.data;
                        }

                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY] = contentDataId;
                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES] = videoString;
                        m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY] = NusysConstants.ContentType.Video;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY] = contentDataId;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATOR_USER_ID_KEY] = "web_import";
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_ACCESS_KEY] = NusysConstants.AccessType.Public.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATION_TIMESTAMP_KEY] = DateTime.UtcNow.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY] = temp.selectionId;
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY] = NusysConstants.ElementType.Video.ToString();
                        m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY] = title;

                        m[NusysConstants.NEW_AUDIO_LIBRARY_ELEMENT_REQUEST_TIME_DURATION] = 1;

                        break;
                }

                if (m.Any())
                {

                    var metadata = new List<MetadataEntry>()
                    {
                        new MetadataEntry("Original_Url", new List<string>() {temp.url}, MetadataMutability.IMMUTABLE)
                    };
                    m[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_METADATA_KEY] = JsonConvert.SerializeObject(metadata);

                    m[NusysConstants.REQUEST_TYPE_STRING_KEY] =
                        NusysConstants.RequestType.CreateNewContentRequest.ToString();

                    var handler = new CreateNewContentRequestHandler();
                    handler.HandleRequest(new Request(m), null);
                    return "success!";
                }

                return "no request made, type wasn't recognized !";
            }
            catch (Exception e)
            {
                return e.Message;
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