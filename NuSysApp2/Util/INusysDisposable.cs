﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp2
{
    public interface INuSysDisposable
    {
        event EventHandler Disposed;
    }
}
