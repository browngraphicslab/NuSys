using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class UnknownFileViewModel : ElementViewModel
    {
        public UnknownFileViewModel(ElementController controller) : base(controller)
        {
        }

        public override void SetSize(double width, double height)
        {
            height = width;
            base.SetSize(width, height);
        }
    }
}
