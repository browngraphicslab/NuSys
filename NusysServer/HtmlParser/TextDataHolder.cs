using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysServer
{
    public class TextDataHolder : DataHolder
    {
        private string _text;

        public String Text
        {
            get { return _text; }
            set
            {
                _text = value;
                Content.Data = value;
            }
        }

        public List<LinkLibraryElementModel> LinkLibraryElementModels;
        public List<PdfDataHolder> ReferenceLibraryElementModels;

        private List<string> _links;
        public List<string> links
        {
            get { return _links; }
            set
            {
                _links = value;
                LinkLibraryElementModels = new List<LinkLibraryElementModel>();
                ReferenceLibraryElementModels = new List<PdfDataHolder>();
                foreach(var link in _links)
                {
                    var pdfDataHolder = new PdfDataHolder(new Uri(link),"Reference: " + _links.IndexOf(link));
                    ReferenceLibraryElementModels.Add(pdfDataHolder);
                    var LLEM = new LinkLibraryElementModel(NusysConstants.GenerateId());
                    LLEM.InAtomId = LibraryElement.LibraryElementId;
                    LLEM.OutAtomId = pdfDataHolder.LibraryElement.LibraryElementId;
                    LLEM.Title = Title + " " + pdfDataHolder.Title;
                }
            } 
        }

        public TextDataHolder(string text,string title="") : base(title)
        {
            LibraryElement.Type=NusysConstants.ElementType.Text;
            Content.ContentType=NusysConstants.ContentType.Text;
            Content.Data = text;
            this.Text = text;
        }
    }
}
