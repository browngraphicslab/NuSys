using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    public class SQLUpdatePropertiesArgs
    {
        /// <summary>
        /// Library or alias id of the property to add or update
        /// </summary>
        public string LibraryOrAliasId;

        /// <summary>
        /// Key of the property to insert or update
        /// </summary>
        public string PropertyKey;

        private string _propertyValue;
        /// <summary>
        /// value of the property to insert or update
        /// </summary>
        public string PropertyValue { get { return _propertyValue; } set { _propertyValue = NusysConstants.CheckString(value); } }
    }
}