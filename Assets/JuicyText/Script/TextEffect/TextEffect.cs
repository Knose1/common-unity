//-///////////////////////////////////////////////////////////-//
//                                                             //
// This script handle the juicyness and the unity messages     //
//                                                             //
//-///////////////////////////////////////////////////////////-//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.GitHub.Knose1.JuicyText
{
	/// <summary>
	/// Draw font on the screen with effects
	/// </summary>
	[AddComponentMenu("UI/Effects/" + nameof(TextEffect))]
	public partial class TextEffect : Text
	{
		/// <summary>
		/// The interval before a new char/quad apear
		/// </summary>
		[SerializeField] public float typingInterval = 0.1f;

		/// <summary>
		/// If true, <see cref="StartText"/> will be called on <see cref="Start"/>
		/// </summary>
		[SerializeField] public bool startTextOnStart = true;

		/// <summary>
		/// If true, <see cref="OnLetterGenerate"/> with yield return new <see cref="WaitForSeconds"/>(<see cref="typingInterval"/>);
		/// </summary>
		[NonSerialized] public bool doDefaultPause;

		/// <summary>
		/// The elapsed time since the quad was spawned
		/// </summary>
		[NonSerialized] public List<float> currentQuadTime = new List<float>();

		/// <summary>
		/// True if the text is writing
		/// </summary>
		public bool IsPlaying => textCoroutine != null;

		/// <summary>
		/// Text rendered at the end. <see cref="TextCoroutine"/>
		/// Proced with care when modifying it.
		/// </summary>
		public string textToShow;

		/// <summary>
		/// See : <see cref="TextCoroutine"/>
		/// </summary>
		private Coroutine textCoroutine;

		/// <summary>
		/// See : <see cref="UpdateQuadsEffect"/>
		/// </summary>
		public bool UpdateTime { get; private set; }

		protected TextEffect() : base() { }

		public override string text { get => base.text; set => base.text = value.Replace("\r", ""); }

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();

			text = text.Replace("\r", "");

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
			if (startTextOnStart) StartText();
		}

		protected virtual void Update()
		{
			UpdateQuadsEffect();
			SetVerticesDirty();
		}

		/// <summary>
		/// Start the text effect.
		/// </summary>
		public void StartText()
		{
			currentQuadTime.Clear();
			if (textCoroutine != null) StopText();

			textCoroutine = StartCoroutine(TextCoroutine());
		}

		/// <summary>
		/// Stop the the text effect.
		/// </summary>
		public void StopText()
		{
			StopCoroutine(textCoroutine);
			textCoroutine = null;
		}

		/// <summary>
		/// Convert a point from screen space to mesh space.
		/// </summary>
		/// <param name="screenPoint"></param>
		/// <returns></returns>
		public Vector2 ScreenSpaceToMeshSpace(Vector2 screenPoint)
		{
			Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform.GetComponentInParent<Canvas>().transform, transform);

			Vector2 absoluteCenter = new Vector2(
				Mathf.LerpUnclamped(bounds.min.x, bounds.max.x, rectTransform.pivot.x),
				Mathf.LerpUnclamped(bounds.min.y, bounds.max.y, rectTransform.pivot.y)
			);

			return screenPoint - absoluteCenter - new Vector2(Screen.currentResolution.width, Screen.currentResolution.height)/2;
		}


		/// <summary>
		/// Return true if the quad exist<br/>
		/// See also <seealso cref="WaitForQuadExist"/>
		/// </summary>
		/// <param name="index">Quad index</param>
		/// <returns></returns>
		public bool DoQuadExist(int index)
		{
			return quads.Count > index;
		}

		/// <summary>
		/// Wait for quad to exist<br/>
		/// See also <seealso cref="DoQuadExist"/>
		/// </summary>
		/// <param name="index">Quad index</param>
		/// <returns></returns>
		public IEnumerator WaitForQuadExist(int index)
		{
			Func<bool> predicate = () => DoQuadExist(index);
			return new WaitUntil(predicate);
		}

		/// <summary>
		/// This function is called in the <see cref="Update"/> and in the <see cref="OnPopulateMesh"/>.<br/>
		/// It iterates on every quad and execute their <see cref="DoQuadUpdate"/>.<br/>
		/// Override only if you know what you're doing.
		/// </summary>
		protected virtual void UpdateQuadsEffect(bool updateTime = true)
		{
			UpdateTime = updateTime;
#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif

			int quadsCount = quads.Count;

			for (int i = quadsCount - 1; i >= 0; i--)
			{
				if (updateTime) this.currentQuadTime[i] += Time.deltaTime;

				quads[i] = DoQuadUpdate(i, quads[i]);
			}


		}

		/// <summary>
		/// This function is called in the <see cref="UpdateQuadsEffect"/>.<br/>
		/// Override this function to give a custom global effect.<br/>
		/// Don't forget to call <see cref="DoXMLTagQuadUpdate"/>
		/// </summary>
		/// <param name="index">Quad index</param>
		/// <param name="quad">Quad data</param>
		/// <returns></returns>
		protected virtual MeshQuad DoQuadUpdate(int index, MeshQuad quad)
		{
			//Custom effect example:
			/*
			Color col = quad.color;
			col.a = 1-(currentQuadTime[index ]/ 3);

			quad.color = col;
			*/

			return DoXMLTagQuadUpdate(index, quad);
		}

		/// <summary>
		/// Search for xml tag and execute their update.
		/// </summary>
		/// <param name="index">Quad index</param>
		/// <param name="quad">Quad data</param>
		/// <returns></returns>
		protected MeshQuad DoXMLTagQuadUpdate(int index, MeshQuad quad)
		{
			var hierarchy = xmlTagHierarchyByIndex[index];

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

		/// <summary>
		/// This event is called before any character get generated.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator OnBeforeStartLetterGeneration()
		{
			yield return new WaitForSeconds(typingInterval);
		}

		/// <summary>
		/// This event is called when a new characer has been generated.
		/// </summary>
		/// <param name="c">New characted</param>
		/// <param name="quadIndex">Quad index</param>
		/// <returns></returns>
		protected virtual IEnumerator OnLetterGenerate(char c, int quadIndex)
		{
			doDefaultPause = true;

			foreach (var item in xmlTagHierarchy)
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
					var subRoutine = (IEnumerator)method.Invoke(t, new object[] { c, quadIndex, item, this });
					if (subRoutine != null)
						yield return subRoutine;
				}
				else if (IsVoid(method))
				{
					method.Invoke(t, new object[] { c, quadIndex, item, this });
				}
				else
				{
					Debug.LogWarning("Return type of the method " + t.Name + "." + LETTER_ADDED_METHOD + " must be or \"void\" or \"" + nameof(IEnumerator) + "\"");
				}
			}

			if (doDefaultPause)
				yield return new WaitForSeconds(typingInterval);
		}
	}

}
