using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Com.GitHub.Knose1.Common.Attributes.PropertyAttributes;
using Com.GitHub.Knose1.Common.Utils;
using Com.GitHub.Knose1.Common.AnimationUtils;
using UnityEngine;

namespace Com.GitHub.Knose1.Common.UI.GridLayout
{
	[RequireComponent(typeof(BetterGrid))]
	public class BetterGridAnimator : MonoBehaviour
	{
		[Serializable]
		public struct GridAnimatorData
		{
			public int tileIndex;
			public float playTime;
			[SerializeField] public AnimatorParameter parameter;
		}

		private BetterGrid betterGrid;

		private List<Animator> animators = new List<Animator>();
		[SerializeField] private List<GridAnimatorData> animatorSequence;
		
		private void Awake()
		{
			betterGrid = GetComponent<BetterGrid>();
			Transform betterGridTransform = betterGrid.transform;

			for (int i = animatorSequence.Count - 1; i >= 0; i--)
			{
				int index = animatorSequence[i].tileIndex;
				object anim = null;
				try
				{
					anim = animators[i];
				}
				catch (Exception)
				{
					animators.Insert(index, betterGridTransform.GetChild(index).GetComponentInChildren<Animator>());
					continue;
				}

				if (anim is null)
					animators[i] = betterGridTransform.GetChild(index).GetComponentInChildren<Animator>();
			}
		}
	}
}
