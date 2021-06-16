using Com.GitHub.Knose1.Common.XML;
using Com.GitHub.Knose1.JuicyText.Attributes;
using UnityEngine;

namespace Com.GitHub.Knose1.JuicyText.Effects
{
	[TextTag(TextTagUsage.Runtime, "rainbow")]
	internal sealed class RainbowEffect
	{
		public static MeshQuad Update(int quadIndex, MeshQuad quad, XMLTag tag, TextEffect text)
		{
			XMLAttribute? attMove = tag.GetAttributeIfExist("moveSpeed");
			XMLAttribute? attColor = tag.GetAttributeIfExist("colorSpeed");
			XMLAttribute? attX = tag.GetAttributeIfExist("xDistance");
			XMLAttribute? attY = tag.GetAttributeIfExist("yDistance");
			float moveSpeed = 1;
			float colorSpeed = 1;
			float xDistance = 2.5f;
			float yDistance = 5;

			if (attMove.HasValue) moveSpeed = float.Parse(attMove.Value.value);
			if (attColor.HasValue) colorSpeed = float.Parse(attColor.Value.value);
			if (attX.HasValue) xDistance = float.Parse(attX.Value.value);
			if (attY.HasValue) yDistance = float.Parse(attY.Value.value);

			float currentTime = text.currentQuadTime[quadIndex];
			Color charColor = Color.HSVToRGB(currentTime * colorSpeed % 1, 1, 1);
			charColor.a = quad.color.a;

			quad.Translate(new Vector3(
				Mathf.Cos(currentTime * moveSpeed) * xDistance,
				Mathf.Sin(currentTime * moveSpeed) * yDistance
			));

			quad.color = charColor;

			return quad;
		}
	}
}
