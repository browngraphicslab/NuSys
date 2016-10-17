using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// the header of a list view
    /// </summary>
    public class ListViewHeader<T> : RectangleUIElement
    {
        /// <summary>
        /// list view from which the header will base its titles
        /// </summary>
        private ListViewUIElement<T> _listview;

        public ListViewHeader(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

        }

        /// <summary>
        /// make the titles for all the columns of the header based off of a listview parameter
        /// </summary>
        /// <param name="listview"></param>
        public void MakeTitles(ListViewUIElement<T> listview)
        {
            _listview = listview;
            foreach (ListColumn<T> c in _listview.ListColumns)
            {
                
            }
        }
    }
}
