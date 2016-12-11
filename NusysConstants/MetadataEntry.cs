using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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

        /// <summary>
        /// Creates a meta data entry with a key, value, and mutability
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="mutability"></param>
        public MetadataEntry(string key, List<string> values, MetadataMutability mutability)
        {
            Key = key;
            Values = values;
            Mutability = mutability;
        }

        /// <summary>
        /// Method to get key value pair as a readable string
        /// Format: Key: Value1, Value2
        /// </summary>
        /// <returns></returns>
        public string GetMetadataAsString()
        {
            string dataString = "";
            dataString += (Key + ": ");
            if (Values.Count == 0)
            {
                dataString += "None";
                return dataString;
            }
            for (int i = 0; i < Values.Count; i++)
            {
                if (i < Values.Count - 1)
                {
                    dataString += (Values[i] + ", ");
                }
                else
                {
                    dataString += Values[i];
                }
            }
            
            return dataString;
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
