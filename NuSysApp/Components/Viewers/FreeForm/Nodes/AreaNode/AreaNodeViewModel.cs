﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class AreaNodeViewModel : ElementCollectionInstanceViewModel
    {

        public PointCollection Points { get; set; }

        public AreaNodeViewModel(ElementCollectionInstanceController controller):base(controller)
        {
            Points = new PointCollection();
            var model = (AreaModel)controller.Model;
            model.Points.ForEach((p => Points.Add(p)));
        }

    }
}
