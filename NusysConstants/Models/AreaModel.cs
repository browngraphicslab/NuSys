using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using NusysIntermediate;

namespace NuSysApp
{
    public class AreaModel : CollectionElementModel
    {
        public List<PointModel> Points { get; set; } 

        public AreaModel(string id) : base(id)
        {
            Points = new List<PointModel>();
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var packed = await base.Pack();
            packed["points"] = Points;
            return packed;
        }

        public override async Task UnPackFromDatabaseMessage(Message props)
        {
<<<<<<< HEAD:NusysConstants/Models/AreaModel.cs
            await base.UnPack(props);
            Points = props.GetList("points", new List<PointModel>());
=======
            await base.UnPackFromDatabaseMessage(props);
            Points = props.GetList("points", new List<Point>());
>>>>>>> 84c15e9142f4749f40a52554665bfc08b7c0d715:NuSysApp/Models/AreaModel.cs
        }
    }
}
