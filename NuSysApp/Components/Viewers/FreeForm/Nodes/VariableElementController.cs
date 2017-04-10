using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static int MaxChars = 0;
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

        public string MetadataKey
        {
            get { return VariableModel.MetadataKey; }
            set
            {
                SetMetadataKey(value);
            }
        }
        public VariableElementController(VariableElementModel model) : base(model)
        {
            if (LibraryElementController?.ContentDataController != null)
            {
                LibraryElementController.ContentDataController.ContentDataUpdated -= ContentChanged;
            }
        }

        private void VariableControllerOnMetadataKeyChanged(object sender, string s)
        {
            SetMetadataKey(s);
        }
        public void SetMetadataKey(string metadataKey)
        {
            metadataKey = metadataKey ?? "";

            VariableModel.MetadataKey = metadataKey;
            UpdateText();

            if (_blockServerInteractionCount==0)
            {
                _debouncingDictionary.Add("metadataKey", metadataKey);
            }
        }

        public void IncrementBlockServerInteraction()
        {
            _blockServerInteractionCount++;
        }

        public void DecrementBlockServerInteraction()
        {
            _blockServerInteractionCount--;
        }


        public override void SetSize(double width, double height, bool saveToServer = true)
        {
            if (ValueString?.Length > MaxChars)
            {
                base.SetSize(width, height, saveToServer);
                return;
            }
            var ratio =  ValueAspectRatio;
            ratio = width/height;
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
            base.Dispose();
        }


        public void UpdateText()
        {
            var text = GetText();
            RecalcAspectRatio(text);
            FireTextChanged(text);
            SetSize(Model.Width,Model.Height);
        }

        private string GetText()
        {
            if (VariableModel.StoredLibraryId != null && !string.IsNullOrEmpty(MetadataKey))
            {
                var toShow = SessionController.Instance.ContentController.GetLibraryElementController(VariableModel.StoredLibraryId);
                if (toShow != null)
                {
                    ValueString = InterpretKey(toShow, MetadataKey);
                    return ValueString;
                }
            }
            if (!string.IsNullOrEmpty(MetadataKey))
            {
                ValueString = MetadataKey;
                return ValueString;
            }
            ValueString = " ";
            return ValueString;
        }


        private void RecalcAspectRatio(string text)
        {
            if (text.Length <= 1)
            {
                ValueAspectRatio = 0;
                return;
            }

            text = string.IsNullOrEmpty(text) ? " " :text;
            var format = new TextboxUIElement(RenderItemInteractionManager.Root, RenderItemInteractionManager.Root.ResourceCreator).CanvasTextFormat;
            format.VerticalAlignment = CanvasVerticalAlignment.Top;
            var layout = new CanvasTextLayout(RenderItemInteractionManager.Root.ResourceCreator, text, format, float.MaxValue, float.MaxValue);

            var b = layout.LayoutBoundsIncludingTrailingWhitespace;
            ValueAspectRatio = (b.Width)/(layout.LineMetrics.First().Height + 1);
        }


        private string InterpretKey(LibraryElementController controller, string key)
        {
            var metadata = controller.FullMetadata;
            if (key == "TextContent")
            {
                if (controller?.ContentDataController == null)
                {
                    Task.Run(async delegate
                    {
                        await SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(
                            controller.LibraryElementModel.ContentDataModelId);
                        Debug.Assert(controller?.ContentDataController != null);
                        this.UpdateText();
                    });
                }
                return controller?.ContentDataController?.ContentDataModel?.Data ?? "";
            }
            return metadata.ContainsKey(key) ? string.Join(",",metadata[key].Values) : " ";
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
            if (props.ContainsKey("metadataKey"))
            {
                SetMetadataKey(props.GetString("metadataKey"));
            }
            return base.UnPack(props);
            _blockServerInteractionCount--;
        }
    }
}
