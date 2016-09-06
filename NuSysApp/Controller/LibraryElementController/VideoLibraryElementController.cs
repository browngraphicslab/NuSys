using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using NusysIntermediate;

namespace NuSysApp
{
    public class VideoLibraryElementController : AudioLibraryElementController
    {

        public VideoLibraryElementModel VideoLibraryElementModel
        {
            get
            {
                return base.LibraryElementModel as VideoLibraryElementModel;
            }
        }
        public VideoLibraryElementController(VideoLibraryElementModel model) : base(model)
        {
            
        }

        public override void UnPack(Message message)
        {
            SetBlockServerBoolean(true);
            base.UnPack(message);
            SetBlockServerBoolean(false);
        }
    }
}
