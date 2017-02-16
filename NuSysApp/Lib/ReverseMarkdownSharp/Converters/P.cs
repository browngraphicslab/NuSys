using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace ReverseMarkdown.Converters
{
	public class P: ConverterBase
	{
		public P(Converter converter):base(converter)
		{
			Converter.Register("p", this);
		}

		public override string Convert(HtmlNode node)
		{
			return Environment.NewLine + Environment.NewLine + TreatChildren(node).Trim() + Environment.NewLine + Environment.NewLine;
		}
	}
}
