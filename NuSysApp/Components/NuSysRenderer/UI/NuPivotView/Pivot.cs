using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class Pivot
    {
        public Pivot(string title, RectangleUIElement displayObject)
        {
            Id = SessionController.Instance.GenerateId();
            Title = title;
            DisplayObject = displayObject;
        }

        /// <summary>
        /// The title of the pivot
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// the display object associated with the pivot, this should support being dynamically resized
        /// unless you are sure that the pivot container it will be placed in does not change size
        /// </summary>
        public RectangleUIElement DisplayObject { get; set; }

        /// <summary>
        /// A randomly generated id for the pivot, unused so far
        /// </summary>
        public string Id { get; private set; }

    }
}
