using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NuSysApp
{
    // TODO: remove BASEINPC
    public abstract class Sendable : BaseINPC
    {
        public delegate void CanEditChangedEventHandler(object source, CanEditChangedEventArg e);

        public delegate void UnPackedEventHandler(object source);
        
        public event CanEditChangedEventHandler CanEditChange;
        public event UnPackedEventHandler UnPacked;

        public enum EditStatus { Yes, No, Maybe }

        private EditStatus _editStatus;

        protected Sendable(string id)
        {
            Id = id;
            _editStatus = EditStatus.Maybe;
            //Children = new ObservableDictionary<string, Sendable>();
        }

        public bool IsUnpacked { get; private set; }

        public string Id { get; set; }

        public EditStatus CanEdit
        {
            get { return _editStatus; }
            set
            {
                if (_editStatus == value)
                {
                    return;
                }
                _editStatus = value;
                CanEditChange?.Invoke(this, new CanEditChangedEventArg(CanEdit));
            }
        }

        public virtual async Task<Dictionary<string, string>> Pack()
        {
            var dict = new Dictionary<string, string>();
            dict.Add("id", Id);
            return dict;
        }

        public virtual async Task UnPack(Message props)
        {
            IsUnpacked = true;
            
            UnPacked?.Invoke(this);
        }

        public virtual async Task<string> Stringify()
        {
            var props = await Pack();

            // TODO: Remove
            /*
            var childs = new Dictionary<string, string>();

            if (Children.Count == 0)
                return JsonConvert.SerializeObject(props);
            
            foreach (var child in Children)
            {
                childs.Add(child.Value.ID, await child.Value.Stringify());
            }
            props["children"] = JsonConvert.SerializeObject(childs);
            */
            
            return JsonConvert.SerializeObject(props);
        }
    }
}