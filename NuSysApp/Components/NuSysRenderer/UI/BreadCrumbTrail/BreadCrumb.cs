using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
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
        public Color Color;


        /// <summary>
        /// The collection id is the id of the collection if thebreadcrumb represents a collection,
        /// otherwise the id of the parent collection for the element model.
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="model"></param>
        public BreadCrumb(string collectionId, ElementModel model = null)
        {
            // set default values
            ElementModel = model;
            CollectionId = collectionId;

            // its a collection if there is no element model
            IsCollection = model == null;

            // set the color of the breadcrumb based on the hash of the collection controller's library element id
            Color = MediaUtil.GetHashColorFromString(CollectionId);
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
