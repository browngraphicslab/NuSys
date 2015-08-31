
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace NuSysApp
{

    public class Link : Atom
    {
        public delegate void DeleteEventHandler(object source, DeleteEventArgs e);
        public event DeleteEventHandler OnDeletion;
        public Link(Atom inAtom, Atom outAtom, string id) : base(id)
        {
            InAtomID = inAtom.ID;
            OutAtomID = outAtom.ID;
            ID = id;
            Atom1 = inAtom;
            Atom2 = outAtom;
        }

        public string InAtomID { get; set; }

        public string OutAtomID { get; set; }

        public override void Delete()
        {
            OnDeletion?.Invoke(this, new DeleteEventArgs("Deleted", this));
        }

        public override async Task UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("id1"))
            {
                this.InAtomID = props["id1"];
            }
            if (props.ContainsKey("id2"))
            {
                this.InAtomID = props["id2"];
            }
            base.UnPack(props);
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> dict = await base.Pack();
            dict.Add("id1",InAtomID);
            dict.Add("id2", OutAtomID);
            dict.Add("type","linq");
            return dict;
        }

        public Node Annotation { get; set; }

        public Atom Atom1 { get; private set; }

        public Atom Atom2 { get; private set; }

        public XmlElement WriteXML(XmlDocument doc)
        {
            //XmlElement 
            XmlElement link = doc.CreateElement(string.Empty, "Link", string.Empty); //TODO: Change how we determine node type for name

            //ID of this link
            XmlAttribute id = doc.CreateAttribute("id");
            id.Value = this.ID.ToString();
            link.SetAttributeNode(id);

            //Atoms that this link is bound to
            XmlAttribute id1 = doc.CreateAttribute("atomID1");
            id1.Value = Atom1.ID;
            link.SetAttributeNode(id1);

            XmlAttribute id2 = doc.CreateAttribute("atomID2");
            id2.Value = Atom2.ID;
            link.SetAttributeNode(id2);

            //Annotation, if any
            if (this.Annotation != null)
            {
                XmlElement linkAnnotation = this.Annotation.WriteXML(doc);
                link.AppendChild(linkAnnotation);
            }

            return link;
        }
    }
}
