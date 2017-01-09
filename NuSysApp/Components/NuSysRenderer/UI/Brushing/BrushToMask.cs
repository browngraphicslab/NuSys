using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace NuSysApp
{
    class BrushToMask
    {
        public static CanvasGeometry FilterMask(HashSet<ElementController> controllers, ICanvasResourceCreator resourceCreator, CanvasGeometry collectionMask)
        {
            // get the current collection from the freeformviewer
            var collection = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection;

            // create a rectangle which exists as if at the origin, but has the same width and height as the CurrentCollection viewpoirt
            var collectionRectOrg = new Rect(collection.ViewModel.X,
                                                collection.ViewModel.Y,
                                                collection.ViewModel.Width,
                                                collection.ViewModel.Height);

            // not sure what this is doing, its always the same values as the collectionRectOrg as far as i can tell //todo ask phil
            var collectionRectScreen = Win2dUtil.TransformRect(collectionRectOrg, SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetTransformUntil(collection));

            // this gives the actual coordinates of the view rectangle, with the upper left corner and width and height equivalent to what the camera
            // can currently see on the collection
            var collectionRect = Win2dUtil.TransformRect(collectionRectScreen, Win2dUtil.Invert(SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetCollectionTransform(collection)));

            // the ratio of how large the collectionRect coordinates are compared to the screen coordinates
            var heightRatio = collectionRect.Height/ collectionRectScreen.Height;
            var widthRatio = collectionRect.Width/collectionRectScreen.Width;

            // create a list of rects which will be used to store geometry of each node 
            List<Rect> rects = new List<Rect>();

            // get a list of all the ids of the passed in element controllers
            var ids = new HashSet<string>(controllers.Select(ec => ec.Id));

            // iterate through all the viewmodels in the current collection
            foreach (var vm in collection.ViewModel.Elements.ToArray())
            {
                // if the viewmodel is not in the set of elements passed in it is not part of the mask just continue
                if (!ids.Contains(vm.Id))
                {
                    continue;
                }
                
                // otherwise add the viewmodel to the list of rects we are going to apply a mask too
                try
                {
                    rects.Add(new Rect((Math.Max(0, vm.X) - collectionRect.X)/widthRatio, (Math.Max(vm.Y, 0) - collectionRect.Y)/heightRatio, Math.Max(0, vm.Width) / widthRatio,
                        Math.Max(vm.Height, 0) / heightRatio));
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Couldn't get element bounds for brush to mask.");
                }
            }

            var combinedRectMask = CanvasGeometry.CreateRectangle(resourceCreator, new Rect(0,0,0,0));

            // combine the mask with all the passed in rectangles
            foreach (var rect in rects)
            {
                combinedRectMask = combinedRectMask.CombineWith(CanvasGeometry.CreateRectangle(resourceCreator, rect),Matrix3x2.Identity, CanvasGeometryCombine.Union);
            }

            return collectionMask.CombineWith(combinedRectMask, Matrix3x2.Identity, CanvasGeometryCombine.Intersect);

        }
    }
}
