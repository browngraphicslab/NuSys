using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Windows.UI;
using NusysIntermediate;

namespace NuSysApp
{
    public class WordNodeLibraryElementController : PdfLibraryElementController
    {
        /// <summary>
        ///  returns the libraryElementId so the lockcontroller can identify this object
        /// </summary>
        public string Id
        {
            get { return LibraryElementModel.LibraryElementId; }
        }

        /// <summary>
        /// kept track of by the lockcontroller, 
        /// should be treated as read-only, and only being set by the lcok controller
        /// </summary>

        public WordNodeLibraryElementController(WordLibraryElementModel model) : base(model)
        {
            Debug.Assert(model.Type == NusysConstants.ElementType.Word);
        }

    }
}
