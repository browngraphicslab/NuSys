using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{

    public class ListColumn<T>
    {
        /// <summary>
        /// This function takes in a generic item, and returns the rectangle UI element
        /// that will be placed in the list. 
        /// </summary>
        public Func<T, BaseRenderItem, ICanvasResourceCreatorWithDpi, RectangleUIElement> ColumnFunction { get; set; }

        /// <summary>
        /// Title of the column
        /// </summary>
        public string Title { get; set; }

    }
}