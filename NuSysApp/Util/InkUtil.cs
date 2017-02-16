using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using NusysIntermediate;

namespace NuSysApp
{
    public static class InkUtil
    {

        /// <summary>
        /// static method to turn an inkstroke into an ink model
        /// </summary>
        /// <param name="inkStroke"></param>
        /// <param name="contentDataModelId"></param>
        /// <param name="color"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        public static InkModel ToInkModel(this InkStroke inkStroke, string contentDataModelId, Color color, float thickness)
        {
            color.ToColorModel();
            var model = new InkModel
            {
                Color = color.ToColorModel(),
                Thickness = thickness,
                InkPoints =
                    inkStroke.GetInkPoints()
                        .Select(p => new PointModel(p.Position.X, p.Position.Y, p.Pressure))
                        .ToList(),
                ContentId = contentDataModelId,
                InkStrokeId = SessionController.Instance.GenerateId()
            };
            return model;
        }

        public static bool IsPointCloseToStroke(Vector2 p, InkStroke stroke)
        {
            return IsPointCloseToStroke(new Point(p.X, p.Y), stroke);
        }

        public static bool IsPointCloseToStroke(Point p, InkStroke stroke)
        {
            var minDist = double.PositiveInfinity;
            foreach (var inqPoint in stroke.GetInkPoints())
            {
                var dist = Math.Sqrt((p.X - inqPoint.Position.X) * (p.X - inqPoint.Position.X) + (p.Y - inqPoint.Position.Y) * (p.Y - inqPoint.Position.Y));
                if (dist < minDist)
                    minDist = dist;
            }
            return minDist < 100.0 / SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalToScreenMatrix.M11;
        }
    }
}
