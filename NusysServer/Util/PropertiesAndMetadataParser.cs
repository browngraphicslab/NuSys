using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer.Util
{
    public class PropertiesAndMetadataParser
    {
        /// <summary>
        /// Returns the message with all properties, and metadata that belong to a single element in a single message. 
        /// </summary>
        /// <param name="oldMessages"></param>
        /// <returns></returns>
        public IEnumerable<Message> ConcatPropertiesAndMetadata(List<Message> oldMessages)
        {
            Dictionary<string, Tuple<Dictionary<string, MetadataEntry>, HashSet<KeyValuePair<string, string>>>>
                idToMetadataAndProperties = new Dictionary<string, Tuple<Dictionary<string, MetadataEntry>, HashSet<KeyValuePair<string, string>>>>();

            foreach (var message in new List<Message>(oldMessages))
            {
                var propertyValueKey = GetPropertyValueKey(message);
                var libraryId = message.GetString(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY);
                if (!idToMetadataAndProperties.ContainsKey(libraryId))
                {
                    idToMetadataAndProperties[libraryId] =
                        new Tuple<Dictionary<string, MetadataEntry>, HashSet<KeyValuePair<string, string>>>(
                            new Dictionary<string, MetadataEntry>(), new HashSet<KeyValuePair<string, string>>());
                }
                else
                {
                    oldMessages.Remove(message);
                }
                if (!message.GetString(NusysConstants.PROPERTIES_KEY_COLUMN_KEY, "").Equals("") && propertyValueKey != null)
                {
                    idToMetadataAndProperties[libraryId].Item2.Add(new KeyValuePair<string, string>(message.GetString(NusysConstants.PROPERTIES_KEY_COLUMN_KEY), message.GetString(propertyValueKey)));
                }

                if (!message.GetString(NusysConstants.METADATA_KEY_COLUMN_KEY, "").Equals(""))
                {
                    var x = message.GetString(NusysConstants.METADATA_MUTABILITY_COLUMN_KEY);
                    MetadataEntry entry = new MetadataEntry(message.GetString(NusysConstants.METADATA_KEY_COLUMN_KEY), JsonConvert.DeserializeObject<List<string>>(message.GetString(NusysConstants.METADATA_VALUE_COLUMN_KEY)), (MetadataMutability)Enum.Parse(typeof(MetadataMutability), message.GetString(NusysConstants.METADATA_MUTABILITY_COLUMN_KEY)));
                    if (!idToMetadataAndProperties[libraryId].Item1.ContainsKey(entry.Key))
                    {
                        idToMetadataAndProperties[libraryId].Item1.Add(entry.Key, entry);
                    }
                }
            }
            foreach (var uniqueMessage in oldMessages)
            {
                foreach (var property in idToMetadataAndProperties[uniqueMessage.GetString(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY)].Item2)
                {
                    uniqueMessage[property.Key] = property.Value;
                }
                uniqueMessage[NusysConstants.LIBRARY_ELEMENT_METADATA_KEY] =
                    JsonConvert.SerializeObject(
                        idToMetadataAndProperties[uniqueMessage.GetString(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY)
                            ].Item1);
            }
            return oldMessages;
        }

        /// <summary>
        /// Returns the full title column key (depending on string, int, or date) for the value of the property. Returns null if no value.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string GetPropertyValueKey(Message message)
        {
            if (message.GetString(NusysConstants.PROPERTIES_DATE_VALUE_COLUMN_KEY) != "")
            {
                return NusysConstants.PROPERTIES_DATE_VALUE_COLUMN_KEY;
            }
            else if (message.GetString(NusysConstants.PROPERTIES_NUMERICAL_VALUE_COLUMN_KEY) != "")
            {
                return NusysConstants.PROPERTIES_NUMERICAL_VALUE_COLUMN_KEY;

            }
            else if (message.GetString(NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY) != "")
            {
                return NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY;
            }
            return null;
        }

    }
}