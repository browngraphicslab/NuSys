using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class VariableNodeViewModel : TextNodeViewModel
    {
        public VariableElementController VariableElementController
        {
            get
            {
                return Controller as VariableElementController;
            }
        }

        public VariableNodeViewModel(ElementController controller) : base(controller)
        {
            Text = VariableElementController.ValueString;
        }

        public override void SetSize(double width, double height)
        {
            return;
            if (width < Constants.MinNodeSize || height < Constants.MinNodeSize)
            {
                var ratio = width / height;
                if (width < Constants.MinNodeSize)
                {
                    width = Constants.MinNodeSize;
                    height = (1 / ratio) * width;
                }
                if (height < Constants.MinNodeSize)
                {
                    height = Constants.MinNodeSize;
                    width = ratio * height;
                }
            }
            base.SetSize(width, height);
        }

    }
}
