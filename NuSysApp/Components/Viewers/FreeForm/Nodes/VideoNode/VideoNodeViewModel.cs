using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class VideoNodeViewModel : ElementViewModel
    {
        public VideoNodeViewModel(ElementController controller) : base(controller)
        {
            //   this.X = 400;
            //   this.Y = 150;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            SetSize(400,400);
        }
        public override void SetSize(double width, double height)
        {

            var model = Model as VideoNodeModel;
            if (model.ResolutionX > model.ResolutionY)
            {
                var r = model.ResolutionY / (double)model.ResolutionX;
                base.SetSize(width, width * r);
            }
            else
            {
                var r = model.ResolutionX / (double)model.ResolutionY;
                base.SetSize(height * r, height);
            }
        }
    }
}
