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
        public double XLocation { get; private set; }

        /// <summary>
        /// the Y coordinate location of the camera when the state was captured.        
        /// Can't be NaN.
        /// </summary>
        public double YLocation { get; private set; }

        /// <summary>
        /// the double representing the X zoom level of the camera when the state was captured.  
        /// Can't be NaN.
        /// </summary>
        public double XZoomLevel { get; private set; }

        /// <summary>
        /// the double representing the Y zoom level of the camera when the state was captured.  
        /// Can't be NaN.
        /// </summary>
        public double YZoomLevel { get; private set; }

        /// <summary>
        /// constructor takes in the collection id, and coordinates of the camera, and the zoom level of the camera when the state is captured.
        /// Will throw an exception if the x,y, xZoom, or yZoom are NaN.
        /// Will throw an exception if collection id (the library element id of the current collection), is null or empty.
        /// </summary>
        /// <param name="collectionLibraryId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="zoom"></param>
        public CapturedStateModel(string collectionLibraryId, double x, double y, double xZoom, double yZoom)
        {
            if (double.IsNaN(x) || double.IsNaN(y) || double.IsNaN(yZoom) || double.IsNaN(xZoom) || string.IsNullOrEmpty(collectionLibraryId))
            {
                throw new Exception("Invalid state was trying to be captured");
            }
            CollectionLibraryElementId = collectionLibraryId;
            XLocation = x;
            YLocation = y;
            XZoomLevel = xZoom;
            YZoomLevel = yZoom;
        }
    }
}
