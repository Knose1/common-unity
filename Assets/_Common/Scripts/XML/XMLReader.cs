using System;
using System.Text.RegularExpressions;

namespace Com.GitHub.Knose1.Common.XML
{
	/// <summary>
	/// Imitate the <see cref="System.Xml.XmlReader"/>'s actions but without the errors.<br/>
	/// See also : <seealso cref="XMLHierarchyComputer"/>
	/// </summary>
	public class XMLReader
	{
		/// <summary>
		/// The text to read
		/// </summary>
		public string text;

		/// <param name="text">The text to read</param>
		public XMLReader(string text) => this.text = text ?? throw new ArgumentNullException(nameof(text));

		/// <summary>
		/// A regex that targets the tags
		/// </summary>
		public readonly static Regex tagFind = new Regex("<(\\w+).*?>|<\\/(\\w+)>");
		/// <summary>
		/// A regex that targets the args (must be used inside a tag)
		/// </summary>
		public readonly static Regex argsFind = new Regex("(?:(\\w+)=\"(.*?)\")");

		private MatchCollection tagMatches;
		private int tagMatchesCount;
		private int tagIndex = 0;

		private MatchCollection argsMatches;
		private int argsIndex = 0;

		private Match CurrentTagMatch => tagIndex >= tagMatchesCount ? null : tagMatches[tagIndex];
		private Match CurrentArgsMatch => argsIndex >= ArgCount ? null : argsMatches[argsIndex];
		
		/// <summary>
		/// The current char index of the tag/arg
		/// </summary>
		public int Index { get; private set; }
		/// <summary>
		/// The name of the tag/arg
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// The value of the arg is aplyable
		/// </summary>
		public string Value { get; private set; }
		/// <summary>
		/// The raw regex match value
		/// </summary>
		public string Raw { get; private set; }
		/// <summary>
		/// True if it's an oppening tag
		/// </summary>
		public bool IsStartElement { get; private set; }

		/// <summary>
		/// True if we're actually reading an argument
		/// </summary>
		public bool IsArg { get; private set; }

		/// <summary>
		/// Get the argument count
		/// </summary>
		public int ArgCount { get; private set; }

		/// <summary>
		/// True if it's the tag's end characted
		/// </summary>
		private bool isTagEndChar = false;

		/// <summary>
		/// Reset the readed
		/// </summary>
		public void Reset()
		{
			tagMatchesCount = tagIndex = 0;
			tagMatches = null;

			ArgCount = argsIndex = 0;
			argsMatches = null;
		}

		/// <summary>
		/// Go to next tag position.<br/>
		/// The positions are the following :<br/>
		/// - Tag, start position
		/// - Tag, end position
		/// </summary>
		/// <returns></returns>
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

			if (IsArg && argsIndex < ArgCount)
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

				Raw = "";
				Name = "";
				Value = "";
				ArgCount = 0;

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


		/// <summary>
		/// Go to attribute <paramref name="i"/>.<br/>
		/// The <see cref="Next"/> method will switch to attribute mode.<br/>
		/// When calling <see cref="Next"/> the method will compute ++<paramref name="i"/>.<br/>
		/// if there are no more attribute to compute, the Next function will switch back to tag mode.
		/// </summary>
		/// <returns></returns>
		public void MoveToAttribute(int i)
		{
			if (i < 0 || i >= ArgCount)
				throw new ArgumentOutOfRangeException(nameof(i), i, "");

			argsIndex = i;

			HandleAttribute();
		}

		private void HandleTag()
		{
			IsArg = false;
			isTagEndChar = true;

			Raw = CurrentTagMatch.Groups[0].Value;
			Name = CurrentTagMatch.Groups[1].Value;
			IsStartElement = Name != "";
			if (Name == "") Name = CurrentTagMatch.Groups[2].Value;
			Value = "";


			Index = CurrentTagMatch.Index;

			argsMatches = argsFind.Matches(CurrentTagMatch.Value);
			ArgCount = argsMatches.Count;
			argsIndex = 0;

		}

		private void HandleAttribute()
		{
			Raw = CurrentTagMatch.Groups[0].Value;
			Name = CurrentArgsMatch.Groups[1].Value;
			Value = CurrentArgsMatch.Groups[2].Value;
			Index = CurrentTagMatch.Index + CurrentArgsMatch.Index;

			IsStartElement = false;

			++argsIndex;
		}
	}
}
