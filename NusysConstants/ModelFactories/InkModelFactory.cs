using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class InkModelFactory
    {
        public static InkModel ParseFromDatabaseMessage(Message databaseMessage)
        {
            var model = new InkModel();
            model.UnPackFromDatabaseMessage(databaseMessage);
            return model;
        }
    }
}
