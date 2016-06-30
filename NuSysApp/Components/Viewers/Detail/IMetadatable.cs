using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// Whoever added this, please explain what this is
    /// </summary>
    public interface IMetadatable
    {
        Dictionary<string, MetadataEntry> GetMetadata();
        bool AddMetadata(MetadataEntry entry);
        bool RemoveMetadata(string key);
        List<string> GetMetadata(string key);
        MetadatableType MetadatableType();
    }
}
