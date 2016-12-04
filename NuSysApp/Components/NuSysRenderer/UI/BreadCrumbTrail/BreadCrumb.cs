using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace NuSysApp
{
    /// <summary>
    /// BreadCrumb Data Object used to stored the necessary data to display the bread crumb
    /// </summary>
    public class BreadCrumb
    {
        /// <summary>
        /// The library element controller associated with this bread crumb
        /// </summary>
        public LibraryElementController Controller { get; }

        /// <summary>
        /// The controller for the collection the breadcrumb was found in
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
        public Color Color;

        /// <summary>
        /// Creates a new breadcrumb data object, the controller is the controller associated with the breadcrumb, the collection controller
        /// is the controller for the collection the breadcrumb was found on. To create a breadcrumb when entering a new collection just pass
        /// the library element controller and collection library elment controller for that new collection
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="collectionController"></param>
        /// <param name="isCollection"></param>
        public BreadCrumb(LibraryElementController controller, LibraryElementController collectionController)
        {
            // set default values
            Controller = controller;
            CollectionController = collectionController;

            // its a collection if the collection controller and library element controller are the same
            IsCollection = collectionController.LibraryElementModel.LibraryElementId ==
                           Controller.LibraryElementModel.LibraryElementId;

            // set the color of the breadcrumb based on the hash of the collection controller's library element id
            Color = MediaUtil.GetHashColorFromString(CollectionController.LibraryElementModel.LibraryElementId);

        }
    }
}
