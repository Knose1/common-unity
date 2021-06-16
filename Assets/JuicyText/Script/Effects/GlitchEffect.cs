using Com.GitHub.Knose1.Common.XML;
using Com.GitHub.Knose1.JuicyText.Attributes;
using System.Text;
using UnityEngine;

namespace Com.GitHub.Knose1.JuicyText.Effects
{
	[TextTag(TextTagUsage.Runtime, "glitch")]
	internal sealed class GlitchEffect
	{
		private static readonly string glitchs = "abcdefghijklmnopqrstuvwyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789*!?;,&~_°$#%[](){}='\"+-/*\\.";

		private static float time;
		public static MeshQuad Update(int quadIndex, MeshQuad quad, XMLTag tag, TextEffect text)
		{
			if (time > 0)
			{
				if (text.UpdateTime)
					time -= Time.deltaTime;
				return quad;
			}

			char charToSet = glitchs[Random.Range(0, glitchs.Length)];

			StringBuilder stringBuilder = new StringBuilder(text.textToShow);

			stringBuilder[text.ConvertQuadIndex(quadIndex).textToShowIndex] = charToSet;

			text.textToShow = stringBuilder.ToString();

			time = 0.5f;
			return quad;
		}
	}
}
