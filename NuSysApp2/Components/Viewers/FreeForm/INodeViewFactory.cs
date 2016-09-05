﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public interface INodeViewFactory
    {
        Task<FrameworkElement> CreateFromSendable(ElementController model);
    }
}