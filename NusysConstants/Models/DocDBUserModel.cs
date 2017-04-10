using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Used to define the model we use to store users in DocumentDB
    /// </summary>
    public class DocDBUserModel
    {
        public string UserId { get; set; }

        public string UserPassword { get; set; }

        public string UserSaltKey { get; set; }

        public string DisplayName { get; set; }

        public List<string> LastVisitedCollections { get; set; }

        public string DocType = NusysConstants.DocDB_DocumentType.User.ToString();

    }
}
