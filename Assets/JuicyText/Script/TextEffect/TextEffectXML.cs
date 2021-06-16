//-///////////////////////////////////////////////////////////-//
//                                                             //
// This script handles the XML and the TextCoroutine. It also  //
// expose util methods to navigate in the text.                //
//                                                             //
//-///////////////////////////////////////////////////////////-//

using Com.GitHub.Knose1.Common.XML;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Com.GitHub.Knose1.JuicyText
{
	public struct TextEffectIndexConversion
	{
		public readonly int quadIndex;
		public readonly int untaggedIndex;
		public readonly int stringIndex;

		public TextEffectIndexConversion(int quadIndex, int untaggedIndex, int stringIndex)
		{
			this.quadIndex = quadIndex;
			this.untaggedIndex = untaggedIndex;
			this.stringIndex = stringIndex;
		}
	}

	public partial class TextEffect : Text
	{
		/// <summary>
		/// The hierarchy of tags by quad index.<br/>
		/// </summary>
		private List<List<XMLTag>> xmlTagHierarchyByIndex = new List<List<XMLTag>>();

		/// <summary>
		/// The current hierarchy of tags in <see cref="TextCoroutine"/>.<br/>
		/// The higher the index, the higher the depth.
		/// </summary>
		private List<XMLTag> xmlTagHierarchy = new List<XMLTag>();

		/// <summary>
		/// The text without the tags
		/// </summary>
		public string UntaggedText => _untaggedText;
		private string _untaggedText;

		/// <summary>
		/// The conversion table of indexes
		/// </summary>
		private List<TextEffectIndexConversion> indexConversionTable = new List<TextEffectIndexConversion>();

		/// <summary>
		/// Get the conversion from a quad index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public TextEffectIndexConversion ConvertQuadIndex(int index)
		{
			return indexConversionTable[index];
		}

		/// <summary>
		/// Get the conversion from a <see cref="UntaggedText"/> index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public TextEffectIndexConversion ConvertUntaggedIndex(int index)
		{
			return indexConversionTable.Find((c) => c.untaggedIndex == index);
		}

		/// <summary>
		/// Get the conversion from a <see cref="Text.text"/> index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public TextEffectIndexConversion ConvertStringIndex(int index)
		{
			return indexConversionTable.Find((c) => c.stringIndex == index);
		}

		/// <summary>
		/// This is the typing coroutine.<br/>
		/// It adds the letters one by one and ensure that all the unity balise are closed.<br/>
		/// It also call the events <see cref="OnBeforeStartLetterGeneration"/> and <see cref="OnLetterGenerate"/>
		/// </summary>
		/// <returns></returns>
		private IEnumerator TextCoroutine()
		{
			XMLHierarchyComputer xmlComputer = new XMLHierarchyComputer(text);
			indexConversionTable.Clear();
			xmlTagHierarchyByIndex.Clear();
			textToShow = "";

			_untaggedText = xmlComputer.UnTagedText;
			int nonTagIndex = 0;

			//Index where to remove the temporary end tags
			int tempTagStartIndex = -1;

			//If the string is empty, return;
			if (string.IsNullOrWhiteSpace(text))
				yield break;

			//Compute the xml
			xmlComputer.Compute();

			//Dispatch OnBeforeLetterGenerate
			{
				var subRoutine = OnBeforeStartLetterGeneration();
				if (subRoutine != null)
					yield return subRoutine;
			}

			//Iterate on each characters
			int quadsCount = 0;
			int textLength = text.Length;
			for (int i = 0; i < textLength; i++)
			{
				this.xmlTagHierarchy.Clear();

				//If there are temporary end tag, remove them
				if (tempTagStartIndex != -1)
				{
					textToShow = textToShow.Remove(tempTagStartIndex);
				}

				//Get the current non space char
				char myChar = GetCurrentChar(textLength, ref i, ref nonTagIndex);

				//Check if next nonspace char is a tag
				XMLTag currentTag = XMLTag.GetTag(i, xmlComputer.TagList, out bool isEndTag);

				bool newTag; //True when current character is the character just after the tag
				do
				{
					newTag = false;

					bool wasCharOverWriten = currentTag != null;
					if (wasCharOverWriten)
					{
						//The current char is a tag's char
						newTag = true;

						//Remove the letter from the text to show
						--nonTagIndex;
						--i;
						textToShow = textToShow.Remove(textToShow.Length - 1);
					}

					//Iterate until we can't find anymore tag
					while (currentTag != null)
					{
						//if it's the </tagName> tag or not
						if (isEndTag)
						{
							i = currentTag.tagEnd.end;
							if (currentTag.isUnityDefault)
							{
								textToShow += currentTag.endTagString;
							}
						}
						else
						{
							i = currentTag.tagStart.end;
							if (currentTag.isUnityDefault)
							{
								textToShow += currentTag.startTagString;
							}
						}

						currentTag = XMLTag.GetTag(i + 1, xmlComputer.TagList, out isEndTag);
					}

					//If the first char was a tag char, it means that no quad will be added.
					//This cause a double pause bug. To avoid this, we're going to search for a new non whitespace character.
					if (wasCharOverWriten)
					{
						//Let's get onto the next char
						i++;
						nonTagIndex++;


						//Avoid text limit breaking
						if (i == textLength)
						{
							i -= 1;
							break;
						}

						myChar = GetCurrentChar(textLength, ref i, ref nonTagIndex);

						currentTag = XMLTag.GetTag(i, xmlComputer.TagList, out isEndTag);

					}

				} while (currentTag != null); //If unfortunately, the next non whitespace character is a tag, we have to restart all the process

				//Update the Hierarchy
				this.xmlTagHierarchy = XMLTag.GetHierarchy(i, xmlComputer.XMLHierarchy);

				//Get the hierarchy to compute temporary end tags
				var xmlBaliseHierarchy = XMLTag.GetHierarchy(newTag ? i + 1 : i, xmlComputer.XMLHierarchy);
				tempTagStartIndex = -1;
				for (int j = xmlBaliseHierarchy.Count - 1; j >= 0; j--)
				{
					XMLTag tag = xmlBaliseHierarchy[j];

					if (tag.isUnityDefault)
					{
						if (tempTagStartIndex == -1)
						{
							tempTagStartIndex = textToShow.Length;
						}

						textToShow += tag.endTagString;
					}
				}

				//Add a hierarchy at quad index
				xmlTagHierarchyByIndex.Add(new List<XMLTag>(this.xmlTagHierarchy));

				//Add a conversion
				indexConversionTable.Add(new TextEffectIndexConversion(quadsCount, nonTagIndex, i));

				//Dispatch OnLetterGenerate
				{
					var subRoutine = OnLetterGenerate(myChar, quadsCount);
					if (subRoutine != null)
						yield return subRoutine;
				}
				SetVerticesDirty();

				//Prepare index for next for loop
				nonTagIndex += 1;
				quadsCount += 1;
			}

			textCoroutine = null;
		}

		/// <summary>
		/// Find the next non whitespace character.<br/>
		/// While itarating, it increase the index and the nonTagIndex.
		/// </summary>
		/// <param name="textLength"></param>
		/// <param name="index"></param>
		/// <param name="nonTagIndex"></param>
		/// <returns>Current char</returns>
		private char GetCurrentChar(int textLength, ref int index, ref int nonTagIndex)
		{
			char myChar = default;
			index--;
			nonTagIndex--;
			do
			{
				index++;
				nonTagIndex++;
				if (index == textLength)
				{
					break;
				}
				myChar = text[index];
				textToShow += myChar;
			}
			while (char.IsWhiteSpace(myChar));

			return myChar;
		}

	}
}
