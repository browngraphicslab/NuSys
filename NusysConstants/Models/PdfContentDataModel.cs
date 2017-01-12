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
        public List<string> PageUrls { get; set; }

        /// <summary>
        /// The number of pages in the document.
        /// Returns zero if the page urls are null
        /// </summary>
        public int PageCount => PageUrls?.Count ?? 0;

        /// <summary>
        /// This constructor will call the same-declaration constructor in the base ContentDataModel class and will set the PageUrls for this contentDataModel.
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <param name="contentData"></param>
        public PdfContentDataModel(string contentDataModelId, string contentData) : base(contentDataModelId, contentData)
        {
            SetUrls(contentData);
        }

        /// <summary>
        /// this override updates the page urls for the pdf.
        /// </summary>
        /// <param name="data"></param>
        public override void SetData(string data)
        {
            SetUrls(data);
            base.SetData(data);
        }

        /// <summary>
        /// method used to set the urls for this pdf content data model.
        /// Pass in the data to set the Urls
        /// </summary>
        private void SetUrls(string contentData)
        {
            try
            {
                PageUrls = JsonConvert.DeserializeObject<List<string>>(contentData ?? "");
            }
            catch (Exception e)
            {
                PageUrls = new List<string>() {contentData};
            }
        }

        /// <summary>
        /// this override simply just sets the pageUrls to null;
        /// </summary>
        public override void Dispose()
        {
            PageUrls = null;
            base.Dispose();
        }

    }
}
