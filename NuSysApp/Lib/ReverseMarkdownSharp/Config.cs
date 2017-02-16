using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReverseMarkdown
{
	public class Config
	{
		private string _unknownTagsConverter = "pass_through";
		private bool _githubFlavored;
		
		public Config() 
		{
		}

		public Config(string unknownTagsConverter="pass_through", bool githubFlavored=false)
		{
			_unknownTagsConverter = unknownTagsConverter;
			_githubFlavored = githubFlavored;
		}

		public string UnknownTagsConverter
		{
			get { return _unknownTagsConverter; }
		}

		public bool GithubFlavored
		{
			get { return _githubFlavored; }
		}
	}
}
