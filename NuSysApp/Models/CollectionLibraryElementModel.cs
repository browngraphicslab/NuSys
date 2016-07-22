using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Microsoft.Graphics.Canvas.Geometry;

namespace NuSysApp
{
    public class CollectionLibraryElementModel : LibraryElementModel
    {
        private HashSet<string> _children;

        public HashSet<string> InkLines;

        public delegate void InkEventHandler(string id);
        public event InkEventHandler OnInkAdded;
        public event InkEventHandler OnInkRemoved;

        public delegate void ChildAddedEventHandler(string id);
        public event ChildAddedEventHandler OnChildAdded;

        public delegate void ChildRemovedEventHandler(string id);
        public event ChildRemovedEventHandler OnChildRemoved;
        public bool IsFinite { get; set; }
        public PointCollection Shape { get; set; }

        public CollectionLibraryElementModel(string id, Dictionary<String, MetadataEntry> metadata = null, string contentName = null, bool favorited = false) : base(id, ElementType.Collection, metadata, contentName)
        {
            _children = new HashSet<string>();

            this.Favorited = favorited;

            InkLines = new HashSet<string>();
            IsFinite = false;
            Shape = null;

        }

        public void AddInk(string id)
        {
            InkLines.Add(id);
            OnInkAdded?.Invoke(id);
        }

        public void RemoveInk(string id)
        {
            InkLines.Remove(id);
            OnInkAdded?.Invoke(id);
        }

        public bool AddChild(string id)
        {
            if (!_children.Contains(id))
            {
                var elementController = SessionController.Instance.IdToControllers[id];
                elementController.Deleted += ElementControllerOnDeleted;

                _children.Add(id);
                OnChildAdded?.Invoke(id);
                return true;
            }
            return false;
        }

        private void ElementControllerOnDeleted(object source)
        {
            var elementController = (ElementController)source;
            Children.Remove(elementController.Model.Id);
        }

        protected override void OnSessionControllerEnterNewCollection()
        {
            _children.Clear();
            base.OnSessionControllerEnterNewCollection();
        }

        public bool RemoveChild(string id)
        {
            if (_children.Contains(id))
            {
                var elementController = SessionController.Instance.IdToControllers[id];
                elementController.Deleted += ElementControllerOnDeleted;
                _children.Remove(id);
                OnChildRemoved?.Invoke(id);
                return true;
            }
            return false;
        }
        public HashSet<string> Children { get { return _children; } }
    }
}
