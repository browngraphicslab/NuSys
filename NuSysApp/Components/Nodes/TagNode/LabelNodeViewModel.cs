﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class LabelNodeViewModel : NodeContainerViewModel
    {
       
        public List<string> TitleSuggestions { get; set; } 

        public LabelNodeViewModel(ElementInstanceController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
            EnableChildMove = true;
            ChildAdded += OnChildAddedToLabel;
            var model = (TagNodeModel)controller.Model;
            TitleSuggestions = model.TitleSuggestions.ToList();
        }

        private async Task OnChildAddedToLabel(object source, FrameworkElement node)
        {
            SessionController.Instance.ActiveWorkspace.AtomViewList.Add(node);
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