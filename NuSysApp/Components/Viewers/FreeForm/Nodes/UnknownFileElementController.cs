using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class UnknownFileElementController : ElementController
    {
        public UnknownFileElementController(UnknownFileElementModel elementModel) : base(elementModel)
        {
            
        }

        public override void SetSize(double width, double height, bool saveToServer = true)
        {
            width = height;
            base.SetSize(width, height, saveToServer);
        }
    }
}
