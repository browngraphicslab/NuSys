﻿using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace ReverseMarkdown.Converters
{
	public class Br: ConverterBase
	{
		public Br(Converter converter)
			: base(converter)
		{
			this.Converter.Register("br", this);
		}

		public override string Convert(HtmlNode node)
		{
			return "  " + Environment.NewLine;
		}
	}
}
