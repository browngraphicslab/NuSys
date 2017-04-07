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
        public event EventHandler<string> MetadataKeyChanged;
        public event EventHandler<double> AspectRatioChanged;

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
        public void SetMetadataKey(string metadataKey)
        {
            metadataKey = metadataKey ?? "";

            /*
            var format = new TextboxUIElement(RenderItemInteractionManager.Root, RenderItemInteractionManager.Root.ResourceCreator).CanvasTextFormat;
            var layout = new CanvasTextLayout(RenderItemInteractionManager.Root.ResourceCreator,metadataKey,format,float.MaxValue, float.MaxValue);

            var ratio = (layout.LayoutBounds.Width * .67)/layout.LayoutBounds.Height;
            SetAspectRatio(ratio);*/

            VariableModel.MetadataKey = metadataKey;

            MetadataKeyChanged?.Invoke(this,metadataKey);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add("metadataKey", metadataKey);
            }
        }
        public override void UnPack(Message message)
        {
            _blockServerInteractionCount++;
            if (message.ContainsKey("metadataKey"))
            {
                SetMetadataKey(message.GetString("metadataKey"));
            }
            if (message.ContainsKey("aspectRatio"))
            {
                //SetAspectRatio(message.GetDouble("aspectRatio"));
            }
            base.UnPack(message);
            _blockServerInteractionCount--;
        }
    }
}
