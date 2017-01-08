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

        /// <summary>
        /// The canvas the bread crumb is going to be rendered on, this may break encapsulation but is necessary for turning the 
        /// smalliconurl into a CanvasBitmap for display in win2d
        /// </summary>
        private ICanvasResourceCreator ResourceCreator { get; set; }

        public delegate void OnBreadCrumbDeletedHandler(BreadCrumb sender);

        /// <summary>
        /// Fired when the breadcrumb is deleted, the breadcrumb disposes of itself
        /// </summary>
        public event OnBreadCrumbDeletedHandler Deleted;

        public delegate void OnTitleChangedEventHandler(BreadCrumb sender, string title);


        /// <summary>
        /// Fired when the title of the breadcrumb changes
        /// </summary>
        public event OnTitleChangedEventHandler TitleChanged;

        /// <summary>
        /// The title of the breadcrumb
        /// </summary>
        public string Title { get; set; }


        public BreadCrumb(LibraryElementController collectionController, ICanvasResourceCreator resourceCreator, ElementController controller = null)
        {
            // set default values
            ElementController = controller;
            CollectionController = collectionController;

            // its a collection if there is no element model
            IsCollection = controller == null;

            // set the initial title
            Title = IsCollection
                ? CollectionController.LibraryElementModel?.Title
                : ElementController.LibraryElementModel?.Title ?? "";

            // set the color of the breadcrumb based on the hash of the collection controller's library element id
            Color = MediaUtil.GetHashColorFromString(CollectionController.LibraryElementModel.LibraryElementId);

            ResourceCreator = resourceCreator;

            // delete the bread crumb if its collection has been deleted
            CollectionController.Deleted += OnBreadCrumbDeleted;

            // delete the breadcrumb if it is not a collection and its element has been deleted
            if (ElementController != null)
            {
                ElementController.Deleted += OnBreadCrumbDeleted;
                ElementController.LibraryElementController.TitleChanged += OnTitleChanged;
            }
            else
            {
                CollectionController.TitleChanged += OnTitleChanged;
            }

        }

        /// <summary>
        /// Fired when the title is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTitleChanged(object sender, string e)
        {
            Title = e;
            TitleChanged?.Invoke(this, e);
        }

        private void OnBreadCrumbDeleted(object sender)
        {
            Deleted?.Invoke(this);
            Dispose();
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

        /// <summary>
        /// override the != operator
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(BreadCrumb a, BreadCrumb b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Remove any events
        /// </summary>
        public void Dispose()
        {
            CollectionController.Deleted -= OnBreadCrumbDeleted;
            if (ElementController != null)
            {
                ElementController.Deleted -= OnBreadCrumbDeleted;

                // we have to null check in case the LibraryElementController was deleted
                if (ElementController.LibraryElementController != null)
                {
                    ElementController.LibraryElementController.TitleChanged -= OnTitleChanged;
                }
            }
            else
            {
                CollectionController.TitleChanged -= OnTitleChanged;
            }


        }
    }
}
