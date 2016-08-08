using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

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
        bool UpdateMetadata(MetadataEntry original, string key, List<string> values );
        List<string> GetMetadata(string key);
        MetadatableType MetadatableType();
        string Title { get; set; }
    }
}
