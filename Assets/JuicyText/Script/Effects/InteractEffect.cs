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
			Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(text.rectTransform.GetComponentInParent<Canvas>().transform,text.transform);

			Vector2 absoluteCenter = new Vector2(
				Mathf.LerpUnclamped(bounds.min.x, bounds.max.x, text.rectTransform.pivot.x), 
				Mathf.LerpUnclamped(bounds.min.y, bounds.max.y, text.rectTransform.pivot.y)
			);
			
			Vector2 mousePosition = (Vector2)Input.mousePosition - absoluteCenter - new Vector2(Screen.currentResolution.width, Screen.currentResolution.height)/2;
			Vector2 fromToMouse = mousePosition - (Vector2)quad.Position;

			float magn = fromToMouse.magnitude;
			if (magn == 0)
				return quad;

			quad.Position = quad.Position+ Vector3.ClampMagnitude(-fromToMouse, 20)/magn*10;

			return quad;
		}
	}
}
