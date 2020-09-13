using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Com.Github.Knose1.Common
{
	public enum SplitMode
	{
		Ignore,
		RemoveMatchAndSplit,
		EncapsulateMatch
	}

	public static class RegexHelper
	{
		public delegate SplitMode SplitFunction(Match match);
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="regex"></param>
		/// <param name="input"></param>
		/// <param name="matchComparer">If return true, confirm the splitting</param>
		/// <returns></returns>
		public static List<string> Split(this Regex regex, string input, SplitFunction matchComparer)
		{
			MatchCollection matches = regex.Matches(input);
			int matchCount = matches.Count;
			
			List<string> splitArray = new List<string>();

			int lastIndex = 0;
			for (int i = 0; i < matchCount; i++)
			{
				Match match = matches[i];

				SplitMode splitMode = matchComparer(match);
				string toAdd;
				switch (splitMode)
				{
					case SplitMode.Ignore:
						break;
					case SplitMode.RemoveMatchAndSplit:
						toAdd = input.Substring(lastIndex, match.Index - lastIndex);
						splitArray.Add(toAdd);
						lastIndex = match.Index + match.Length;
						break;
					case SplitMode.EncapsulateMatch:
						if (match.Index != lastIndex)
						{
							toAdd = input.Substring(lastIndex, match.Index - lastIndex);
							splitArray.Add(toAdd);
						}

						toAdd = input.Substring(match.Index, match.Length);
						splitArray.Add(toAdd);
						lastIndex = match.Index + match.Length;
						break;
				}
			}

			int inputLength = input.Length;
			if (inputLength != lastIndex)
			{
				string toAdd = input.Substring(lastIndex, inputLength - lastIndex);
				splitArray.Add(toAdd);
			}


			return splitArray;
		}
	}
}
