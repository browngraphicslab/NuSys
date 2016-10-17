using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace NuSysApp
{
    /// <summary>
    /// wrapper to contain a listview and its header
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListViewUIElementContainer<T> : RectangleUIElement
    {
        /// <summary>
        /// the listview that will be contained by this container
        /// </summary>
        private ListViewUIElement<T> _listview;

        /// <summary>
        /// header for the listview - should be treated differently from a listview row
        /// </summary>
        private ListViewHeader<T> _header;

        /// <summary>
        /// header size for header
        /// </summary>
        private float _headerHeight;

        /// <summary>
        /// setter for header size
        /// </summary>
        public float HeaderHeight { set { _headerHeight = value; } }

        /// <summary>
        /// setter and getter for listview
        /// adds the listview to the container's children so it can draw it relative to the container
        /// </summary>
        public ListViewUIElement<T> ListView
        {
            get { return _listview; }
            set
            {
                if (value != null)
                {
                    _listview = value;
                    AddChild(_listview);
                }
            }
        }

        public ListViewUIElementContainer(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _header = new ListViewHeader<T>(parent, resourceCreator);
            _headerHeight = 0;
        }

        /// <summary>
        /// generate the titles/columns for the header
        /// </summary>
        public void GenerateHeader()
        {
            if (_listview != null)
            {
                
            }
        }

        /// <summary>
        /// draw the list container and its inner children (the listview and the header)
        /// those in turn draw their children
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            //draw the listview below the header
            _listview.Transform.LocalPosition = new Vector2(0, _headerHeight);
            base.Draw(ds);
        }
    }
}
