///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 06/06/2020 12:19
///-----------------------------------------------------------------
#define KNOSE_BETTER_GRID_TWINNING

using DG.Tweening;
using DG.Tweening.Core;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace Com.GitHub.Knose1.Common.UI
{
	[RequireComponent(typeof(RectTransform))]
	[ExecuteAlways]
	public class BetterGrid : MonoBehaviour
	{
		protected const string BETTER_GRID_DEBUG_TAG = "["+nameof(BetterGrid)+"]";

		[SerializeField] int childByMainAxis = 10;
		[SerializeField] int childBySecondAxis = 10;
		[SerializeField, Range(0,1)] float padding = 0;
		[SerializeField] Corner startCorner = default;
		[SerializeField] Axis startAxis = default;
		[SerializeField] Vector2 gridCenter = new Vector2(0.5f, 0.5f);
		[SerializeField] Vector2 elementsPivot = new Vector2(0.5f, 0.5f);


#if KNOSE_BETTER_GRID_TWINNING
		[Header("Twinning")]
		[SerializeField] float moveTime = 0;
		[SerializeField] AnimationCurve moveCurve = AnimationCurve.Linear(0,0,1,1);
#if UNITY_EDITOR
		[SerializeField] bool testTwin = false;
#endif
#endif
		protected RectTransform rectTransform;

		private bool isMoving = false;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
#if KNOSE_BETTER_GRID_TWINNING
			if (testTwin && !isMoving)
			{
				testTwin = false;
				MoveGridDown(null);
			}
#endif
		}
#endif

		protected void Start()
		{
			if (!Application.isPlaying) return;

			GetSecondAxis(out int length, out int colCount);

			for (int i = 0; i < length; i++)
			{
				Transform child = transform.GetChild(i);

				GetPosFromIndex(i, out int posX, out int posY);

				bool canBeActive = posY < (colCount + 1);

				if (!canBeActive)
				{
					DispatchRemovedEvent(child.GetComponentsInChildren<IBetterGridElement>(), posX, posY);
					child.gameObject.SetActive(false);
				}

				DispatchAddedEvent(child.GetComponentsInChildren<IBetterGridElement>(), posX, posY);

			}
		}

		private void Update()
		{
			if (!rectTransform) rectTransform = transform as RectTransform;

			GetSecondAxis(out int length, out int colCount);

			for (int i = 0; i < length; i++)
			{
				RectTransform child = transform.GetChild(i) as RectTransform;

				GetPosFromIndex(i, out int posX, out int posY);
				PosToWorld(posX, posY, out Vector2 min, out Vector2 max);

				bool canBeActive = posY < (colCount + 1);

				child.sizeDelta = new Vector2(0, 0);
				child.anchoredPosition = new Vector2(0, 0);

				child.anchorMin = min;
				child.anchorMax = max;

				child.pivot = elementsPivot;
			}
		}

		/// <summary>
		/// Get the total number of Y axis
		/// </summary>
		/// <param name="length">The number of child</param>
		/// <param name="elementCount">The number of column</param>
		public void GetUnclampedSecondAxis(out int length, out int elementCount)
		{
			length = transform.childCount;
			elementCount = (length - length % childByMainAxis) / childByMainAxis + (length % childByMainAxis == 0 ? 0 : 1);
		}

		/// <summary>
		/// Get the clamped number of Y axis
		/// </summary>
		/// <param name="length">The number of child</param>
		/// <param name="elementCount">The number of column</param>
		public void GetSecondAxis(out int length, out int elementCount)
		{
			GetUnclampedSecondAxis(out length, out elementCount);
			elementCount = Mathf.Min(elementCount, childBySecondAxis);
		}

		/// <summary>
		/// Get a tile at a certain position of the grid
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public GameObject GetTileAt(int x, int y)
		{
			GetUnclampedSecondAxis(out int length, out int colCount);

			if (x < 0 || y < 0 || x >= childByMainAxis || y >= colCount)
			{
				Debug.LogWarning(BETTER_GRID_DEBUG_TAG + " You are trying to get a child out of bounds");
				return null;
			}

			int index = y * childByMainAxis + x;

			return transform.GetChild(index).gameObject;
		}

		private void GetPosFromIndex(int index, out int posX, out int posY)
		{
			posX = index % childByMainAxis;
			posY = (index - posX) / childByMainAxis;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="posX"></param>
		/// <param name="posY"></param>
		public void PosToWorld(int posX, int posY, out Vector2 min, out Vector2 max)
		{
			GetUnclampedSecondAxis(out int _, out int unclampedColCount);
			GetSecondAxis(out int __, out int colCount);

			switch (startCorner)
			{
				case Corner.LowerLeft:
					break;
				case Corner.LowerRight:
					posX = childByMainAxis - posX - 1;
					break;
				case Corner.UpperRight:
					posX = childByMainAxis - posX - 1;
					posY = unclampedColCount - posY - 1;
					break;
				case Corner.UpperLeft:
					posY = unclampedColCount - posY - 1;
					break;
			}

			float cellSize = 1 - padding;

			float xMin = cellSize / childByMainAxis * (posX);
			float xMax = cellSize / childByMainAxis * (posX + 1);

			float yMin = cellSize / colCount * (posY);
			float yMax = cellSize / colCount * (posY + 1);

			xMin += padding / colCount;
			xMax -= padding / colCount;
			yMin += padding / colCount;
			yMax -= padding / colCount;


			xMin += padding * gridCenter.x;
			xMax += padding * gridCenter.x;
			yMin += padding * gridCenter.y;
			yMax += padding * gridCenter.y;

			min = new Vector2(xMin, yMin);
			max = new Vector2(xMax, yMax);


			if (startAxis == Axis.Vertical)
			{
				Vector2 _min = new Vector2(1 - max.y, 1 - max.x);
				Vector2 _max = new Vector2(1 - min.y, 1 - min.x);

				min = _min;
				max = _max;
			}
		}

#if KNOSE_BETTER_GRID_TWINNING
		/*
		 * An old twin I made for a gamejam, uncomment to use
		 * Only works with LowerRight Corner, without padding and with one extra line
		 */

		/*////////////////////////////////////////////////////////////////////////////////////*/
		/*                                                                                    */
		/*                                      TWINNING                                      */
		/*                                                                                    */
		/*////////////////////////////////////////////////////////////////////////////////////*/
		
			/// <param name="nextVisibleLine">The line that will be visible at the next call of MoveGridDown</param>
			public delegate void MoveComplete(List<GameObject> nextVisibleLine);

			/// <summary>
			/// Move all the gameobjects down
			/// The bottomLine is moved to the top and disabled
			/// </summary>
			public void MoveGridDown(MoveComplete onComplete)
			{
				if (!Application.isPlaying)
				{
					Debug.LogWarning(BETTER_GRID_DEBUG_TAG+" Can't Move the Grid in editor");
					return;
				}

				if (isMoving)
				{
					Debug.LogWarning(BETTER_GRID_DEBUG_TAG+" Wait for the Grid to finish moving before calling " +nameof(MoveGridDown));
					return;
				}

				isMoving = true;

				GetUnclampedSecondAxis(out int length, out int colCount);
				List<GameObject> nextVisibleLine = new List<GameObject>();

				//maxVisibleCol + 1 will be visible so maxVisibleCol + 2 the nextVisibleLine
				int startIndex = Mathf.Min(colCount, childBySecondAxis + 2);

				//If there is no maxVisibleCol + 2, then the raw 0 will be the nextVisibleLine
				if (startIndex == childBySecondAxis + 1) startIndex = 0;

				for (int i = 0; i < childByMainAxis; i++)
				{
					nextVisibleLine.Add(transform.GetChild(i + startIndex).gameObject);
				}

				int colCountClamped = Mathf.Min(colCount, childBySecondAxis);

				Vector2 endPos = new Vector2(0, -rectTransform.rect.height / colCountClamped);

				TweenerCore<Vector2, Vector2, DG.Tweening.Plugins.Options.VectorOptions> twin = rectTransform.DOAnchorPos(endPos, moveTime);

				twin.onComplete = twinComplete;
				twin.SetEase(moveCurve);

				void twinComplete()
				{
					isMoving = false;

					rectTransform.anchoredPosition = Vector2.zero;

					for (int i = 0; i < childByMainAxis; i++)
					{
						Transform child = transform.GetChild(0);

						child.SetAsLastSibling();
						DispatchRemovedEvent(child.GetComponentsInChildren<IBetterGridElement>(), i, colCount);
						child.gameObject.SetActive(false);
					}

					for (int i = 0; i < childByMainAxis; i++)
					{
						GameObject child = nextVisibleLine[i];

						child.gameObject.SetActive(true);
						DispatchAddedEvent(child.GetComponentsInChildren<IBetterGridElement>(), i, childBySecondAxis);
					}

					for (int i = 0; i < transform.childCount; i++)
					{
						GameObject child = transform.GetChild(i).gameObject;

						GetPosFromIndex(i, out int x, out int y);
						DispatchMovedEvent(child.GetComponentsInChildren<IBetterGridElement>(), x, y);
					}


					onComplete?.Invoke(nextVisibleLine);
				}
			}
#endif

		/*////////////////////////////////////////////////////////////////////////////////////*/
		/*                                                                                    */
		/*                             IBETTER GRID ELEMENT EVENT                             */
		/*                                                                                    */
		/*////////////////////////////////////////////////////////////////////////////////////*/
		protected void DispatchMovedEvent(IBetterGridElement[] betterGridElement, int x, int y)
		{
			int length = betterGridElement.Length;
			for (int i = 0; i < length; i++)
			{
				betterGridElement[i].OnMoved(x, y);
			}
		}

		protected void DispatchRemovedEvent(IBetterGridElement[] betterGridElement, int x, int y)
		{
			int length = betterGridElement.Length;
			for (int i = 0; i < length; i++)
			{
				betterGridElement[i].RemovedFromGrid(x, y);
			}
		}

		protected void DispatchAddedEvent(IBetterGridElement[] betterGridElement, int x, int y)
		{
			int length = betterGridElement.Length;
			for (int i = 0; i < length; i++)
			{
				betterGridElement[i].AddedToGrid(x, y);
			}
		}

	}
}