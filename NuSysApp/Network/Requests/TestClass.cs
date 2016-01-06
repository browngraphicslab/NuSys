using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class TestClass
    {
        private double _x;
        private double _y;
        private Dictionary<string, string> _dict;
        public TestClass(double x,double y, Dictionary<string,string> dict)
        {
            _x = x;
            _y = y;
            _dict = dict;
        }

        public async Task Test()
        {

        }
    }
}
