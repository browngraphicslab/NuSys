using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;
using NuSysApp.Controller;

namespace NuSysApp
{
    public class ElementController
    {
        private NetworkUser _lastNetworkUser;
        private ElementModel _model;
        private DebouncingDictionary _debouncingDictionary;

        public delegate void AlphaChangedEventHandler(object source, double alpha);

        public delegate void DeleteEventHandler(object source);

        public delegate void LocationUpdateEventHandler(object source, double x, double y);

        public delegate void MetadataChangeEventHandler(object source, string key);

        public delegate void NetworkUserChangedEventHandler(NetworkUser user);

        public delegate void ScaleChangedEventHandler(object source, double sx, double sy);

        public delegate void TitleChangedHandler(object source, string title);

        public delegate void SizeUpdateEventHandler(object source, double width, double height);

        public delegate void ContentLoadedHandler(object source, NodeContentModel data);

        public delegate void LinkAddedEventHandler(object source, LinkElementController linkController);

        public event DeleteEventHandler Deleted;
        public event LinkAddedEventHandler LinkedAdded;
        public event ContentLoadedHandler ContentLoaded;
        public event ContentLoadedHandler ContentChanged;
        public event MetadataChangeEventHandler MetadataChange;
        public event LocationUpdateEventHandler PositionChanged;
        public event SizeUpdateEventHandler SizeChanged;
        public event ScaleChangedEventHandler ScaleChanged;
        public event AlphaChangedEventHandler AlphaChanged;
        public event TitleChangedHandler TitleChanged;
        public event NetworkUserChangedEventHandler UserChanged;

        public ElementController(ElementModel model)
        {
            _model = model;
            _debouncingDictionary = new DebouncingDictionary(model.Id);
        }
        public virtual async Task FireContentLoaded(NodeContentModel content)
        {
            ContentLoaded?.Invoke(this, content);
        }

        public void SetCreator(string parentId)
        {
            Model.Creator = parentId;
        }

        public void AddLink(LinkElementController linkController)
        {
            var linkModel = (LinkModel)linkController.Model;
            LinkedAdded?.Invoke(this, linkController);
        }

        public void SetScale(double sx, double sy)
        {
            Model.ScaleX = sx;
            Model.ScaleY = sy;

            ScaleChanged?.Invoke(this, sx, sy);

            _debouncingDictionary.Add("scaleX", sx);
            _debouncingDictionary.Add("scaleY", sy);
        }

        public void SetSize(double width, double height)
        {
            if (width < 5 || height < 5)
            {
                return;
            }
            Model.Width = width;
            Model.Height = height;

            SizeChanged?.Invoke(this, width, height);

            _debouncingDictionary.Add("width", width);
            _debouncingDictionary.Add("height", height);
        }

 

        public void SetPosition(double x, double y)
        {
            Model.X = x;
            Model.Y = y;

            PositionChanged?.Invoke(this, x, y);

            _debouncingDictionary.Add("x", x);
            _debouncingDictionary.Add("y", y);
        }

        public void SetAlpha(double alpha)
        {
            Model.Alpha = alpha;

            AlphaChanged?.Invoke(this, alpha);

            _debouncingDictionary.Add("alpha", alpha);
        }

        public void SetTitle(string title)
        {
            Model.Title = title;

            TitleChanged?.Invoke(this, title);

            _debouncingDictionary.Add("title", title);
        }

        public void SetMetadata(string key, object val)
        {
            Model.SetMetaData(key, val);
            MetadataChange?.Invoke(this, key);
        }

        public void Delete()
        {
            Deleted?.Invoke(this);
        }

        public async virtual Task RequestDelete()
        {
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(Model.Id));
        }


        public async virtual Task RequestDuplicate(double x, double y)
        {
            Message m = new Message();
            m["contentId"] = Model.ContentId;
            m["data"] = "";
            m["x"] = x;
            m["y"] = y;
            m["width"] = Model.Width;
            m["height"] = Model.Height;
            m["nodeType"] = Model.ElementType.ToString();
            m["creator"] = Model.Creator;
            m["creatorContentID"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));
        }

        public virtual async Task RequestLinkTo(string otherId)
        {
            var contentId = SessionController.Instance.GenerateId();
            var libraryElementRequest = new CreateNewLibraryElementRequest(contentId,null,ElementType.Link, "NEW LINK");
            var request = new NewLinkRequest(Model.Id, otherId, Model.Creator,contentId);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(libraryElementRequest);
        }

        public virtual async Task RequestMoveToCollection(string newCollectionId,string newCollectionContentID)
        {
            var metadata = new Dictionary<string, object>();
            metadata["node_creation_date"] = DateTime.Now;

            var m1 = new Message();
            m1["metadata"] = metadata;
            m1["contentId"] = Model.ContentId;
            m1["nodeType"] = Model.ElementType;
            m1["x"] = 50000;
            m1["y"] = 50000;
            m1["width"] = 200;
            m1["height"] = 200;
            m1["autoCreate"] = true;
            m1["creator"] = newCollectionId;
            m1["creatorContentID"] = newCollectionContentID;

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(Model.Id));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m1));

        }

        public void SetLastNetworkUser( NetworkUser user )
        {
            if (user != null)
            {
                _lastNetworkUser?.RemoveAtomInUse(_model);
                user.AddAtomInUse(_model);
                _lastNetworkUser = user;
                UserChanged?.Invoke(user);
            }
            else
            {
                _lastNetworkUser = null;
                UserChanged?.Invoke(null);
            }
         }

        public ElementModel Model
        {
            get { return _model; }
        }
        public NodeContentModel ContentModel
        {
            get
            {
                if (Model.ContentId != null && SessionController.Instance.ContentController.Get(Model.ContentId) != null)
                {
                    return SessionController.Instance.ContentController.Get(Model.ContentId);
                }
                return null;
            }
        }

        public virtual async Task UnPack(Message props)
        {
            if (props.ContainsKey("data"))
            {
                var content = SessionController.Instance.ContentController.Get(props.GetString("contentId", ""));
                if (content != null)
                {
                    content.Data = props.GetString("data", "");
                    ContentChanged?.Invoke(this, content);
                }
            }
            if (props.ContainsKey("x") || props.ContainsKey("y"))
            {
                //if either "x" or "y" are not found in props, x/y stays the current value stored in Model.X/Y
                var x = props.GetDouble("x", this.Model.X);
                var y = props.GetDouble("y", this.Model.Y);
                PositionChanged?.Invoke(this, x,y);
            }
            if (props.ContainsKey("width") || props.ContainsKey("height"))
            {
                var width = props.GetDouble("width", this.Model.Width);
                var height = props.GetDouble("height", this.Model.Height);
                SizeChanged?.Invoke(this,width,height);
            }
        }
    }
}
