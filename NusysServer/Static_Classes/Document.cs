using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public class Document
    {
        public string Id { get; set; }
        public string Text { get; set; }

        public Document(string id, string text)
        {
            Id = id;
            Text = text;
        }

    }
}