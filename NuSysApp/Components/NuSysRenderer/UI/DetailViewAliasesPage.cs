using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    internal class DetailViewAliasesPage : RectangleUIElement
    {
        private StackLayoutManager _layoutManager;
        private RectangleUIElement _aliasesList;
        private LibraryElementController _controller;
        private List<ElementModel> _aliasList;

        public DetailViewAliasesPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) :
            base(parent, resourceCreator)
        {

            _controller = controller;

            ListViewUIElementContainer<ElementModel> listView = new ListViewUIElementContainer<ElementModel>(parent, resourceCreator);
            listView.Height = Height;
            listView.Width = Width;
            listView.Transform.LocalPosition = new Vector2(0, 0);
            listView.AddItems(_aliasList);

            ListTextColumn<ElementModel> title = new ListTextColumn<ElementModel>();
            title.RelativeWidth = 1;
            title.ColumnFunction = delegate(ElementModel e)
            {
                return e.Title;
            };

            //ListTextColumn<string> creator = new ListTextColumn<string>();
            //creator.RelativeWidth = 1;
            //creator.ColumnFunction = delegate(string creatorString)
            //{
            //    return "creator";
            //};

            List<ListColumn<ElementModel>> cols = new List<ListColumn<ElementModel>>();
            cols.Add(title);
            //cols.Add(creator);
            listView.AddColumns(cols);
                
                
                
                _aliasesList = new RectangleUIElement(parent, resourceCreator);
            _aliasesList.Background = Colors.Aqua;
            _layoutManager = new StackLayoutManager(StackAlignment.Vertical);
            AddChild(_aliasesList);
            _layoutManager.AddElement(_aliasesList);

            AddChild(listView);

        }

        public override Task Load()
        {
            List<ElementModel> aliasList = GetAliases(_controller).Result;
            return base.Load();
        }

        private async Task<List<ElementModel>> GetAliases(LibraryElementController controller)
        {
            GetAliasesOfLibraryElementRequest req = new GetAliasesOfLibraryElementRequest(controller.LibraryElementModel.LibraryElementId);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(req);
            return req.GetReturnedElementModels();
           
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _layoutManager.SetSize(Width, Height);
            _layoutManager.VerticalAlignment = VerticalAlignment.Center;
            _layoutManager.HorizontalAlignment = HorizontalAlignment.Center;
            _layoutManager.ItemHeight = Height - 20;
            _layoutManager.ItemWidth = Width - 20;
            _layoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}