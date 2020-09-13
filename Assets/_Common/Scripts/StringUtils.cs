using System;
using System.Text.RegularExpressions;

namespace Com.GitHub.Knose1.Common
{
	public static class StringUtils
	{
		public readonly static Regex ESCAPE = new Regex("\\\\(.)");
		public readonly static Regex MATCH_STRING = new Regex("^\"(?:(?:\\\\\\\\)?|(?:\\\\\")?|[^\"]?)+\"$");

		public static bool TryParse(string input, out string output)
		{
			output = "";
			if (!IsRawString(input)) return false;

			output = input.Substring(1, input.Length - 2);
			output = ESCAPE.Replace(output, "$1");


			return true;
		}

		public static bool IsRawString(string value) => MATCH_STRING.IsMatch(value);
	}
}