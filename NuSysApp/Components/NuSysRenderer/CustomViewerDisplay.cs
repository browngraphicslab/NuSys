using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;
using WinRTXamlToolkit.Tools;

namespace NuSysApp
{
    public class CustomViewerDisplay : RectangleUIElement
    {
        private static Dictionary<CustomViewerDisplay,List<ElementRenderItem>> _allDisplayRenderItems = new Dictionary<CustomViewerDisplay, List<ElementRenderItem>>();
        private static LibraryElementController _currentElement { get; set; }

        public static List<ElementController> Controllers { get; set; } = new List<ElementController>();

        public static LibraryElementController CurrentElement
        {
            get
            {
                return _currentElement;
            }
            set
            {
                Debug.Assert(_currentElement != null);
                _currentElement = value;
                Reset();
            }
        }

        private static async Task Reset()
        {
            var models = Controllers.Select(c => c.Model);
            var newModels = models.Select(m => GetCleanModel(m, _currentElement));

            foreach (var controller in Controllers)//dispose of old controllers
            {
                ElementController outController;
                SessionController.Instance.ElementModelIdToElementController.TryRemove(controller.Id, out outController);
                controller.Dispose();
            }

            Controllers.Clear();
            _allDisplayRenderItems.SelectMany(r => r.Value).ForEach(i => i.Dispose());
            _allDisplayRenderItems.Values.ForEach(i => i.Clear());

            await UITask.Run(async delegate
            {
                foreach (var newModel in newModels)
                {
                    await AddDisplayModel(newModel);
                }
            });
            
        }

        public static async Task AddDisplayModel(ElementModel model)
        {
            var c = ElementControllerFactory.CreateFromModel(model);
            Controllers.Add(c);
            SessionController.Instance.ElementModelIdToElementController[model.Id] = c;

            var viewModel = await new FreeFormElementViewModelFactory().CreateFromSendable(c);

            foreach (var display in _allDisplayRenderItems)
            {
                var view = await GetView(viewModel,display.Key);
                display.Value.Add(view);
            }
        }

        private static async Task<ElementRenderItem> GetView(ElementViewModel vm, BaseRenderItem parent)
        {
            ElementRenderItem item = null;
            if (vm is VariableNodeViewModel)
            {
                item = new VariableElementRenderItem((VariableNodeViewModel)vm, parent, parent.ResourceCreator);
            }
            else if (vm is TextNodeViewModel)
            {
                item = new TextElementRenderItem((TextNodeViewModel)vm, parent, parent.ResourceCreator);
            }
            else if (vm is UnknownFileViewModel)
            {
                item = new UnknownFileElementRenderItem((UnknownFileViewModel)vm, parent, parent.ResourceCreator);
            }
            else if (vm is HtmlNodeViewModel)
            {
                item = new HtmlElementRenderItem((HtmlNodeViewModel)vm, parent, parent.ResourceCreator);
            }
            else if (vm is ImageElementViewModel)
            {
                item = new ImageElementRenderItem((ImageElementViewModel)vm, parent, parent.ResourceCreator);
            }
            else if (vm is WordNodeViewModel)
            {
                item = new WordElementRenderItem((WordNodeViewModel)vm, parent, parent.ResourceCreator);
            }
            else if (vm is PdfNodeViewModel)
            {
                item = new PdfElementRenderItem((PdfNodeViewModel)vm, parent, parent.ResourceCreator);
            }

            else if (vm is AudioNodeViewModel)
            {
                item = new AudioElementRenderItem((AudioNodeViewModel)vm, parent, parent.ResourceCreator);
            }
            else if (vm is VideoNodeViewModel)
            {
                item = new VideoElementRenderItem((VideoNodeViewModel)vm, parent, parent.ResourceCreator);
            }
            await item.Load();
            item.Transform.SetParent(parent.Transform);
            return item;
        }

        private static ElementModel GetCleanModel(ElementModel model, LibraryElementController newController)
        {
            if (model.ElementType == NusysConstants.ElementType.Variable)
            {
                (model as VariableElementModel).StoredLibraryId = newController.LibraryElementModel.LibraryElementId;
                return model;
            }
            var m = new Message()
                {
                    { NusysConstants.LIBRARY_ELEMENT_TYPE_KEY ,_currentElement.LibraryElementModel.Type},
                    { NusysConstants.ALIAS_ID_KEY, model.Id}
                };
            var newModel = ElementModelFactory.CreateFromMessage(m);

            newModel.X = model.X;
            newModel.Y = model.Y;
            newModel.Width = model.Width;
            newModel.Height = model.Height;
            newModel.LibraryId = model.LibraryId;
            newModel.CreatorId = model.CreatorId;
            return newModel;
        }

        public CustomViewerDisplay(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            BorderWidth = 5;
            BorderColor = Colors.Black;
            _allDisplayRenderItems.Add(this,new List<ElementRenderItem>());
        }

        public override void Dispose()
        {
            if (_allDisplayRenderItems.ContainsKey(this))
            {
                _allDisplayRenderItems[this].ForEach(i => i.Dispose());
                _allDisplayRenderItems.Remove(this);
            }
            base.Dispose();
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orig = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            foreach (var element in _allDisplayRenderItems[this])
            {
                element.Draw(ds);
            }
            base.Draw(ds);
            ds.Transform = orig;
        }

        public static async Task AddElementToDisplay(NusysConstants.ElementType type)
        {
            var args = new NewElementRequestArgs();
            args.Width = 150;
            args.Height = 150;
            args.AccessType = NusysConstants.AccessType.Public;
            args.LibraryElementId = "display";
            args.ParentCollectionId = "Doesn't matter!";
            args.X = 50;
            args.Y = 50;
            var request = new NewElementRequest(args);
            request.SetSystemProperties(new Dictionary<string, object>()
            {
                {NusysConstants.LIBRARY_ELEMENT_TYPE_KEY, type}
            });
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddReturnedElementToSession();
        }
    }
}
