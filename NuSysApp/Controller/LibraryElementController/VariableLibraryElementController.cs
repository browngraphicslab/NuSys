using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;

namespace NuSysApp
{
    public class VariableLibraryElementController : LibraryElementController
    {

        public VariableLibraryElementModel VariableModel
        {
            get
            {
                Debug.Assert(LibraryElementModel is VariableLibraryElementModel);
                return LibraryElementModel as VariableLibraryElementModel;
            }
        }

        public VariableLibraryElementController(VariableLibraryElementModel libraryElementModel)
            : base(libraryElementModel)
        {

        }
        /*
        public void SetAspectRatio(double ratio)
        {
            VariableModel.AspectRatio = ratio;
            AspectRatioChanged?.Invoke(this, ratio);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add("aspectRatio", ratio);
            }
        }
        */
        
        public override void UnPack(Message message)
        {
            _blockServerInteractionCount++;

            base.UnPack(message);
            _blockServerInteractionCount--;
        }
    }
}
