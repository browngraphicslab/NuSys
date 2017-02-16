﻿using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace ReverseMarkdown.Converters
{
	public class Li: ConverterBase
	{
		public Li(Converter converter)
			: base(converter)
		{
			Converter.Register("li", this);
		}

		public override string Convert(HtmlNode node)
		{
			string content = TreatChildren(node);
			string indentation = IndentationFor(node);
			string prefix = PrefixFor(node);
	
			return string.Format("{0}{1}{2}" + Environment.NewLine, indentation, prefix, content.Chomp());
		}

		private string PrefixFor(HtmlNode node)
		{
		    if (node.ParentNode != null && node.ParentNode.Name == "ol")
			{
                // index are zero based hence add one
                int index = node.ParentNode.Descendants("./li").ToList().IndexOf(node) + 1;
				return string.Format("{0}. ",index);
			}
		    return "- ";
		}

	    private string IndentationFor(HtmlNode node)
		{
			int length = node.Ancestors("ol").Count() + node.Ancestors("ul").Count();
			return new string(' ', Math.Max(length-1,0));
		}
	}
}
