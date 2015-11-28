﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GroupModel : NodeModel
    {
        private bool _isTemporary;

        public delegate void NodeChangeHandler(object source, Sendable node);

        public GroupModel(string id) : base(id)
        {
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

        public InqCanvasModel InqModel { get; set; }

        public event NodeChangeHandler linkAdded;
        public event NodeChangeHandler ChildAdded;
        public event NodeChangeHandler ChildRemoved;
        public event NodeChangeHandler ModeChanged;


        public void AddChild(Sendable nodeModel)
        {
            Children.Add(nodeModel.ID, nodeModel);
            ChildAdded?.Invoke(this, nodeModel);
        }

        public void RemoveChild(Sendable nodeModel)
        {
            Children.Remove(nodeModel.ID);
            ChildRemoved?.Invoke(this, nodeModel);
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            var dict = await base.Pack();
            dict["nodeType"] = NodeType.Group.ToString();
            dict["isTemporary"] = IsTemporary.ToString();
            var idList = "";
            foreach (var s in Children.Keys)
            {
                idList += s + ",";
            }
            if (idList.Length > 0)
            {
                idList.Substring(0, idList.Length - 1);
            }
            dict.Add("idList", idList);

            return dict;
        } //TODO add in pack functions

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
                Children = new ObservableDictionary<string, Sendable>(idDict);
            }
        } //TODO add in pack functions
    }
}