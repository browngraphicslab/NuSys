
using System.Xml;

namespace NuSysApp
{
    public class Link : Atom
    {
        public Link(Atom inAtom, Atom outAtom, int id) : base(id)
        {
            InAtomID = inAtom.ID;
            OutAtomID = outAtom.ID;
            ID = id;
        }

        public int InAtomID { get; set; }
        public int OutAtomID { get; set; }

        public Atom atom1 { get; set; }
        public Atom atom2 { get; set; }

        public XmlElement WriteXML(XmlDocument doc)
        {

            //XmlElement 
            XmlElement link = doc.CreateElement(string.Empty, "Link", string.Empty); //TODO: Change how we determine node type for name

            //Atoms that this link is bound to
            XmlAttribute id1 = doc.CreateAttribute("atomID1");
            id1.Value = InAtomID.ToString();
            link.SetAttributeNode(id1);

            XmlAttribute id2 = doc.CreateAttribute("atomID2");
            id2.Value = OutAtomID.ToString();
            link.SetAttributeNode(id2);

            return link;
        }

    }
}
