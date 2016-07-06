using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media;
using NuSysApp;
using LineSegment = NuSysApp.LineSegment;

namespace NuSysApp
{
    public class ScribbleGesture : IGesture
    {
        private IList<InqLineModel> _hitStroqs;
        private InqCanvasModel _inqCanvas;

        public IList<InqLineModel> HitStroqs
        {
            get { return _hitStroqs; }
        }

        public ScribbleGesture(InqCanvasModel inqCanvas)
        {
            _inqCanvas = inqCanvas;
        }

        public bool Recognize(InqLineModel stroq)
        {
            _hitStroqs = new List<InqLineModel>();

            bool isScribble = false;

            foreach (InqLineModel existingStroq in _inqCanvas.Lines.Where(s => s != stroq))
            {
                var intersections = new List<Vector2d>();
                var existingStroqSegs = existingStroq.ToLineSegments();
                var scribbleSegs = stroq.ToLineSegments();

                foreach (LineSegment stroqResampledSeg in existingStroqSegs)
                {
                    foreach (LineSegment stroqSeg in scribbleSegs)
                    {
                        var intersection = new Vector2d(0,0);
                        if (stroqSeg.Intersects(stroqResampledSeg, out intersection))
                        {
                            intersections.Add(intersection);
                        }
                    }
                }

     
                if (intersections.Count > 2 )
                {
                    isScribble = true;
                    _hitStroqs.Add(existingStroq);
                }

                // Check for small exisiting strokes that are completely covered by the scribble.

                // TODO: add back in
                /*
                if (stroq.BoundingRect.Contains(existingStroq.BoundingRect))
                {
                    isScribble = true;
                    _hitStroqs.Add(existingStroq);
                }
                */
            }

            return isScribble;
        }
    }
}