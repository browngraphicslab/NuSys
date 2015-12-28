using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class NodeContainerModel : NodeModel
    {
        private bool _isTemporary;

        public delegate void NodeChangeHandler(object source, Sendable node);
        public delegate Task NodeChangeHandler2(object source, Sendable node);

        //public ObservableDictionary<string, Sendable> Children { set; get; }

        public NodeContainerModel(string id) : base(id)
        {
         //   Children = new ObservableDictionary<string, Sendable>();
        }

        public bool IsTemporary
        {
            get { return _isTemporary; }
            set
            {
                _isTemporary = value;
                ModeChanged?.Invoke(this, this);
            }
        }

        //public InqCanvasModel InqModel { get; set; }

        public event NodeChangeHandler linkAdded;
        public event NodeChangeHandler2 ChildAdded;
        public event NodeChangeHandler2 ChildRemoved;
        public event NodeChangeHandler ModeChanged;


        public async Task AddChild(Sendable nodeModel)
        {
            // TODO: wait for all
            //await ChildAdded?.Invoke(this, nodeModel);

            var handler = ChildAdded;
            if (handler != null)
            {
                var tasks = handler.GetInvocationList().Cast<NodeChangeHandler2>().Select(s => s(this, nodeModel));
                await Task.WhenAll(tasks);
            }
        }

        public async Task RemoveChild(Sendable nodeModel)
        {
            // TODO: wait for all
            await ChildRemoved?.Invoke(this, nodeModel);
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            var dict = await base.Pack();
            dict["isTemporary"] = IsTemporary.ToString();
            return dict;
        }

        public override async Task UnPack(Message props)
        {
            base.UnPack(props);

            if (props.ContainsKey("isTemporary"))
            {
                IsTemporary = bool.Parse(props["isTemporary"]);
            }

            if (props.ContainsKey("idList"))
            {
                var ids = props["idList"];
                var idList = ids.Split(',');
                var idDict = new Dictionary<string, Sendable>();
                foreach (var id in idList)
                {
                    var tempNode = (NodeModel) SessionController.Instance.IdToSendables[id];
                    idDict.Add(id, tempNode);
                }
            }
        } //TODO add in pack functions
    }
}