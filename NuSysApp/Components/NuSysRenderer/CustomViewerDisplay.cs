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
                if (value == null)
                {
                    return;
                }
                if (_currentElement == null)
                {
                    _currentElement = value;
                    foreach (var model in _firstModelList)
                    {
                        AddDisplayModel(model);
                    }
                    _firstModelList.Clear();
                }
                _currentElement = value;
                Reset();
            }
        }

        public static VariableElementController LastVariableNodeAdded {
            get
            {
                return SessionController.Instance.ElementModelIdToElementController.Values.OfType<VariableElementController>().OrderBy(i => i.LocalCreationTimestamp).Last();
            }
        }

        private static async Task Reset()
        {
            var models = Controllers.Select(c => c.Model).ToArray();
            var controllers = Controllers.ToArray();

            var all = _allDisplayRenderItems.ToArray();
            all.SelectMany(r => r.Value).ForEach(i => i.Dispose());

            all.ForEach(k => _allDisplayRenderItems[k.Key].Clear());

            foreach (var controller in controllers)//dispose of old controllers
            {
                ElementController outController;
                SessionController.Instance.ElementModelIdToElementController.TryRemove(controller.Id, out outController);
                controller.Dispose();
                Controllers.Remove(controller);
            }


            await UITask.Run(async delegate
            {
                foreach (var newModel in models)
                {
                    await AddDisplayModel(newModel);
                }
            });
            
        }

        private static List<ElementModel> _firstModelList = new List<ElementModel>();

        public static async Task AddDisplayModel(ElementModel model)
        {
            if (_currentElement == null)
            {
                _firstModelList.Add(model);
                return;
            }
            model = GetCleanModel(model, _currentElement);
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
            parent.AddChild(item);
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
                    { NusysConstants.LIBRARY_ELEMENT_TYPE_KEY ,_currentElement?.LibraryElementModel.Type ?? NusysConstants.ElementType.Variable},
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

        public static async Task<string> AddElementToDisplay(NusysConstants.ElementType type, double x = 50, double y = 50, double width = 150, double height  = 150)
        {
            var args = new NewElementRequestArgs();
            args.Width = width;
            args.Height = height;
            args.AccessType = NusysConstants.AccessType.Public;
            args.LibraryElementId = "display";
            args.ParentCollectionId = "Doesn't matter!";
            args.X = x;
            args.Y = y;
            args.Id = SessionController.Instance.GenerateId();
            var request = new NewElementRequest(args);
            request.SetSystemProperties(new Dictionary<string, object>()
            {
                {NusysConstants.LIBRARY_ELEMENT_TYPE_KEY, type.ToString()},
                {"hacky_type_fix", type.ToString()}
            });
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddReturnedElementToSession();
            return args.Id;
        }
    }
}
