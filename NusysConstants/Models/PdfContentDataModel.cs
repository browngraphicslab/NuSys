using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NusysIntermediate
{
    /// <summary>
    /// the subclass of ContentDataModel that will be used for Pdfs.
    /// Simply improves by storing a list of all the PagUrls as strings.  
    /// </summary>
    public class PdfContentDataModel : ContentDataModel
    {
        /// <summary>
        /// this list of Urls, where each one corresponds to an image of the page at the index in the list. 
        /// for example, Page 0 of the pdf can be viewed by displaying the url of index 0 in this list.  
        /// </summary>
        public List<string> PageUrls { get; private set; }

        /// <summary>
        /// This constructor will call the same-declaration constructor in the base ContentDataModel class and will set the PageUrls for this contentDataModel.
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <param name="contentData"></param>
        public PdfContentDataModel(string contentDataModelId, string contentData) : base(contentDataModelId, contentData)
        {
            //set the page Urls
            PageUrls = JsonConvert.DeserializeObject<List<string>>(contentData);
        }
    }
}
