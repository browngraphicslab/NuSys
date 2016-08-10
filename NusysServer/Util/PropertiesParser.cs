using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NusysIntermediate;

namespace NusysServer
{
    public class PropertiesParser
    {
        /// <summary>
        /// Concatenates all properties of the same alias or library element to the same message. 
        /// </summary>
        /// <param name="oldMessages"></param>
        /// <returns></returns>
        public IEnumerable<Message> ConcatMessageProperties(IEnumerable<Message> oldMessages)
        {
            var newMessages = new Dictionary<string, Message>();
            foreach (var message in oldMessages)
            {
                var propertyValueKey = GetPropertyValueKey(message);
                string idToGroupBy = GetIDKey(message);
                if (message.ContainsKey(idToGroupBy) &&
                    !newMessages.ContainsKey(message.GetString(idToGroupBy)))
                {
                    if (propertyValueKey != null)
                    {
                        message.Add(message.GetString(Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_KEY_COLUMN_KEY).First()), message.GetString(propertyValueKey));
                        message.Remove(Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_KEY_COLUMN_KEY).First());
                        message.Remove(Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY).First());
                        message.Remove(Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_DATE_VALUE_COLUMN_KEY).First());
                        message.Remove(Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_NUMERICAL_VALUE_COLUMN_KEY).First());
                    }
                    newMessages[message.GetString(idToGroupBy)] = message;
                }
                else if (newMessages.ContainsKey(message.GetString(idToGroupBy)))
                {
                    if (propertyValueKey != null)
                    {
                        newMessages[message.GetString(idToGroupBy)].Add(message.GetString(Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_KEY_COLUMN_KEY).First()), message.GetString(propertyValueKey));
                    }
                }
            }
            return newMessages.Values;
        }

        /// <summary>
        /// Given a message, looks whether the properties belongs to a library element or an alias and returns the proper id key.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string GetIDKey(Message message)
        {
            var idToGroupBy = "";
            var aliasIDKey = Constants.GetFullColumnTitle(Constants.SQLTableType.Alias, NusysConstants.ALIAS_ID_KEY).First();
            var libraryIDKey = Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY).First();
            if (message.ContainsKey(aliasIDKey) &&
                message.GetString(aliasIDKey) == message.GetString(Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY).First()))
            {
                idToGroupBy = aliasIDKey;
            }
            else if (message.ContainsKey(libraryIDKey) &&
                     message.GetString(libraryIDKey) == message.GetString(Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY).First()))
            {
                idToGroupBy = libraryIDKey;
            }
            else if (!message.ContainsKey(Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY).First()) || message.GetString(Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY).First()) == "")
            {
                if (message.ContainsKey(aliasIDKey))
                {
                    idToGroupBy = aliasIDKey;
                }
                else
                {
                    Debug.Assert(message.ContainsKey(libraryIDKey));
                    idToGroupBy = libraryIDKey;
                }
            }
            return idToGroupBy;
        }

        /// <summary>
        /// Returns the full title column key (depending on string, int, or date) for the value of the property. Returns null if no value.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string GetPropertyValueKey(Message message)
        {
            if (message.GetString(Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_DATE_VALUE_COLUMN_KEY).First()) != "")
            {
                return Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_DATE_VALUE_COLUMN_KEY).First();
            }
            else if (message.GetString(Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_NUMERICAL_VALUE_COLUMN_KEY).First()) != "")
            {
                return Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_NUMERICAL_VALUE_COLUMN_KEY).First();

            }
            else if (message.GetString(Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY).First()) != "")
            {
                return Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY).First();
            }
            return null;
        }
    }
}