//-///////////////////////////////////////////////////////////-//
//                                                             //
// This script handle the quad generation                      //
//                                                             //
//-///////////////////////////////////////////////////////////-//

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.GitHub.Knose1.JuicyText
{
	/// <summary>
	/// The default Graphic to draw font data to screen.
	/// </summary>
	public partial class TextEffect : Text
	{
		/// <summary>
		/// A the data of quads without the custom effects
		/// </summary>
		protected List<MeshQuad> unModifiedQuads = new List<MeshQuad>();

		/// <summary>
		/// A the data of quads with the custom effects
		/// </summary>
		protected List<MeshQuad> quads = new List<MeshQuad>();

		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			int previousQuadsCount = unModifiedQuads.Count;

			//Clear previous quads
			toFill.Clear();

			GenerateQuads();
			quads = new List<MeshQuad>(unModifiedQuads);

			//Get if there is more quads (difference)
			int unModifiedQuadsCount = unModifiedQuads.Count;
			int difference = Mathf.Max(0, unModifiedQuadsCount - previousQuadsCount); //Can't be negative

			//This part add a time slot for each new generated quads
			for (int i = 0; i < difference; i++)
			{
				currentQuadTime.Add(0);
			}
			UpdateQuadsEffect(false); //Call the update

			//For each quads, add quad to the screen
			int quadsCount = quads.Count;
			for (int i = 0; i < quadsCount; i++)
			{
				toFill.AddUIVertexQuad(quads[i]);
			}
		}

		MeshQuad m_TempVerts = new MeshQuad();
		private void GenerateQuads()
		{
			// A part of the function is from :
			// https://github.com/Unity-Technologies/uGUI/blob/2019.1/UnityEngine.UI/UI/Core/Text.cs#L645

			if (font == null)
				return;

			// We don't care if we the font Texture changes while we are doing our Update.
			// The end result of cachedTextGenerator will be valid for this instance.
			// Otherwise we can get issues like Case 619238.
			m_DisableFontTextureRebuiltCallback = true;

			Vector2 extents = rectTransform.rect.size;

			var settings = GetGenerationSettings(extents);
			cachedTextGenerator.PopulateWithErrors(textToShow, settings, gameObject);

			// Apply the offset to the vertices
			IList<UIVertex> verts = cachedTextGenerator.verts;
			float unitsPerPixel = 1 / pixelsPerUnit;
			int vertCount = verts.Count;

			// We have no verts to process just return (case 1037923)
			if (vertCount <= 0)
			{
				unModifiedQuads.Clear();
				return;
			}

			Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
			roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
			unModifiedQuads.Clear();
			if (roundingOffset != Vector2.zero)
			{
				for (int i = 0; i < vertCount; ++i)
				{
					int tempVertsIndex = i & 3;

					UIVertex uIVertex = verts[i];
					uIVertex.position *= unitsPerPixel;
					uIVertex.position.x += roundingOffset.x;
					uIVertex.position.y += roundingOffset.y;
					m_TempVerts[tempVertsIndex] = uIVertex;

					if (tempVertsIndex == 3)
						unModifiedQuads.Add(m_TempVerts.Copy());
				}
			}
			else
			{
				for (int i = 0; i < vertCount; ++i)
				{
					int tempVertsIndex = i & 3;

					UIVertex uIVertex = verts[i];
					uIVertex.position *= unitsPerPixel;
					m_TempVerts[tempVertsIndex] = uIVertex;

					if (tempVertsIndex == 3)
						unModifiedQuads.Add(m_TempVerts.Copy());
				}
			}

			m_DisableFontTextureRebuiltCallback = false;
		}
	}
}
