using Com.GitHub.Knose1.Common.XML;
using Com.GitHub.Knose1.JuicyText.Attributes;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Com.GitHub.Knose1.JuicyText.Effects
{
	[TextTag(TextTagUsage.LetterAdding, TAG_NAME)]
	internal sealed class PauseEffect
	{
		private const string TAG_NAME = "pause";

		public static IEnumerator LetterAdded(char c, int index, XMLTag tag, TextEffect text)
		{
			text.doDefaultPause = false;

			var attribute = tag.attributes.First( (a) => a == TAG_NAME );
			float time = text.typingInterval * float.Parse(attribute.value);

			yield return new WaitForSeconds(time);
		}
	}
}
