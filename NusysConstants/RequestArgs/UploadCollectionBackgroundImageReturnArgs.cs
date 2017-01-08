using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Return Args class for the UploadCollectionBackgroundImage request.
    /// This should simply add in the string URL for the requested image upload.
    /// </summary>
    public class UploadCollectionBackgroundImageReturnArgs : ServerReturnArgsBase
    {
        /// <summary>
        /// The string url for the requested image upload.
        /// REQUIRED for return on a succesfull return
        /// </summary>
        public string Url = null;

        /// <summary>
        /// This should simply return false if the string URL is null, true otherwise.
        /// </summary>
        /// <returns></returns>
        protected override bool CheckIsValid()
        {
            return Url != null;
        }
    }
}
