using Com.GitHub.Knose1.Common.XML;
using Com.GitHub.Knose1.JuicyText.Attributes;
using UnityEngine;

namespace Com.GitHub.Knose1.JuicyText.Effects
{
	[TextTag(TextTagUsage.Runtime, "interact_avoid")]
	internal sealed class InteractEffect
	{
		public static MeshQuad Update(int quadIndex, MeshQuad quad, XMLTag tag, TextEffect text)
		{
			Vector2 fromToMouse = text.ScreenSpaceToMeshSpace(Input.mousePosition) - (Vector2)quad.Position;

			float magn = fromToMouse.magnitude;
			if (magn == 0)
				return quad;

			quad.Position = quad.Position+ Vector3.ClampMagnitude(-fromToMouse, 20)/magn*10;

			return quad;
		}
	}
}
