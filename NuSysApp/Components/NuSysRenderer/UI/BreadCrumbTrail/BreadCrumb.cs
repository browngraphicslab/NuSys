using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// BreadCrumb Data Object used to stored the necessary data to display the bread crumb
    /// </summary>
    public class BreadCrumb : IDisposable
    {
        /// <summary>
        /// The element model associated with the bread crumb, null if the bread crumb represents a collection
        /// </summary>
        public ElementController ElementController { get; }

        /// <summary>
        /// The id of the parent collection the breadcrumb was found in
        /// </summary>
        public LibraryElementController CollectionController { get; }

        /// <summary>
        /// True if the breadcrumb represents going to a new collection. For example when we enter a workspace
        /// but not if we clicked on a collection node
        /// </summary>
        public bool IsCollection { get;  }

        /// <summary>
        /// The color of the breadcrumb
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// Preview icon for the breadcrumb
        /// </summary>
        public CanvasBitmap Icon { get; private set; }

        public ICanvasResourceCreator ResourceCreator { get; set; }

        public delegate void OnBreadCrumbDeletedHandler(BreadCrumb sender);

        public event OnBreadCrumbDeletedHandler Deleted; 


        public BreadCrumb(LibraryElementController collectionController, ICanvasResourceCreator resourceCreator, ElementController controller = null)
        {
            // set default values
            ElementController = controller;
            CollectionController = collectionController;

            // its a collection if there is no element model
            IsCollection = controller == null;

            // set the color of the breadcrumb based on the hash of the collection controller's library element id
            Color = MediaUtil.GetHashColorFromString(CollectionController.LibraryElementModel.LibraryElementId);

            ResourceCreator = resourceCreator;

            CollectionController.Deleted += OnBreadCrumbDeleted;
            if (ElementController != null)
            {
                ElementController.Deleted += OnBreadCrumbDeleted;
            }
        }

        private void OnBreadCrumbDeleted(object sender)
        {
            Deleted?.Invoke(this);

        }



        /// <summary>
        /// Load the breadcrumb
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {
            if (!IsCollection)
            {
                Icon = await CanvasBitmap.LoadAsync(ResourceCreator, ElementController.LibraryElementController.SmallIconUri);
            }
            else
            {
                Icon = await CanvasBitmap.LoadAsync(ResourceCreator, CollectionController.SmallIconUri);
            }

        }

        /// <summary>
        /// override the equality operator
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(BreadCrumb a, BreadCrumb b)
        {
            return a?.CollectionController == b?.CollectionController && a?.ElementController == b?.ElementController;
        }

        public static bool operator !=(BreadCrumb a, BreadCrumb b)
        {
            return !(a == b);
        }

        public void Dispose()
        {
            CollectionController.Deleted -= OnBreadCrumbDeleted;
            if (ElementController != null)
            {
                ElementController.Deleted -= OnBreadCrumbDeleted;
            }
        }
    }
}
