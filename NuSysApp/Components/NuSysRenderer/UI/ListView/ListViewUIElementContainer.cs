using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

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
        /// instance variable for resourcecreator so it can make UI elements
        /// </summary>
        private ICanvasResourceCreatorWithDpi _resourceCreator;

        /// <summary>
        /// where listview will draw itself
        /// </summary>
        private float _listYPos;

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
            _resourceCreator = resourceCreator;
            _listYPos = 0;
        }

        /// <summary>
        /// makes a header if you want a header
        /// </summary>
        public void GenerateHeader(ICanvasResourceCreatorWithDpi resourceCreator)
        {
            if (_listview != null)
            {
                ListViewHeader<T> header = new ListViewHeader<T>(this, resourceCreator);
                header.Transform.LocalPosition = new Vector2(0,0);
                header.BorderWidth = 0;
                header.Background = Colors.DarkSlateGray;
                header.Width = this.Width;
                header.Height = _listview.RowHeight + 10;
                _listYPos = header.Height;
                header.MakeTitles(_listview, resourceCreator);
                this.AddChild(header);
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
            _listview.Transform.LocalPosition = new Vector2(0, _listYPos);
            base.Draw(ds);
        }
    }
}
