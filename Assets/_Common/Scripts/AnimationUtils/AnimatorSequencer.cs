using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Com.GitHub.Knose1.Common.Attributes.PropertyAttributes;
using Com.GitHub.Knose1.Common.Utils;
using Com.GitHub.Knose1.Common.AnimationUtils;
using UnityEngine;
using System.Data;
using Com.GitHub.Knose1.Common.UI;

namespace Com.GitHub.Knose1.Common.AnimationUtils
{
	public class AnimatorSequencer : MonoBehaviour
	{
		public enum SequenceType
		{
			MathFunction,
			List
		}

		[Serializable]
		public struct AnimatorData
		{
			public int childIndex;
			public float startPlayTime;
			[SerializeField] public AnimatorParameter parameter;
		}

		private const string DEBUG_PREFIX = "[" + nameof(AnimatorSequencer) + "]";
		
		private const string WHITE_SPACE = " ";

		private const string CHILD_INDEX_REPLACEABLE = "i";
		private const string CHILD_INDEX_REPLACEABLE_SPACED = WHITE_SPACE + CHILD_INDEX_REPLACEABLE + WHITE_SPACE;
		
		private const string CHILD_COUNT_REPLACEABLE = "c";
		private const string CHILD_COUNT_REPLACEABLE_SPACED = WHITE_SPACE + CHILD_COUNT_REPLACEABLE + WHITE_SPACE;

		private const string X_REPLACEABLE = "x";
		private const string X_REPLACEABLE_SPACED = WHITE_SPACE + X_REPLACEABLE + WHITE_SPACE;

		private const string Y_REPLACEABLE = "y";
		private const string Y_REPLACEABLE_SPACED = WHITE_SPACE + Y_REPLACEABLE + WHITE_SPACE;

		private const string WIDTH_REPLACEABLE = "w";
		private const string WIDTH_REPLACEABLE_SPACED = WHITE_SPACE + WIDTH_REPLACEABLE + WHITE_SPACE;

		private const string HEIGHT_REPLACEABLE = "h";
		private const string HEIGHT_REPLACEABLE_SPACED = WHITE_SPACE + HEIGHT_REPLACEABLE + WHITE_SPACE;

		private bool isPlaying = false;
		private List<AnimatorData> notAnimatedYet = new List<AnimatorData>();
		private List<Animator> animators = new List<Animator>();
		
		[SerializeField] private bool executeOnStart = false;
		[SerializeField] private SequenceType sequenceType = SequenceType.MathFunction;
		[SerializeField] private bool computeAnimatorsOnExecute = true;
		
		[Header("Sequence Function")]
		[SerializeField, Tooltip("Variables:\ni: index\nc child count\n\nBetterGrid:\nx: x grid position\ny: y grid position\nw: width\nh: height ")] private string animatorSequenceFunction = "";
		[SerializeField] public bool animatorSequenceFunctionDebug;
		[SerializeField] public AnimatorParameter animatorSequenceFunctionParameter;

		[Header("Sequence List")]
		[SerializeField] private List<AnimatorData> animatorSequenceList;

		private BetterGrid betterGrid;
		private float time = 0;

		private void Awake()
		{
			TryGetComponent<BetterGrid>(out betterGrid);
		}

		private void Start()
		{
			if (!computeAnimatorsOnExecute) ComputeAnimatorsFromAnimatorData();

			if (executeOnStart) Execute();
		}

		public void ComputeAnimators()
		{
			switch (sequenceType)
			{
				case SequenceType.MathFunction:
					
						ComputeAnimatorsFromMathFunction();
						
					break;
				case SequenceType.List:
					ComputeAnimatorsFromAnimatorData();
					break;
			}
		}

