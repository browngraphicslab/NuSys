using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface IMetadatable
    {
        Dictionary<string, Tuple<string, bool>> GetMetadata();
        bool AddMetadata(MetadataEntry entry);
        bool RemoveMetadata(string key);
        string GetMetadata(string key);
        MetadatableType MetadatableType();
    }
}
