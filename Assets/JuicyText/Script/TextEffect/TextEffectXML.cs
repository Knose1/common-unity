using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Com.GitHub.Knose1.Common.XML;
using Com.GitHub.Knose1.Common.Utils;
using System;

namespace Com.GitHub.Knose1.JuicyText
{
	public partial class TextEffect : Text
	{
		private List<List<XMLTag>> xmlBaliseHierarchyByIndex = new List<List<XMLTag>>();
		private List<XMLTag> xmlBaliseHierarchy = new List<XMLTag>();

		private IEnumerator TextCoroutine()
		{
			textToShow = "";
			int baliseStartIndex = -1;

			xmlBaliseHierarchyByIndex.Clear();
			XMLHierarchyComputer xmlComputer = new XMLHierarchyComputer(text);

			if (string.IsNullOrWhiteSpace(text))
				yield break;

			xmlComputer.Compute();

			int textLength = text.Length;
			for (int i = 0; i < textLength; i++)
			{
				this.xmlBaliseHierarchy.Clear();

				//Dispatch OnBeforeLetterGenerate
				{
					var subRoutine = OnBeforeLetterGenerate(i);
					if (subRoutine != null)
						yield return subRoutine;
				}

				int previousI = i;

				//If there are temporary end tag, remove them
				if (baliseStartIndex != -1)
				{
					textToShow = textToShow.Remove(baliseStartIndex);
				}

				int iBefore = i;
				char myChar = GetCurrentChar(textLength, ref i);
				//bool hasChanged = i != iBefore;

				bool newTag = false;
				XMLTag currentTag = XMLTag.GetTag(i, xmlComputer.TagList, out bool isEndTag);

				do
				{
					
					newTag = false;
					bool wasCharOverWriten = currentTag != null;
					if (wasCharOverWriten)
					{
						newTag = true;
						i -= 1;
						textToShow = textToShow.Remove(textToShow.Length - 1);
					}

					while (currentTag != null)
					{
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

					if (wasCharOverWriten)
					{
						i++;
						if (i == textLength)
						{
							i -= 1;
							break;
						}

						myChar = GetCurrentChar(textLength, ref i);

						currentTag = XMLTag.GetTag(i, xmlComputer.TagList, out isEndTag);

					}

				} while (currentTag != null);

				this.xmlBaliseHierarchy = XMLTag.GetHierarchy(i, xmlComputer.XMLHierarchy);
				var xmlBaliseHierarchy = XMLTag.GetHierarchy(newTag ? i + 1 : i, xmlComputer.XMLHierarchy);
				baliseStartIndex = -1;
				for (int j = xmlBaliseHierarchy.Count - 1; j >= 0; j--)
				{
					XMLTag tag = xmlBaliseHierarchy[j];

					if (tag.isUnityDefault)
					{
						if (baliseStartIndex == -1)
						{
							baliseStartIndex = textToShow.Length;
						}

						textToShow += tag.endTagString;
					}
				}
				
				xmlBaliseHierarchyByIndex.Add(new List<XMLTag>(this.xmlBaliseHierarchy));

				//Dispatch OnLetterGenerate
				{
					var subRoutine = OnLetterGenerate(myChar, i);
					if (subRoutine != null)
						yield return subRoutine;
				}
				SetVerticesDirty();
			}

			textCoroutine = null;
		}

		private char GetCurrentChar(int textLength, ref int index)
		{
			char myChar = default;
			index--;
			do
			{
				index++;
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
