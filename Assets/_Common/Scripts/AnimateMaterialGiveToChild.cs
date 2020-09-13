using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.GitHub.Knose1.Common
{
	public class AnimateMaterialGiveToChild : MonoBehaviour
	{
		private const float MIN_INTENSITY = 0.001f;
		public Material originalMaterial;
		private Material material;

		[Header("Animation")]
		public List<string> texturesOffsetProperties = new List<string>();
		public Vector2 tilingAnimate;
		Vector2 currentTilingValue;

		[Space]
		public List<string> texturesColorProperties = new List<string>();
		public float hueAnimate;
		public Color startColor;
		public float colorIntensity;
		private Color currentColorValue;

		private void OnValidate()
		{
			if (!originalMaterial) return;

			Start();
		}

		private void Start()
		{
			MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>();
			material = new Material(originalMaterial);

			for (int i = childRenderers.Length - 1; i >= 0; i--)
			{
				childRenderers[i].material = material;
			}

			currentTilingValue.x = 0;
			currentTilingValue.y = 0;
			currentColorValue = startColor * Mathf.Max(colorIntensity, MIN_INTENSITY);

			//DynamicGI.SetEmissive(GetComponentInChildren<Renderer>(), currentColorValue);
		}

		private void Update()
		{
#if UNITY_EDITOR
			material.CopyPropertiesFromMaterial(originalMaterial);
#endif
			float deltaTime = Time.deltaTime;

			//Tiling
			currentTilingValue += tilingAnimate * deltaTime;
			currentTilingValue.x %= 1;
			currentTilingValue.y %= 1;


			for (int i = texturesOffsetProperties.Count - 1; i >= 0; i--)
			{
				material.SetTextureOffset(texturesOffsetProperties[i], currentTilingValue);
			}

			//Color

			Color.RGBToHSV(currentColorValue, out float H, out float S, out float V);

			H += hueAnimate * deltaTime;
			H %= 360;
			float A = currentColorValue.a;
			float intensity = (currentColorValue.r + currentColorValue.g + currentColorValue.b) / 3f;
		
			float newIntensity = Mathf.Max(colorIntensity, MIN_INTENSITY);
			
			currentColorValue = Color.HSVToRGB(H,S,V, true);

			if (intensity != 0)
			{
				currentColorValue.r *= newIntensity / intensity;
				currentColorValue.g *= newIntensity / intensity;
				currentColorValue.b *= newIntensity / intensity;
			}

			currentColorValue.a = A;

			for (int i = texturesColorProperties.Count - 1; i >= 0; i--)
			{
				material.SetColor(texturesColorProperties[i], currentColorValue);
			}
		}
	}
}