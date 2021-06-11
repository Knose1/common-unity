using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace Com.GitHub.Knose1.Common.Editor.ShaderAttribute
{
	public class GradientDrawer : MaterialPropertyDrawer
	{
		private const string GRADIENT = "T_G_";

		//private const int TEXTURE_WIDTH = 100;
		Texture2D gradientTexture = null;
		Gradient gradient = null;

		protected readonly string gradentModeParam;
		protected readonly string sizeParam;

		public GradientDrawer(string gradentModeParam) {
			this.gradentModeParam = gradentModeParam;
		}
		public GradientDrawer(string gradentModeParam, string sizeParam)
		{
			this.gradentModeParam = gradentModeParam;
			this.sizeParam = sizeParam;
		}

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			//Handle error
			if (editor.targets.Length > 1)
			{
				GUI.Label(position, "Can't edit multiple gradient");
				return;
			}
			editor.TextureCompatibilityWarning(prop);

			//Setup
			Material m = (Material)editor.target;
			gradientTexture = FindSubTexture(m, prop);
			GetGradientFromTexture(m);

			//BEGIN CHANGE CHECK
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;

			//Show gradient
			gradient = EditorGUI.GradientField(position, label, gradient);
			
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck()) 
			{
				UpdateGradientTexture(gradient);
				prop.textureValue = gradientTexture;

				//If size param
				if (sizeParam != string.Empty)
				{
					//Set size param's value
					int width = gradientTexture.width;
					m.SetFloat(sizeParam, width);
					m.SetFloat(gradentModeParam, (float)gradient.mode);
				}
			}
		}

		/// <summary>
		/// Find the sub texture
		/// </summary>
		/// <param name="target"></param>
		/// <param name="prop"></param>
		/// <returns></returns>
		private static Texture2D FindSubTexture(UnityEngine.Object target, MaterialProperty prop)
		{
			Texture2D texture2D = null;

			texture2D = (Texture2D)AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target))
				.FirstOrDefault((o) => o is Texture2D && o.name == GetTextureAssetName(prop));

			if (!texture2D) AssetDatabase.AddObjectToAsset(texture2D = GenerateNewTexture(prop), target);

			return texture2D;
		}

		/// <summary>
		/// Get the gradient from the texture
		/// </summary>
		/// <param name="m"></param>
		private void GetGradientFromTexture(Material m)
		{
			gradient = new Gradient();

			int width = gradientTexture.width;
			GradientColorKey[] keys = new GradientColorKey[width];

			for (int i = 0; i < width; i++)
			{
				Color pixel = gradientTexture.GetPixel(i, 0);
				float time = pixel.a;
				pixel.a = 1;

				keys[i] = new GradientColorKey(pixel, time);
			}

			gradient.colorKeys = keys;
			gradient.mode = (GradientMode)m.GetFloat(gradentModeParam);
		}
		
		/// <summary>
		/// Update the texture's size and pixels depending on the gradient
		/// </summary>
		/// <param name="gradient"></param>
		private void UpdateGradientTexture(Gradient gradient)
		{
			GradientColorKey[] colorKeys = gradient.colorKeys;
			int lLength = colorKeys.Length;
			gradientTexture.Resize(Mathf.Max(2, lLength), 1);
			gradientTexture.filterMode = FilterMode.Point;
			
			for (int i = 0; i < lLength; i++)
			{
				GradientColorKey lKey = colorKeys[i];
				Color lColor = lKey.color;
				lColor.a = lKey.time;
				gradientTexture.SetPixel(i,0, lColor);
				gradientTexture.Apply();
			}
		}
		
		/// <summary>
		/// Get the texture asset name
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		private static string GetTextureAssetName(MaterialProperty prop) => GRADIENT + "_" + prop.displayName;

		/// <summary>
		/// Create a new texture
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		private static Texture2D GenerateNewTexture(MaterialProperty prop)
		{
			Texture2D toReturn = new Texture2D(2, 1);
			toReturn.name = GRADIENT+"_"+prop.displayName;
			toReturn.SetPixel(0, 0, new Color(0, 0, 0, 0));
			toReturn.SetPixel(1, 0, new Color(1, 1, 1, 1));
			toReturn.Apply();
			toReturn.filterMode = FilterMode.Point;
			return toReturn;
		}

	}
}
