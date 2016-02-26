
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NuSysApp
{
    public class TextElementModel : ElementModel
    {
        private string _text = string.Empty;
        public delegate void TextChangedEventHandler(object source, string text);
        public event TextChangedEventHandler TextChanged;

        public TextElementModel(string id): base(id)
        {
            ElementType = ElementType.Text;
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
                TextChanged?.Invoke(this, _text);
            } 
        }

        public override async Task UnPack(Message props)
        {
            await base.UnPack(props);

            var controller = SessionController.Instance.ContentController;
            var contentId = props.GetString("contentId");
            if (!string.IsNullOrEmpty(contentId) && controller.Get(contentId) != null)
                Text = SessionController.Instance.ContentController.Get(ContentId).Data;

        }

        public override async Task<Dictionary<string,object>> Pack()
        {
            var dict = await base.Pack();
            return dict;
        }
    }
}
