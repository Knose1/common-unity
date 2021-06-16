using Com.GitHub.Knose1.Common.XML;
using Com.GitHub.Knose1.JuicyText.Attributes;
using System.Text;
using UnityEngine;

namespace Com.GitHub.Knose1.JuicyText.Effects
{
	[TextTag(TextTagUsage.Runtime, "shake")]
	internal sealed class ShakeEffect
	{
		
		public static MeshQuad Update(int quadIndex, MeshQuad quad, XMLTag tag, TextEffect text)
		{
			XMLAttribute? attForce = tag.GetAttributeIfExist("force");
			XMLAttribute? attFrequency = tag.GetAttributeIfExist("frequency");

			float force = 1;
			float frequency = 1;

			if (attForce.HasValue) force = float.Parse(attForce.Value.value);
			if (attFrequency.HasValue) frequency = float.Parse(attFrequency.Value.value);

			quad.Translate(Random.insideUnitCircle * force);

			return quad;
		}
	}
}
