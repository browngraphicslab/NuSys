using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class ElementInstanceController
    {
        private NetworkUser _lastNetworkUser;
        private ElementInstanceModel _model;
        private DebouncingDictionary _debouncingDictionary;

        public delegate void AlphaChangedEventHandler(object source, double alpha);
        public delegate void DeleteEventHandler(object source);
        public delegate void LocationUpdateEventHandler(object source, double x, double y);
        public delegate void MetadataChangeEventHandler(object source, string key);
        public delegate void NetworkUserChangedEventHandler(NetworkUser user);
        public delegate void ScaleChangedEventHandler(object source, double sx, double sy);
        public delegate void TitleChangedHandler(object source, string title);
        public delegate void SizeUpdateEventHandler(object source, double width, double height);
        public delegate void CanEditChangedEventHandler(object source, EditStatus status);
        public event MetadataChangeEventHandler MetadataChange;
        public event DeleteEventHandler Deleted;
        public event LocationUpdateEventHandler PositionChanged;
        public event LocationUpdateEventHandler Translated;
        public event SizeUpdateEventHandler SizeChanged;
        public event SizeUpdateEventHandler Resized;
        public event ScaleChangedEventHandler ScaleChanged;
        public event AlphaChangedEventHandler AlphaChanged;
        public event TitleChangedHandler TitleChanged;
        public event NetworkUserChangedEventHandler UserChanged;
        public event CanEditChangedEventHandler CanEditChange;

        private EditStatus _editStatus;

        public ElementInstanceController(ElementInstanceModel model)
        {
            _model = model;
            _debouncingDictionary = new DebouncingDictionary(model.Id);
            _editStatus = EditStatus.Maybe;
        }

        public void SetScale(double sx, double sy)
        {
            ScaleChanged?.Invoke(this, sx, sy);
        }

        public void SetSize(double width, double height)
        {
            SizeChanged?.Invoke(this, width, height);
        }

        public void Resize(double dx, double dy)
        {
            var changeX = dx / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var changeY = dy / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleY;

            Resized?.Invoke(this, changeX, changeY);

            _debouncingDictionary?.Add("resizeDeltaWidth", dx);
            _debouncingDictionary?.Add("resizeDeltaHeight", dy);
        }

        public void SetPosition(double x, double y)
        {
            PositionChanged?.Invoke(this, x, y);

            _debouncingDictionary?.Add("x", Model.X);
            _debouncingDictionary?.Add("y", Model.Y);
        }

        public void Translate(double tx, double ty)
        {
            tx /= SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            ty /= SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleY;
            Translated?.Invoke(this, tx, ty);

            _debouncingDictionary?.Add("translateX", tx);
            _debouncingDictionary?.Add("translateY", ty);
        }

        public void SetAlpha(double alpha)
        {
            AlphaChanged?.Invoke(this, alpha);
        }

        public void SetTitle(string title)
        {
            TitleChanged?.Invoke(this, title);
        }

        public void SetMetadata(string key, object val)
        {
            MetadataChange?.Invoke(this, key);
        }

        public void Delete()
        {
            Deleted?.Invoke(this);
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

        public ElementInstanceModel Model
        {
            get { return _model; }
        }

    }
}
