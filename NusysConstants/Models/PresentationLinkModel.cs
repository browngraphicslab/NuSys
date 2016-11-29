using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NusysIntermediate
{
    /// <summary>
    /// the Model class for presentation links.  
    /// since the presentation links are Element-to-Element, they require a ParentCollectionId to be fetched and saved correctly.
    /// </summary>
    public class PresentationLinkModel
    {
        /// <summary>
        /// The unique Id for this link.  Helpful for deleting specific links
        /// </summary>
        public string LinkId { get; set; }

        /// <summary>
        /// The ElementId of the element that ends this presentation link.
        /// </summary>
        public string OutElementId { get; set; }

        /// <summary>
        /// The ElementId of the element that is the start of this presentation link.
        /// </summary>
        public string InElementId { get; set; }

        /// <summary>
        /// the string that will hold the text of the presentation links when annotations are desired.
        /// Approximately max 2048 characters.
        /// </summary>
        public string AnnotationText { get; set; }

        /// <summary>
        /// The LibraryId of the collection that this link will exist on.  
        /// </summary>
        public string ParentCollectionId { get; set; }
    }
}
