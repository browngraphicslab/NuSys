using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// class used to store the current session state when the app goes into an unintenional state (suspension, loss of wifi).
    /// </summary>
    public class CapturedStateModel
    {
        /// <summary>
        /// the library element Id of the collection that we were in (null if not in one) when the state was captured.
        /// </summary>
        public string CollectionLibraryElementId { get; private set; }

        /// <summary>
        /// the X coordinate lcoation of the camera when the state was captured.
        /// Can't be NaN.
        /// </summary>
        public float XLocation { get; private set; }

        /// <summary>
        /// the Y coordinate location of the camera when the state was captured.        
        /// Can't be NaN.
        /// </summary>
        public float YLocation { get; private set; }

        /// <summary>
        /// the double representing the X zoom level of the camera when the state was captured.  
        /// Can't be NaN.
        /// </summary>
        public float XZoomLevel { get; private set; }

        /// <summary>
        /// the double representing the Y zoom level of the camera when the state was captured.  
        /// Can't be NaN.
        /// </summary>
        public float YZoomLevel { get; private set; }

        /// <summary>
        /// constructor takes in the collection id, and coordinates of the camera, and the zoom level of the camera when the state is captured.
        /// Will throw an exception if the x,y, xZoom, or yZoom are NaN.
        /// Will throw an exception if collection id (the library element id of the current collection), is null or empty.
        /// </summary>
        /// <param name="collectionLibraryId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="zoom"></param>
        public CapturedStateModel(string collectionLibraryId, float x, float y, float centerX, float centerY, float xZoom, float yZoom)
        {
            if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(yZoom) || float.IsNaN(xZoom) || string.IsNullOrEmpty(collectionLibraryId))
            {
                throw new Exception("Invalid state was trying to be captured");
            }
            CollectionLibraryElementId = collectionLibraryId;
            XLocation = x;
            YLocation = y;
            XCenter = centerX;
            YCenter = centerY;
            XZoomLevel = xZoom;
            YZoomLevel = yZoom;
        }

        public float YCenter { get; set; }

        public float XCenter { get; set; }
    }
}
