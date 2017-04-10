using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class CustomVariableKeyPopup : CenteredPopup
    {
        private ScrollableTextboxUIElement _text;
        private string _elementId;
        public CustomVariableKeyPopup(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, string elementId) : base(parent, resourceCreator, "Enter key: ")
        {
            _text = new ScrollableTextboxUIElement(this,resourceCreator, false, false);
            _text.Height = 100;
            _text.Width = 200;
            _dismissButton.Transform.LocalY += 150;
            Height += 150;
            AddChild(_text);
            _elementId = elementId;
            Dismissed += OnDismissed;
        }

        public override void Dispose()
        {
            Dismissed -= OnDismissed;
            base.Dispose();
        }

        private void OnDismissed(object sender, EventArgs eventArgs)
        {
            Debug.Assert(SessionController.Instance.ElementModelIdToElementController.ContainsKey(_elementId));
            if (SessionController.Instance.ElementModelIdToElementController.ContainsKey(_elementId))
            {
                var controller = SessionController.Instance.ElementModelIdToElementController[_elementId] as VariableElementController;
                controller.SetMetadataKey(_text.Text);
            }
        }

        public override async Task Load()
        {
           await _text.Load();
           base.Load();
        }

    }
}
