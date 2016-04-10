using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public delegate void DisposeEventHandler(object source);

        public delegate void LocationUpdateEventHandler(object source, double x, double y, double dx = 0, double dy = 0);

        public delegate void MetadataChangeEventHandler(object source, string key);

        public delegate void NetworkUserChangedEventHandler(NetworkUser user);

        public delegate void ScaleChangedEventHandler(object source, double sx, double sy);

        public delegate void TitleChangedHandler(object source, string title);

        public delegate void SizeUpdateEventHandler(object source, double width, double height);

        public delegate void ContentLoadedHandler(object source, LibraryElementModel data);

        public delegate void LinkAddedEventHandler(object source, LinkElementController linkController);

        public event DisposeEventHandler Disposed;
        public event DeleteEventHandler Deleted;
        public event LinkAddedEventHandler LinkedAdded;
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
            if (_model != null)
            {
                _debouncingDictionary = new DebouncingDictionary(model.Id);
            }
            if (LibraryElementModel != null)
            {
                LibraryElementModel.OnDelete += Delete;
            }
        }

        public void Dispose()
        {
            var delegates1 = SizeChanged?.GetInvocationList();
            if (delegates1 != null)
                foreach (var d in delegates1)
                {
                    SizeChanged -= (SizeUpdateEventHandler)d;
                }
            var invocationList1 = ScaleChanged?.GetInvocationList();
            if (invocationList1 != null)
                foreach (var d in invocationList1)
                {
                    ScaleChanged -= (ScaleChangedEventHandler)d;
                }
            var ds = AlphaChanged?.GetInvocationList();
            if (ds != null)
                foreach (var d in ds)
                {
                    AlphaChanged -= (AlphaChangedEventHandler)d;
                }
            var list = TitleChanged?.GetInvocationList();
            if (list != null)
                foreach (var d in list)
                {
                    TitleChanged -= (TitleChangedHandler)d;
                }
            var delegates = UserChanged?.GetInvocationList();
            if (delegates != null)
                foreach (var d in delegates)
                {
                    UserChanged -= (NetworkUserChangedEventHandler)d;
                }

            var invocationList = Deleted?.GetInvocationList();
            if (invocationList != null)
                foreach (var d in invocationList)
                {
                    Deleted -= (DeleteEventHandler)d;
                }

            var invocationList2 = LinkedAdded?.GetInvocationList();
            if (invocationList2 != null)
                foreach (var d in LinkedAdded?.GetInvocationList())
            {
                LinkedAdded -= (LinkAddedEventHandler)d;
            }

            var invocationList3 = MetadataChange?.GetInvocationList();
            if (invocationList3 != null)
                foreach (var d in MetadataChange?.GetInvocationList())
            {
                MetadataChange -= (MetadataChangeEventHandler)d;
            }

            var invocationList4 = PositionChanged?.GetInvocationList();
            if (invocationList4 != null)
                foreach (var d in PositionChanged?.GetInvocationList())
            {
                PositionChanged -= (LocationUpdateEventHandler)d;
            }

            Disposed?.Invoke(this);
        }

        public void AddLink(LinkElementController linkController)
        {
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
            var px = Model.X;
            var py = Model.Y;
            Model.X = x;
            Model.Y = y;

            PositionChanged?.Invoke(this, x, y, x - px, y - py);

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
            m["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));
        }

        public virtual async Task RequestLinkTo(string otherId, Dictionary<string, object> inFGDictionary = null, Dictionary<string, object> outFGDictionary = null)
        {
            var contentId = SessionController.Instance.GenerateId();
            var libraryElementRequest = new CreateNewLibraryElementRequest(contentId,null,ElementType.Link, "NEW LINK");
            var request = new NewLinkRequest(Model.Id, otherId, Model.Creator, contentId, inFGDictionary, outFGDictionary);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(libraryElementRequest);
        }

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
            // TODO: remove temp
            Random rnd = new Random();
            metadata["random_id"] = rnd.Next(1, 1000);
            metadata["random_id2"] = rnd.Next(1, 100);
            var m1 = new Message();
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
        public LibraryElementModel LibraryElementModel
        {
            get
            {
                if (Model.LibraryId != null && SessionController.Instance.ContentController.Get(Model.LibraryId) != null)
                {
                    return SessionController.Instance.ContentController.Get(Model.LibraryId);
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
