using SQLite.Net.Attributes;

namespace NuSysApp
{
    public class Content
    {
        public Content() { }
        public Content(byte[] data, string id)
        {
            Image = data;
            assocAtomID = id;
        }

        [Column("Image")]
        public byte[] Image { get; set; }

        [Column("assocAtomID")]
        public string assocAtomID { get; set; }
    }
}
