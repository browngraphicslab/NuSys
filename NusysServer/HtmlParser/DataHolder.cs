using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysServer
{
    public enum DataType
    {
        Text,Image,Pdf,Video,Audio
    };

    public class DataHolder
    {

        public DataType DataType { get; set; }
        public String Title { get; set; }
        public DataHolder(DataType dt,string title)
        {
            DataType = dt;
            Title = title;
        }

        public ContentDataModel Content;
        public LibraryElementModel LibraryElement;

    }
}
