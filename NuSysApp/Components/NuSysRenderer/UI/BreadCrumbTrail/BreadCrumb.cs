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
    public class BreadCrumb
    {
        /// <summary>
        /// The element model associated with the bread crumb, null if the bread crumb represents a collection
        /// </summary>
        public ElementModel ElementModel { get; }

        /// <summary>
        /// The id of the parent collection the breadcrumb was found in
        /// </summary>
        public string CollectionId { get; }

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

        /// <summary>
        /// The collection id is the id of the collection if thebreadcrumb represents a collection,
        /// otherwise the id of the parent collection for the element model.
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="model"></param>
        public BreadCrumb(string collectionId, ICanvasResourceCreator resourceCreator, ElementModel model = null)
        {
            // set default values
            ElementModel = model;
            CollectionId = collectionId;

            // its a collection if there is no element model
            IsCollection = model == null;

            // set the color of the breadcrumb based on the hash of the collection controller's library element id
            Color = MediaUtil.GetHashColorFromString(CollectionId);

            ResourceCreator = resourceCreator;
        }

        

        /// <summary>
        /// Load the breadcrumb
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {
            if (!IsCollection)
            {
                var controller =
                    SessionController.Instance.ContentController.GetLibraryElementController(ElementModel.LibraryId);

                Icon = await CanvasBitmap.LoadAsync(ResourceCreator, controller.SmallIconUri);
            }
            else
            {
                var controller =
                    SessionController.Instance.ContentController.GetLibraryElementController(CollectionId);

                Icon = await CanvasBitmap.LoadAsync(ResourceCreator, controller.SmallIconUri);
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
            return a?.CollectionId == b?.CollectionId && a?.ElementModel == b?.ElementModel;
        }

        public static bool operator !=(BreadCrumb a, BreadCrumb b)
        {
            return !(a == b);
        }
    }
}
