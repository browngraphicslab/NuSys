using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class XMLFile
    {
        public XMLFile()
        { }

        [Column("ID"), AutoIncrement]
        public int ID { get; set; }

        [Column("toXML")]
        public string toXML { get; set; }
    }
}
