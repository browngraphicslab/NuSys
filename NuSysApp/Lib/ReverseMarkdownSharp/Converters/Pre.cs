﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ReverseMarkdown.Converters
{
	public class Pre : ConverterBase
	{
		public Pre(Converter converter)
			: base(converter)
		{
			Converter.Register("pre", this);
		}

		public override string Convert(HtmlNode node)
		{
			if (Converter.Config.GithubFlavored)
			{
				return Environment.NewLine + string.Format("```{0}", GetLanguage(node)) + Environment.NewLine
					+ node.InnerText.Trim() + Environment.NewLine
					+ "```" + Environment.NewLine;
			}
		    // get the lines based on carriage return and prefix four spaces to each line
		    var lines = node.InnerText.ReadLines().Select(item => "    " + item + Environment.NewLine);

		    // join all the lines to a single line
		    var result = lines.Aggregate((curr, next) => curr + next);

		    return Environment.NewLine + Environment.NewLine + result + Environment.NewLine;
		}


		private string GetLanguage(HtmlNode node)
		{
			string lang = GetLanguageFromHighlightClassAttribute(node);
			return lang !=string.Empty ? lang : GetLanguageFromConfluenceClassAttribute(node); 
		}

		private string GetLanguageFromHighlightClassAttribute(HtmlNode node)
		{
			string val = node.GetAttributeValue("class","");
			var rx = new System.Text.RegularExpressions.Regex("highlight-([a-zA-Z0-9]+)");
			var res = rx.Match(val);
			return res.Success ? res.Value : "";
		}

		private string GetLanguageFromConfluenceClassAttribute(HtmlNode node)
		{
			string val = node.GetAttributeValue("class", "");
			var rx = new System.Text.RegularExpressions.Regex(@"brush:\s?(:?.*);");
			var res = rx.Match(val);
			return res.Success ? res.Value : "";
		}
	}
}
