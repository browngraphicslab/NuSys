using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    public class VariableElementController : TextNodeController
    {
        public event EventHandler<string> StoredLibraryIdChanged; 
        public VariableLibraryElementController VariableController
        {
            get
            {
                return LibraryElementController as VariableLibraryElementController;
            }
        }

        public VariableElementModel VariableModel
        {
            get { return Model as VariableElementModel; }
        }

        public VariableElementController(TextElementModel model) : base(model)
        {
            VariableController.AspectRatioChanged += VariableControllerOnAspectRatioChanged;
            VariableController.ContentDataController.ContentDataUpdated -= ContentChanged;
            VariableController.MetadataKeyChanged += VariableControllerOnMetadataKeyChanged;
            SessionController.Instance.EnterNewCollectionCompleted += InstanceOnEnterNewCollectionCompleted;
        }

        private void InstanceOnEnterNewCollectionCompleted(object sender, string s)
        {
            UpdateText();
        }

        private void VariableControllerOnMetadataKeyChanged(object sender, string s)
        {
            UpdateText();
        }

        private void VariableControllerOnAspectRatioChanged(object sender, double d)
        {
            SetSize(Model.Width, Model.Height);
        }

        public override void SetSize(double width, double height, bool saveToServer = true)
        {
            var ratio =  ValueAspectRatio;
            height = (1 / ratio) * width;

            if (width < Constants.MinNodeSize || height < Constants.MinNodeSize)
            {
                
                if (width < Constants.MinNodeSize)
                {
                    width = Constants.MinNodeSize;
                    height = (1/ratio)*width;
                }
                else if (height < Constants.MinNodeSize)
                {
                    height = Constants.MinNodeSize;
                    width = ratio*height;
                }
            }
            
            base.SetSize(width, height, saveToServer);
        }

        public override void Dispose()
        {
            VariableController.AspectRatioChanged -= VariableControllerOnAspectRatioChanged;
            VariableController.MetadataKeyChanged -= VariableControllerOnMetadataKeyChanged;
            SessionController.Instance.EnterNewCollectionCompleted -= InstanceOnEnterNewCollectionCompleted;
            base.Dispose();
        }


        public void UpdateText()
        {
            var text = GetText();
            RecalcAspectRatio(text);
            FireTextChanged(text);
        }

        private string GetText()
        {
            if (VariableModel.StoredLibraryId != null && !string.IsNullOrEmpty(VariableController.VariableModel.MetadataKey))
            {
                var toShow = SessionController.Instance.ContentController.GetLibraryElementController(VariableModel.StoredLibraryId);
                if (toShow != null)
                {
                    ValueString = InterpretKey(toShow, VariableController.VariableModel.MetadataKey);
                    return ValueString;
                }
            }
            if (!string.IsNullOrEmpty(VariableController.VariableModel.MetadataKey))
            {
                ValueString = "__" + VariableController.VariableModel.MetadataKey + "__";
                return ValueString;
            }
            ValueString = " ";
            return ValueString;
        }


        private void RecalcAspectRatio(string text)
        {
            text = text ?? " ";
            var format = new TextboxUIElement(RenderItemInteractionManager.Root, RenderItemInteractionManager.Root.ResourceCreator).CanvasTextFormat;
            format.VerticalAlignment = CanvasVerticalAlignment.Top;
            var layout = new CanvasTextLayout(RenderItemInteractionManager.Root.ResourceCreator, text, format, float.MaxValue, float.MaxValue);

            var b = layout.LayoutBoundsIncludingTrailingWhitespace;
            ValueAspectRatio = (layout.GetLeadingCharacterSpacing(0)*text.Length +
                                layout.ClusterMetrics.Sum(i => i.Width) + 1)/(layout.LineMetrics.First().Height + 1);
        }


        private string InterpretKey(LibraryElementController controller, string key)
        {
            return controller.FullMetadata.ContainsKey(key) ? string.Join(",",controller.FullMetadata[key].Values) : "";
        }


        public string ValueString = null;
        public double ValueAspectRatio = 0;

        public void SetStoredLibraryId(string libraryId)
        {
            VariableModel.StoredLibraryId = libraryId;
            StoredLibraryIdChanged?.Invoke(this,libraryId);
            UpdateText();
            SetSize(Model.Width,Model.Height);
            if (_blockServerInteractionCount == 0)
            {
                _debouncingDictionary.Add("StoredLibraryId",libraryId);
            }
        }

        public override Task UnPack(Message props)
        {
            _blockServerInteractionCount++;
            if (props.ContainsKey("StoredLibraryId"))
            {
                SetStoredLibraryId(props.GetString("StoredLibraryId"));
            }
            return base.UnPack(props);
            _blockServerInteractionCount--;
        }
    }
}
