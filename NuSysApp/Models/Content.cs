using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public abstract class Content
    {
        public abstract string GetData();
        public abstract void SetData(string data);
    }
}
