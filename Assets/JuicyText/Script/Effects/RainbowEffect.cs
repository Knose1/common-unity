using Com.GitHub.Knose1.Common.XML;
using Com.GitHub.Knose1.JuicyText.Attributes;
using UnityEngine;

namespace Com.GitHub.Knose1.JuicyText.Effects
{
	[TextTag(TextTagUsage.Runtime, "rainbow")]
	internal sealed class RainbowEffect
	{
		public static MeshQuad Update(int index, MeshQuad quad, XMLTag tag, TextEffect text)
		{
			Color charColor = Color.HSVToRGB(text.currentQuadTime[index] % 1, 1, 1);
			
			quad.Translate(new Vector3(
				Mathf.Cos(text.currentQuadTime[index]) * 2.5f,
				Mathf.Sin(text.currentQuadTime[index]) * 5
			));

			quad.color = charColor;

			return quad;
		}
	}
}
