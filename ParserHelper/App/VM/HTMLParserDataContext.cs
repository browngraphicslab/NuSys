﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParserHelper
{
    public class HTMLParserDataContext
    {
        private string _url;
        public IEnumerable<DataHolder> DataObjects;

        public HTMLParserDataContext(string url)
        {
            this._url = url;
        }
        public async Task<IEnumerable<DataHolder>> loadResults()
        {
            var importer = new HtmlImporter();
            DataObjects = await importer.Run((_url==null || _url=="")?null:new System.Uri(_url));
            return DataObjects;
        }
    }
}