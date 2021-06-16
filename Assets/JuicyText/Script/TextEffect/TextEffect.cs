using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.GitHub.Knose1.JuicyText
{
	public partial class TextEffect : Text
	{
		[SerializeField] public float typingInterval = 0.1f;
		[NonSerialized] public bool doDefaultPause;
		[NonSerialized] public List<float> currentQuadTime = new List<float>();


		public bool IsPlaying => textCoroutine != null;
		protected string textToShow;
		Coroutine textCoroutine;

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();

			if (Application.isPlaying)
			{
				if (IsPlaying) StartText();
			}
			else
			{
				textToShow = text;
			}
		}
#endif

		protected override void Start()
		{
			base.Start();
			InitTags();

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				textToShow = text;
				return;
			}
#endif
			StartText();
		}

		public void StartText()
		{
			currentQuadTime.Clear();
			if (textCoroutine != null) StopText();

			textCoroutine = StartCoroutine(TextCoroutine());
		}

		private void StopText()
		{
			StopCoroutine(textCoroutine);
			textCoroutine = null;
		}

		private enum CharPosition
		{
			Outside, Inside, Balise
		}

		private void Update()
		{
			UpdateQuadsEffect();
			SetVerticesDirty();
		}

		protected virtual void UpdateQuadsEffect()
		{

#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif

			int quadsCount = quads.Count;

			for (int i = quadsCount - 1; i >= 0; i--)
			{
				this.currentQuadTime[i] += Time.deltaTime;
				float currentTime = this.currentQuadTime[i];

				quads[i] = DoQuadUpdate(i, quads[i]);
			}


		}

		protected MeshQuad DoXMLQuadUpdate(int index, MeshQuad quad)
		{
			/*if (!xmlBaliseHierarchyByIndex.ContainsKey(index))
			{
				return quad;
			}*/

			var hierarchy = xmlBaliseHierarchyByIndex[index];

			foreach (var item in hierarchy)
			{
				Type t = GetTagRuntimeHandler(item);
				if (t is null)
					continue;

				var method = GetUpdateMethode(t);
				if (method is null)
				{
					Debug.LogWarning(t.Name + " doesn't implement the method \"" + UPDATE_METHOD + "\"");
					continue;
				}

				if (IsMeshQuad(method))
				{
					quad = (MeshQuad)method.Invoke(t, new object[] { index, quad, item, this });
				}

				else
				{
					Debug.LogWarning("Return type of the method " + t.Name + "." + UPDATE_METHOD + " must be or \"" + typeof(MeshQuad) + "\"");
				}
			}

			return quad;
		}
		protected virtual MeshQuad DoQuadUpdate(int index, MeshQuad quad)
		{
			/*Color.RGBToHSV(meshQuad.color, out float h, out float s, out float v);
			Color charColor = Color.HSVToRGB(currentTime + h, 1, 1);
			charColor.a = 1-(currentTime/3);

			if (charColor.a < 0)
			{
				quads.RemoveAt(i);
				continue;
			}

			meshQuad.color = charColor;*/
			//meshQuad.Rotate(Quaternion.AngleAxis(currentTime * 10, Vector3.back), meshQuad.Center());

			return DoXMLQuadUpdate(index, quad);
		}

		protected virtual IEnumerator OnBeforeLetterGenerate(int index)
		{
			return null;
		}

		protected virtual IEnumerator OnLetterGenerate(char c, int index)
		{
			doDefaultPause = true;

			foreach (var item in xmlBaliseHierarchy)
			{
				Type t = GetTagLetterAddingHandler(item);
				if (t is null)
					continue;

				var method = GetLetterAddedMethode(t);
				if (method is null)
				{
					Debug.LogWarning(t.Name + " doesn't implement the method \"" + LETTER_ADDED_METHOD + "\"");
					continue;
				}

				if (IsIEnumerator(method))
				{
					yield return (IEnumerator)method.Invoke(t, new object[] { c, index, item, this });
				}

				else if (IsVoid(method))
				{
					method.Invoke(t, new object[] { c, index, item, this });
				}

				else
				{
					Debug.LogWarning("Return type of the method " + t.Name + "." + LETTER_ADDED_METHOD + " must be or \"void\" or \"IEnumerator\"");
				}
			}

			if (doDefaultPause)
			yield return new WaitForSeconds(typingInterval);
		}
	}

}
