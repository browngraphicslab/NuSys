using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace NuSysApp
{
    public abstract class Sendable : BaseINPC
    {
        public enum EditStatus
        {
            Yes,
            No,
            Maybe
        }
        private EditStatus _editStatus;
        public delegate void CanEditChangedEventHandler(object source, CanEditChangedEventArg e);
        public event CanEditChangedEventHandler OnCanEditChanged;
        public Sendable(string id) : base()
        {
            ID = id;
            _editStatus = EditStatus.Maybe;
        }
        public async virtual Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("id", ID);
            return dict;
        }
        public async virtual Task UnPack(Dictionary<string, string> props) { }
        public string ID { get;}
        public EditStatus CanEdit
        {
            get
            {
                return _editStatus;
            }
            set
            {
                if (_editStatus == value)
                {
                    return;
                }
                _editStatus = value;
                OnCanEditChanged?.Invoke(this, new CanEditChangedEventArg("Can edit changed", CanEdit));
            }
        } //Network locks

        public abstract void Delete();
        public List<Sendable> Children { set; get; }
        public virtual async Task<string> Stringify()
        {
            Dictionary<string, string> props = await Pack();
            Dictionary<string, string> childs = new Dictionary<string, string>();//YES, i know childs is bad grammars but I already used Children
            foreach(Sendable child in Children)
            {
                childs.Add(child.ID, await child.Stringify());
            }
            props["children"] = Newtonsoft.Json.JsonConvert.SerializeObject(childs);
            return Newtonsoft.Json.JsonConvert.SerializeObject(props);
        }
    }
}
