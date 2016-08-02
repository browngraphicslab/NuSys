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
            await base.UnPackFromDatabaseMessage(props);
            if (props.ContainsKey(NusysConstants.AREA_MODEL_POINTS_KEY))
            {
                Points = props.GetList(NusysConstants.AREA_MODEL_POINTS_KEY, new List<PointModel>());

            }
        }
    }
}
