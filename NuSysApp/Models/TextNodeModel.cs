
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NuSysApp
{
    public class TextNodeModel : NodeModel
    {
        private string _text = string.Empty;
        public delegate void TextChangedEventHandler(object source, TextChangedEventArgs e);
        public event TextChangedEventHandler TextChanged;

        public TextNodeModel(string id): base(id)
        {
            NodeType = NodeType.Text;
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                var content = SessionController.Instance.ContentController.Get(ContentId);
                if (content != null)
                    content.Data = _text;
                TextChanged?.Invoke(this, new TextChangedEventArgs(_text));
            } 
        }

        public override async Task UnPack(Message props)
        {
            await base.UnPack(props);
<<<<<<< HEAD
            if (!string.IsNullOrEmpty(props.GetString("contentId")) && SessionController.Instance.ContentController.Get(props.GetString("contentId")) != null)
               Text = SessionController.Instance.ContentController.Get(ContentId).Data;
=======
            var controller = SessionController.Instance.ContentController;
            var contentId = props.GetString("contentId");
            if (!string.IsNullOrEmpty(contentId) && controller.Get(contentId) != null)
                Text = SessionController.Instance.ContentController.Get(ContentId).Data;
>>>>>>> origin/phil_groups_trent
        }

        public override async Task<Dictionary<string,object>> Pack()
        {
            var dict = await base.Pack();
            return dict;
        }
    }
}
