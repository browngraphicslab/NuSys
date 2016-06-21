using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using NuSysApp.Nodes.AudioNode;

namespace NuSysApp
{
    public class VideoNodeViewModel : ElementViewModel
    {
        public VideoNodeViewModel(ElementController controller) : base(controller)
        {
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
        }

        public override void Dispose()
        {
            Controller.LibraryElementController.Loaded -= LibraryElementModelOnOnLoaded;
            base.Dispose();
        }

        public override async Task Init()
        {
            if (Controller.LibraryElementController.IsLoaded)
            {
                Controller.SetSize(Model.Width, Model.Height);
            }
            else
            {
                Controller.LibraryElementController.Loaded += LibraryElementModelOnOnLoaded;
            }
        }

        private void LibraryElementModelOnOnLoaded(object sender)
        {
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

        public ObservableCollection<LinkedTimeBlockModel> LinkedTimeModels
        {
            get { return (Model as VideoNodeModel).LinkedTimeModels; }
        }


        public void AddLinkTimeModel(LinkedTimeBlockModel model)
        {
            (Model as VideoNodeModel).LinkedTimeModels.Add(model);
        }

        protected override void OnSizeChanged(object source, double width, double height)
        {
            SetSize(width, height);
        }
    }
}
