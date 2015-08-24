using SQLite.Net.Attributes;

namespace NuSysApp
{
    public class Content
    {
        public Content() { }
        public Content(byte[] data, string id)
        {
            Data = data;
            assocAtomID = id;
        }

        [Column("Data")]
        public byte[] Data { get; set; }

        [Column("assocAtomID")]
        public string assocAtomID { get; set; }
    }
}
