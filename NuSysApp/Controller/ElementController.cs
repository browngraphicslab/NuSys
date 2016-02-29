using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class ElementController
    {
        private NetworkUser _lastNetworkUser;
        private ElementModel _model;
        private DebouncingDictionary _debouncingDictionary;

        public delegate void AlphaChangedEventHandler(object source, double alpha);
        public delegate void LocationUpdateEventHandler(object source, double x, double y);
        public delegate void MetadataChangeEventHandler(object source, string key);
        public delegate void NetworkUserChangedEventHandler(NetworkUser user);
        public delegate void ScaleChangedEventHandler(object source, double sx, double sy);
        public delegate void TitleChangedHandler(object source, string title);
        public delegate void SizeUpdateEventHandler(object source, double width, double height);
        public delegate void CanEditChangedEventHandler(object source, EditStatus status);
        public delegate void ContentLoadedHandler(object source, NodeContentModel data);
        public event ContentLoadedHandler ContentLoaded;
        public event MetadataChangeEventHandler MetadataChange;
        public event LocationUpdateEventHandler PositionChanged;
        public event SizeUpdateEventHandler SizeChanged;
        public event ScaleChangedEventHandler ScaleChanged;
        public event AlphaChangedEventHandler AlphaChanged;
        public event TitleChangedHandler TitleChanged;
        public event NetworkUserChangedEventHandler UserChanged;
        public event CanEditChangedEventHandler CanEditChange;

        private EditStatus _editStatus;

        public ElementController(ElementModel model)
        {
            _model = model;
            _debouncingDictionary = new DebouncingDictionary(model.Id);
            _editStatus = EditStatus.Maybe;
        }

        public virtual async Task FireContentLoaded( NodeContentModel content )
        {
            ContentLoaded?.Invoke(this, content);
        }

        public void SetCreator(string parentId)
        {
            Model.Creator = parentId;
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
            MetadataChange?.Invoke(this, key);
        }

        public virtual void Delete()
        {
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(Model.Id));
        }
        public virtual void Duplicate()
        {
            Message m = new Message();
            m["contentId"] = Model.ContentId;
            m["data"] = "";
            m["x"] = Model.X;
            m["y"] = Model.Y;
            m["width"] = 400;
            m["height"] = 400;
            m["nodeType"] = Model.ElementType.ToString();
            m["creator"] = Model.Creator;

            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));
        }

        public virtual void MoveToCollection(ElementCollectionController collection)
        {
            Delete();
            collection.AddChild(this);
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


        public EditStatus CanEdit
        {
            get { return _editStatus; }
            set
            {
                if (_editStatus == value)
                {
                    return;
                }
                _editStatus = value;
                CanEditChange?.Invoke(this, CanEdit);
            }
        }

        public ElementModel Model
        {
            get { return _model; }
        }

        public virtual async Task UnPack(Message props)
        {
        }

    }
}
