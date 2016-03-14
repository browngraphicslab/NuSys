using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class TextNodeController : ElementController
    {
        public delegate void TextChangedHandler(object source, string text, object originalSender);
        public event TextChangedHandler TextChanged;

        private object _lastSender;
        public TextNodeController(TextElementModel model) : base(model)
        {
            if (SessionController.Instance.ContentController.Get(Model.ContentId) != null)
            {
                var content = SessionController.Instance.ContentController.Get(Model.ContentId);
                content.OnContentChanged += ContentChanged;
            }
        }

        public override async Task FireContentLoaded(NodeContentModel content)
        {
            TextChanged?.Invoke(this,content.Data, null);
            await base.FireContentLoaded(content);
        }

        private void ContentChanged()
        {
            var content = SessionController.Instance.ContentController.Get(Model.ContentId);
            TextChanged?.Invoke(this, content.Data, _lastSender);
        }

        public async void SetText(object sender, string text)
        {
            if (SessionController.Instance.ContentController.Get(Model.ContentId) != null)
            {
                var content = SessionController.Instance.ContentController.Get(Model.ContentId);

                Task.Run(async delegate
                {
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(content.Id,
                        text));
                });
                _lastSender = sender;
            }
        }
    }
}
