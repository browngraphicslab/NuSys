﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp  
{
    public interface ITool
    {
        event EventHandler<Point2d> ToolAnchorChanged;

    }
}