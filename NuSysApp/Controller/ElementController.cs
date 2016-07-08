using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;
using NuSysApp.Components.Nodes;
using NuSysApp.Components.Viewers.FreeForm;
using NuSysApp.Controller;
using NuSysApp.Util;
using NuSysApp.Nodes.AudioNode;
using NuSysApp.Viewers;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class ElementController : ILinkable
    {
        private ElementModel _model;
        protected DebouncingDictionary _debouncingDictionary;

        public delegate void AlphaChangedEventHandler(object source, double alpha);

        public delegate void DeleteEventHandler(object source);

        public delegate void LocationUpdateEventHandler(object source, double x, double y, double dx = 0, double dy = 0);

        public delegate void MetadataChangeEventHandler(object source, string key);

        public delegate void ScaleChangedEventHandler(object source, double sx, double sy);

        public delegate void SizeUpdateEventHandler(object source, double width, double height);

        public delegate void RegionTestChangedEventHandler(object source, RectangleViewModel region);

        public delegate void SelectionChangedHandler(object source, bool selected);

        public event EventHandler Disposed;
        public event DeleteEventHandler Deleted;
        public event MetadataChangeEventHandler MetadataChange;
        public event LocationUpdateEventHandler PositionChanged;
        public event SizeUpdateEventHandler SizeChanged;
        public event ScaleChangedEventHandler ScaleChanged;
        public event AlphaChangedEventHandler AlphaChanged;
        public event RegionTestChangedEventHandler RegionTestChanged;
        public event SelectionChangedHandler SelectionChanged;
        public event EventHandler<Point2d> AnchorChanged;


        public Point2d Anchor
        {
            get
            {
                return new Point2d(Model.X + Model.Width/2, Model.Y+Model.Width / 2);
            }
        }


        public ElementController(ElementModel model)
        {
            _model = model;
            
         //   Debug.WriteLine(Model.Title);

         //   LibraryElementModel.SetTitle(Model.Title);

            if (_model != null)
            {
                _debouncingDictionary = new DebouncingDictionary(model.Id);
            }
            if (LibraryElementController != null)
            {
                LibraryElementController.Deleted += Delete;
                var title = LibraryElementModel.Title;
                Model.Title = title;
            }
            Debug.Assert(this.Id != null);
            SessionController.Instance.LinksController.AddLinkable(this);
        }


        public virtual void Dispose()
        {
            if (LibraryElementController != null)
            {
                LibraryElementController.Deleted -= Delete;
            }
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public void SetScale(double sx, double sy)
        {
            Model.ScaleX = sx;
            Model.ScaleY = sy;

            ScaleChanged?.Invoke(this, sx, sy);

            _debouncingDictionary.Add("scaleX", sx);
            _debouncingDictionary.Add("scaleY", sy);
        }

        public void Selected(bool selected)
        {
            SelectionChanged?.Invoke(this, selected);
        }

        public virtual void SetSize(double width, double height)
        {
            if (width < 20 || height < 20)
            {
                return;
            }

            Model.Width = width;
            Model.Height = height;
            SizeChanged?.Invoke(this, width, height);
            AnchorChanged?.Invoke(this,Anchor);
            _debouncingDictionary.Add("width", width);
            _debouncingDictionary.Add("height", height);
        }

 

        public void SetPosition(double x, double y)
        {
            var px = Model.X;
            var py = Model.Y;
            Model.X = x;
            Model.Y = y;

            PositionChanged?.Invoke(this, x, y, x - px, y - py);
            AnchorChanged?.Invoke(this, Anchor);

            _debouncingDictionary.Add("x", x);
            _debouncingDictionary.Add("y", y);
        }

        public void SaveTimeBlock()
        {
            switch (Model.ElementType)
            {
                case ElementType.Image:
                    break;
                case ElementType.Text:
                    break;
                case ElementType.Audio:
                    _debouncingDictionary.Add("linkedTimeModels", ((AudioNodeModel)Model).LinkedTimeModels);

                    break;
                case ElementType.Video:
                    _debouncingDictionary.Add("linkedTimeModels", ((VideoNodeModel)Model).LinkedTimeModels);
                    break;
            }
        }

        public void SetAlpha(double alpha)
        {
            Model.Alpha = alpha;

            AlphaChanged?.Invoke(this, alpha);

            _debouncingDictionary.Add("alpha", alpha);
        }

        public void SetRegionModel(RectangleViewModel region)
        {
            Model.RegionsModel.Add(region);
            RegionTestChanged?.Invoke(this, region);
            _debouncingDictionary.Add("regionsModel", Model.RegionsModel);
        }

        public void AddPageRegion(int page, RectangleViewModel region)
        {
            if ((Model as PdfNodeModel).PageRegionDict.ContainsKey(page))
            {
                (Model as PdfNodeModel).PageRegionDict[page].Add(region);
            }
            else
            {
                (Model as PdfNodeModel).PageRegionDict[page] = new List<RectangleViewModel>() { region };
            }

            var m = new Message();
            m["pageRegionDict"] = (Model as PdfNodeModel).PageRegionDict;
            m["id"] = Model.Id;
            var request = new SendableUpdateRequest(m, true);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            //_debouncingDictionary.Add("pageRegionDict", (Model as PdfNodeModel).PageRegionDict);
        }

        public void Delete(object sender)
        {
            Deleted?.Invoke(this);
            SessionController.Instance.ActiveFreeFormViewer.DeselectAll();

            Dispose();
        }

        public async virtual Task RequestDelete()
        {
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(Model.Id));
        }
        public async virtual Task RequestDuplicate(double x, double y, Message m = null)
        {
           if (m == null)
                m = new Message();

            m.Remove("id");
            m["contentId"] = Model.LibraryId;
            m["data"] = "";
            m["x"] = x;
            m["y"] = y;
            m["width"] = Model.Width;
            m["height"] = Model.Height;
            m["nodeType"] = Model.ElementType.ToString();
            m["creator"] = Model.ParentCollectionId;
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));
        }
        /*
        public virtual async Task RequestLinkTo(string otherId, RectangleView rectangle = null, UserControl regionView = null, Dictionary<string, object> inFGDictionary = null, Dictionary<string, object> outFGDictionary = null)
        {
            var contentId = SessionController.Instance.GenerateId();
            var libraryElementRequest = new CreateNewLibraryElementRequest(contentId,null,ElementType.Link, "NEW LINK");
            var request = new NewLinkRequest(new string(Model.ContentId), otherId, Model.ParentCollectionId,contentId, regionView, rectangle, inFGDictionary, outFGDictionary);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(libraryElementRequest);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
        }*/
        /*
        public void RequestVisualLinkTo(string id)
        {
            var parent = SessionController.Instance.ContentController.GetContent(Model.ParentCollectionId) as CollectionLibraryElementModel;
            parent.addLink(id);
        }*/
        /*
        public void RequestDeleteVisualLink(string id)
        {
            var parent = SessionController.Instance.ContentController.GetContent(Model.ParentCollectionId) as CollectionLibraryElementModel;
            parent.removeLink(id);
        }*/
        /*
        public virtual async Task RequestPresentationLinkTo(string otherId, RectangleView rectangle = null, LinkedTimeBlock block = null, Dictionary<string, object> inFGDictionary = null, Dictionary<string, object> outFGDictionary = null)
        {

            var contentId = SessionController.Instance.GenerateId();
         //   var libraryElementRequest = new CreateNewLibraryElementRequest(contentId, null, ElementType.Link, "NEW PRESENTATION LINK");
            var request = new NewPresentationLinkRequest(Model.ContentId, otherId, Model.ParentCollectionId, contentId, block, rectangle, inFGDictionary, outFGDictionary, null, true);
   //         await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(libraryElementRequest);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
        }
        */
        public Dictionary<string, object> CreateImageDictionary(double x, double y, double height, double width)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("x", x);
            dic.Add("y", x);
            dic.Add("height", x);
            dic.Add("width", x);
            return dic;
        }

        public Dictionary<string, object> CreateMediaDictionary(TimeSpan start, TimeSpan end)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("start", start);
            dic.Add("end", end);
            return dic;
        }

        public Dictionary<string, object> CreateTextDictionary(double x, double y, double height, double width)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("x", x);
            dic.Add("y", x);
            dic.Add("height", x);
            dic.Add("width", x);
            return dic;
        }

        public virtual async Task RequestMoveToCollection(string newCollectionContentID, double x=50000, double y=50000)
        {
            var metadata = new Dictionary<string, object>();
            metadata["node_creation_date"] = DateTime.Now;

            var m1 = new Message(await Model.Pack());
            m1["metadata"] = metadata;
            m1["contentId"] = Model.LibraryId;
            m1["nodeType"] = Model.ElementType;
            m1["title"] = Model.Title;
            m1["x"] = x;
            m1["y"] = y;
            m1["width"] = 200;
            m1["height"] = 200;
            m1["autoCreate"] = true;
            m1["creator"] = newCollectionContentID;

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(Model.Id));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m1));

        }

        public ElementModel Model
        {
            get { return _model; }
        }
        public LibraryElementController LibraryElementController
        {
            get
            {
                return SessionController.Instance.ContentController.GetLibraryElementController(Model.LibraryId);
            }
        }
        public LibraryElementModel LibraryElementModel
        {
            get
            {
                return LibraryElementController?.LibraryElementModel;
            }
        }

        public string Id
        {
            get
            {
                Debug.Assert(Model != null);
                return Model.Id;
            }
        }

        public string ContentId
        {
            get
            {
                Debug.Assert(Model != null);
                return Model.LibraryId;
            }
        }

        public virtual async Task UnPack(Message props)
        {
            if (props.ContainsKey("data"))
            {
                var content = SessionController.Instance.ContentController.GetContent(props.GetString("contentId", ""));
                if (content != null)
                {
                    content.Data = props.GetString("data", "");
                }
            }
            if (props.ContainsKey("x") || props.ContainsKey("y"))
            {
                //if either "x" or "y" are not found in props, x/y stays the current value stored in Model.X/Y
                var x = props.GetDouble("x", this.Model.X);
                var y = props.GetDouble("y", this.Model.Y);
                Model.X = x;
                Model.Y = y;

                PositionChanged?.Invoke(this, x,y);
            }
            if (props.ContainsKey("width") || props.ContainsKey("height"))
            {
                var width = props.GetDouble("width", this.Model.Width);
                var height = props.GetDouble("height", this.Model.Height);
                SizeChanged?.Invoke(this,width,height);
            }

            if (props.ContainsKey("region"))
            {
                string region = props.Get("region");
                Debug.WriteLine("REGIONS!!!!" + region);
                //RegionChanged?.Invoke(this, region);
            }
        }
    }
}
