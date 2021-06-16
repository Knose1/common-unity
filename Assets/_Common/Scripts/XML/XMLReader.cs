using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Com.GitHub.Knose1.Common.XML
{
	public class XMLReader
	{
		public string text;

		public XMLReader(string text) => this.text = text ?? throw new ArgumentNullException(nameof(text));

		private readonly static Regex tagFind = new Regex("<(\\w+).*?>|<\\/(\\w+)>");
		private readonly static Regex argsFind = new Regex("(?:(\\w+)=\"(.*?)\")");

		private MatchCollection tagMatches;
		private int tagMatchesCount;
		private int tagIndex = 0;

		private MatchCollection argsMatches;
		private int argsMatchesCount;
		private int argsIndex = 0;

		private Match CurrentTagMatch => tagIndex >= tagMatchesCount ? null : tagMatches[tagIndex];
		private Match CurrentArgsMatch => argsIndex >= argsMatchesCount ? null : argsMatches[argsIndex];
		
		public int Index { get; private set; }
		public string Name { get; private set; }
		public string Value { get; private set; }
		public string Balise { get; private set; }
		public bool IsStartElement { get; private set; }

		public bool IsArg { get; private set; }

		public int ArgCount => argsMatchesCount;
		private bool isTagEndChar = false;

		public void Reset()
		{
			tagMatchesCount = tagIndex = 0;
			tagMatches = null;

			argsMatchesCount = argsIndex = 0;
			argsMatches = null;
		}

		public bool Next()
		{
			
			if (tagMatches is null)
			{
				//INIT THE MATCHES
				tagMatches = tagFind.Matches(text);
				tagMatchesCount = tagMatches.Count;

				//Handle first match
				if (tagMatchesCount != 0) HandleTag();

				//Return true
				return tagMatchesCount != 0;
			}
			
			if (tagIndex >= tagMatchesCount)
				return false;

			if (IsArg && argsIndex < argsMatchesCount)
			{
				//Handle Args
				MoveToAttribute(argsIndex);
				return true;
			}
			else if (IsArg)
			{
				//Args ends
				argsIndex = 0;
				IsArg = false;
				isTagEndChar = true;
			}

			if (isTagEndChar)
			{
				//Handle end char (the > in <yo> for example)
				IsArg = false;

				Balise = "";
				Name = "";
				Value = "";
				argsMatchesCount = 0;

				IsStartElement = false;
				isTagEndChar = false;

				Index = CurrentTagMatch.Index + CurrentTagMatch.Length - 1;

				++tagIndex;
			}
			else
			{
				HandleTag();
				return true;
			}

			return tagIndex < tagMatchesCount;
		}

		public void MoveToAttribute(int i)
		{
			if (i < 0 || i >= argsMatchesCount)
				throw new ArgumentOutOfRangeException(nameof(i), i, "");

			argsIndex = i;

			HandleAttribute();
		}

		private void HandleTag()
		{
			IsArg = false;
			isTagEndChar = true;

			Balise = CurrentTagMatch.Groups[0].Value;
			Name = CurrentTagMatch.Groups[1].Value;
			IsStartElement = Name != "";
			if (Name == "") Name = CurrentTagMatch.Groups[2].Value;
			Value = "";


			Index = CurrentTagMatch.Index;

			argsMatches = argsFind.Matches(CurrentTagMatch.Value);
			argsMatchesCount = argsMatches.Count;
			argsIndex = 0;

		}

		private void HandleAttribute()
		{
			Balise = CurrentTagMatch.Groups[0].Value;
			Name = CurrentArgsMatch.Groups[1].Value;
			Value = CurrentArgsMatch.Groups[2].Value;
			Index = CurrentTagMatch.Index + CurrentArgsMatch.Index;

			IsStartElement = false;

			++argsIndex;
		}
	}
}
