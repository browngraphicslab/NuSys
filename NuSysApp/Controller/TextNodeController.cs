using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class TextNodeController : ElementController
    {
        public delegate void TextChangedHandler(object source, string text);
        public event TextChangedHandler TextChanged;

        public TextNodeController(TextElementModel model) : base(model)
        {
            if (SessionController.Instance.ContentController.Get(Model.ContentId) != null)
            {
                var content = SessionController.Instance.ContentController.Get(Model.ContentId);
                content.OnContentChanged += delegate
                {
                    TextChanged?.Invoke(this,content.Data);
                };
            }
        }

        public async void SetText(string text)
        {
            if (SessionController.Instance.ContentController.Get(Model.ContentId) != null)
            {
                var content = SessionController.Instance.ContentController.Get(Model.ContentId);

                Task.Run(async delegate
                {
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(content.Id,
                        text));
                });
            }
        }
    }
}