		/// <summary>
		/// Compute <see cref="animators"/> when <see cref="sequenceType"/> is <see cref="SequenceType.MathFunction"/>
		/// </summary>
		protected virtual void ComputeAnimatorsFromMathFunction()
		{
			int childCount = transform.childCount;

			animators = new List<Animator>();
			animatorSequenceList = new List<AnimatorData>();

			Regex addSpaceAroundWords = new Regex("\\w+");

			for (int i = 0; i < childCount; i++)
			{
				animators.Add(transform.GetChild(i).GetComponent<Animator>());

				string mathOperation = addSpaceAroundWords.Replace(animatorSequenceFunction, " $& ").Replace('.', ',');
				float fl = 0;
				DataTable dt = new DataTable();
				
				if	(!betterGrid && 
						(
							animatorSequenceFunction.Contains(X_REPLACEABLE_SPACED) || 
							animatorSequenceFunction.Contains(Y_REPLACEABLE_SPACED) || 
							animatorSequenceFunction.Contains(WIDTH_REPLACEABLE_SPACED) || 
							animatorSequenceFunction.Contains(HEIGHT_REPLACEABLE_SPACED)
						)
					)
				{
					Debug.LogWarning(DEBUG_PREFIX+" "+$"Trying to acces {X_REPLACEABLE}, {Y_REPLACEABLE} or {WIDTH_REPLACEABLE} in math expression without "+nameof(BetterGrid)+" component");
				}
				else
				{
					string debugString = "";
					debugString += CHILD_INDEX_REPLACEABLE + ":" + i;
					debugString += " " + CHILD_COUNT_REPLACEABLE + ":" + childCount;

					mathOperation = mathOperation
						.Replace(CHILD_INDEX_REPLACEABLE_SPACED, i.ToString())
						.Replace(CHILD_COUNT_REPLACEABLE_SPACED, childCount.ToString());

					if (betterGrid)
					{
						betterGrid.GetPosFromIndex(i, out int x, out int y);
						int w = betterGrid.GetChildsOnRow(y);
						betterGrid.GetUnclampedSecondAxis(out _, out int h);
						mathOperation = mathOperation
							.Replace(X_REPLACEABLE_SPACED, x.ToString())
							.Replace(Y_REPLACEABLE_SPACED, y.ToString())
							.Replace(WIDTH_REPLACEABLE_SPACED, w.ToString())
							.Replace(HEIGHT_REPLACEABLE_SPACED, h.ToString());

						debugString += " " + X_REPLACEABLE + ":" + x;
						debugString += " " + Y_REPLACEABLE + ":" + y;
						debugString += " " + WIDTH_REPLACEABLE + ":" + w;
						debugString += " " + HEIGHT_REPLACEABLE + ":" + h;

					}

					if (animatorSequenceFunctionDebug) Debug.Log(DEBUG_PREFIX + " " + debugString);

					object obj = dt.Compute(mathOperation, "");
					fl = float.Parse(obj.ToString());
				}

				
				animatorSequenceList.Add(new AnimatorData() { childIndex = i, parameter = animatorSequenceFunctionParameter, startPlayTime = fl});
			}
		}

		/// <summary>
		/// Compute <see cref="animators"/> when <see cref="sequenceType"/> is <see cref="SequenceType.List"/>
		/// </summary>
		protected virtual void ComputeAnimatorsFromAnimatorData()
		{
			if (animatorSequenceList.Count == 0)
			{
				animators = new List<Animator>();
				return;
			}

			animators = Enumerable.Repeat<Animator>(null, animatorSequenceList.Map((AnimatorData ad) => ad.childIndex).Max() + 1).ToList();

			for (int i = animatorSequenceList.Count - 1; i >= 0; i--)
			{
				int index = animatorSequenceList[i].childIndex;

				Transform childTr = transform.GetChild(index);
				if (!childTr) continue;

				Animator anim = null;
				anim = animators[index];

				if (anim is null)
					animators[index] = childTr.GetComponentInChildren<Animator>();
			}
		}

		public void Execute()
		{
			if (computeAnimatorsOnExecute) ComputeAnimators();

			time = 0;
			notAnimatedYet = animatorSequenceList;
			isPlaying = true;
		}

		public void Stop()
		{
			time = 0;
			notAnimatedYet = null;
			isPlaying = false;
		}

		private void Update()
		{
			if (isPlaying)
			{
				if (notAnimatedYet.Count == 0) Stop();
				else
				{
					for (int i = animatorSequenceList.Count - 1; i >= 0; i--)
					{
						AnimatorData data = animatorSequenceList[i];
						int index = data.childIndex;
						Animator animator = animators[index];

						if (!animator)
						{
							Debug.LogWarning(DEBUG_PREFIX + " Animator not found on child " + index);
							notAnimatedYet.RemoveAt(i);
							continue;
						}
						if (animator && time >= data.startPlayTime)
						{
							data.parameter.Call(animator);
							notAnimatedYet.RemoveAt(i);
						}
					}

					time += Time.deltaTime;
				}
			}
		}
	}
}
