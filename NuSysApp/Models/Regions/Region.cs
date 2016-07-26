using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using NusysIntermediate;

namespace NuSysApp
{
    public abstract class Region : LibraryElementModel
    {

        public string ClippingParentId { get; set; }

        public Region(string libraryElementId, NusysConstants.ElementType type) : base(libraryElementId, type)
        {

        }
        

    }
}