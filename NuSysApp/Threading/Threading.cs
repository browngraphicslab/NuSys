using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;

namespace NuSysApp
{

    public static class WinRTExtensionMethods
    {
        public static TResult Await<TResult>(this IAsyncOperation<TResult> operation)
        {
            return operation.AsTask().Result;
        }

 
                
    }

}
