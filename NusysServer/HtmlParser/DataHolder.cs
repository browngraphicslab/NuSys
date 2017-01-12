using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysServer
{
    public class DataHolder
    {

        public String Title { get; set; }
        public DataHolder(string title)
        {
            Content = new ContentDataModel(NusysConstants.GenerateId(),"");
            LibraryElement = new LibraryElementModel(NusysConstants.GenerateId(),NusysConstants.ElementType.Collection);
            LibraryElement.Title = title;
            LibraryElement.ContentDataModelId = Content.ContentId;
            Title = title;
        }

        public ContentDataModel Content;
        public LibraryElementModel LibraryElement;

    }
}
