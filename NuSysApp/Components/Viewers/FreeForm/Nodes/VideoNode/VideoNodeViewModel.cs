using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using NusysIntermediate;
using NuSysApp.Util;

namespace NuSysApp
{
    public class VideoNodeViewModel : ElementViewModel
    {
        public VideoNodeViewModel(ElementController controller) : base(controller)
        {
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
        }

        public override async Task Init()
        {
            if (!Controller.LibraryElementController.ContentLoaded)
            {
                await Controller.LibraryElementController.LoadContentDataModelAsync();
            }
            Controller.SetSize(Model.Width, Model.Height);
        }

        public override void SetSize(double width, double height)
        {
            var model = (VideoNodeModel)Model;
            if (model.ResolutionX < 1)
            {
                return;
            }
            if (width > height)
            {
                var r = model.ResolutionY / (double)model.ResolutionX;
                base.SetSize(width, width * r + 150);
            }
            else
            {

                var r = model.ResolutionX / (double)model.ResolutionY;
                base.SetSize(height * r, height + 150);
            }
        }

        protected override void OnSizeChanged(object source, double width, double height)
        {
            // don't edit if we are in exploration or presentation mode
            if (SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION ||
                SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.PRESENTATION)
            {
                return;
            }

            SetSize(width, height);
        }
    }
}
