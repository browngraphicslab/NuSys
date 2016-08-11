

using System.Collections.Generic;

namespace NusysIntermediate
{
    /// <summary>
    /// Wraps a metadata entry, which is defined by a key, value, 
    /// and mutability. If you can edit the key or value, then the entr is mutable. 
    /// </summary>
    public class MetadataEntry
    {
        public string Key { get; set; }
        private List<string> _values;
        public List<string> Values {
            get { return _values; }
            set
            {
                _values = value; 
                
            } }       
        public MetadataMutability Mutability { get; set; }
        //public Windows.UI.Xaml.Media.SolidColorBrush Brush { get; set; }

        /// <summary>
        /// Creates a meta data entry with a key, value, mutability, and brush (based on mutability)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="mutability"></param>
        public MetadataEntry(string key, List<string> values, MetadataMutability mutability)
        {
            Key = key;
            Values = values;
            Mutability = mutability;
            /*
            Mutability = MetadataMutability.IMMUTABLE;
            if (mutability)
            {
                Mutability = MetadataMutability.MUTABLE; 
            }*/



        }
    }

    /// <summary>
    /// Enum to describe mutability of a metadata entry
    /// </summary>
    public enum MetadataMutability
    {
        MUTABLE,IMMUTABLE
    }

}
