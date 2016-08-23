using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using SharpDX.Multimedia;

namespace NuSysApp2
{
    public abstract class Region : LibraryElementModel
    {
        public string ClippingParentLibraryId { get; private set; }
        
        public Region(string id, ElementType elementType) : base(id, elementType)
        {

        }

        public override async Task UnPack(Message message)
        {
            if (message.GetString("clipping_parent_library_id") != null)
            {
                ClippingParentLibraryId = message.GetString("clipping_parent_library_id");
            }
            await base.UnPack(message);
        }
    }
}