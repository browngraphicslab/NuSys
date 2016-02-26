using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class LabelNodeViewModel : ElementCollectionInstanceViewModel
    {
       
        public List<string> TitleSuggestions { get; set; } 

        public LabelNodeViewModel(ElementCollectionInstanceController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));

            var model = (TagNodeModel)controller.Model;
            TitleSuggestions = model.TitleSuggestions.ToList();
        }

        private async Task OnChildAddedToLabel(object source, FrameworkElement node)
        {
            SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Add(node);
            RaisePropertyChanged("NumChildren");
            RaisePropertyChanged("Title");
        }

        public bool IsTemporary
        {
            get { return ((NodeContainerModel) Model).IsTemporary; }
            set { ((NodeContainerModel) Model).IsTemporary = value; }
        }
    }
}