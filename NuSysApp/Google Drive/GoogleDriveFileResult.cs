using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GoogleDriveFileResult
    {
        public String MimeType { get; set; }

        public String CreatedDate { get; set; }

        public String ModifiedDate { get; set; }

        public String SelfLink { get; set; }

        public String AlternateLink { get; set; }

        public String IconLink { get; set; }

        public String ThumbnailLink { get; set; }

        public String Title { get; set; }

        public List<String> OwnerNames { get; set; }

        public String id { get; set; }

        /// <summary>
        /// This creates a google drive file result instance based on the json of the file that was returned by google from the api search
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static GoogleDriveFileResult fromJson(JToken driveFileJson)
        {
            var result = new GoogleDriveFileResult();
            result.MimeType = driveFileJson["mimeType"]?.ToString();
            result.CreatedDate = driveFileJson["createdDate"]?.ToString();
            result.ModifiedDate = driveFileJson["modifiedDate"]?.ToString();
            result.SelfLink = driveFileJson["selfLink"]?.ToString();
            result.AlternateLink = driveFileJson["alternateLink"]?.ToString();
            result.IconLink = driveFileJson["iconLink"]?.ToString();
            result.Title = driveFileJson["title"]?.ToString();
            result.OwnerNames = new List<String>();
            result.id = driveFileJson["id"]?.ToString();
            foreach(var f in driveFileJson["ownerNames"])
            {
                result.OwnerNames.Add(f.ToString());
            }
            result.ThumbnailLink = driveFileJson["thumbnailLink"]?.ToString();
            return result;
        }

    }
}
