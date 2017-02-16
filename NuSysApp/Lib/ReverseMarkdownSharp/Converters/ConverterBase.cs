using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace ReverseMarkdown.Converters
{
    public abstract class ConverterBase : IConverter
    {
		private Converter _converter;

		public ConverterBase(Converter converter) 
		{
			_converter = converter;
		}

		protected Converter Converter 
		{
			get 
			{
				return _converter;
			}
		}

		public string TreatChildren(HtmlNode node)
		{
			string result = string.Empty;

			if (node.HasChildNodes)
			{
				foreach(HtmlNode nd in node.ChildNodes)
				{
					result+=Treat(nd);
				}
			}

			return result;
		}

		public string Treat(HtmlNode node){
			return Converter.Lookup(node.Name).Convert(node); 
		}

		public string ExtractTitle(HtmlNode node)
		{
			string title = node.GetAttributeValue("title", "");

			return title;
		}

		public abstract string Convert(HtmlNode node); 
    }
}
