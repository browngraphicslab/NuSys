using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class BaseRenderItem
    {
        public bool IsDirty { get; set; }

        public virtual void Update(){
            
        }

        public virtual void Draw(CanvasDrawingSession ds) 
        {
         
        }
    }
}
