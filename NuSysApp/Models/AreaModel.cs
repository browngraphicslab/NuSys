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
        public List<Point> Points { get; set; } 

        public AreaModel(string id) : base(id)
        {
            Points = new List<Point>();
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var packed = await base.Pack();
            packed["points"] = Points;
            return packed;
        }

        public override async Task UnPack(Message props)
        {
            await base.UnPack(props);
            Points = props.GetList("points", new List<Point>());
        }
    }
}
