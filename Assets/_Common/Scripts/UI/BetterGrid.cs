///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 06/06/2020 12:19
///-----------------------------------------------------------------
//#define KNOSE_BETTER_GRID_TWINNING

#if KNOSE_BETTER_GRID_TWINNING
using DG.Tweening;
using DG.Tweening.Core;
#endif
using Com.GitHub.Knose1.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace Com.GitHub.Knose1.Common.UI
{
	[RequireComponent(typeof(RectTransform))]
	[ExecuteAlways]
	public class BetterGrid : MonoBetterEditor
	{
		protected const string BETTER_GRID_DEBUG_TAG = "["+nameof(BetterGrid)+"]";

		private const char PATERN_SEPARATOR = '-'; //When modifying this constant, don't forget to update the regex patern
		private const string CHILD_BY_MAIN_AXIS_PATERN_REGEX_CHECK = "\\d-?";

		private int[] childByMainAxiss;
		[SerializeField] string childByMainAxisPatern = "1";
		[SerializeField] int childByMainAxis = 10;
		[SerializeField] int minimumChildSecondAxis = 0;
		[SerializeField] int maximumChildSecondAxis = 10;
		[SerializeField] Vector2 padding = default;
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

#if KNOSE_BETTER_GRID_TWINNING
		private bool isMoving = false;
#endif
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
			minimumChildSecondAxis = Mathf.Clamp(minimumChildSecondAxis, 0, maximumChildSecondAxis);
			maximumChildSecondAxis = Mathf.Max(maximumChildSecondAxis, 1);

			childByMainAxisPatern = Regex.Matches(childByMainAxisPatern, CHILD_BY_MAIN_AXIS_PATERN_REGEX_CHECK).Cast<Match>()
				  .Aggregate("", (s, e) => s + e.Value, s => s);


			if (childByMainAxisPatern == string.Empty) childByMainAxisPatern = "2";
		}
#endif

		protected void Start()
		{
			ComputeChildByMainAxiss();

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
			if (!Application.isPlaying)
			{
				ComputeChildByMainAxiss();
			}

			if (!rectTransform) rectTransform = transform as RectTransform;

			GetSecondAxis(out int length, out int colCount);

			for (int i = 0; i < length; i++)
			{
				RectTransform child = transform.GetChild(i) as RectTransform;

				GetPosFromIndex(i, out int posX, out int posY);
				PosToWorld(posX, posY, out Vector2 min, out Vector2 max);

				//bool canBeActive = posY < (colCount + 1);

				child.sizeDelta = new Vector2(0, 0);
				child.anchoredPosition = new Vector2(0, 0);

				child.anchorMin = min;
				child.anchorMax = max;

				child.pivot = elementsPivot;
			}
		}

		private void ComputeChildByMainAxiss() => childByMainAxiss = childByMainAxisPatern.Split(PATERN_SEPARATOR).Map((string item) => int.Parse(item)).ToArray();

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
			elementCount = Mathf.Min(elementCount, maximumChildSecondAxis);
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

		/// <summary>
		/// Get the position of a child by its index
		/// </summary>
		/// <param name="index">Child index</param>
		/// <param name="posX">X position in the grid</param>
		/// <param name="posY">Y position in the grid</param>
		public void GetPosFromIndex(int index, out int posX, out int posY)
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
			GetSecondAxis(out _, out int colCount);

			colCount = Mathf.Max(minimumChildSecondAxis, colCount); //Base the layout on the minimum child 2nd axis
			unclampedColCount = Mathf.Max(minimumChildSecondAxis, unclampedColCount); //Base the layout on the minimum child 2nd axis

			Vector2Int gridSize = new Vector2Int(childByMainAxis, colCount);
			Vector2Int originalGridSize = gridSize;
			if (startAxis == Axis.Vertical)
				gridSize = new Vector2Int(gridSize.y, gridSize.x);

			GetCellSize(gridSize, out Vector2 size, out Vector2 sizeWithPadding, out Vector2 padding);

			Vector2 startPosition = default;
			Vector2 lDirection = default;

			switch (startCorner)
			{
				case Corner.LowerLeft:
					startPosition.x = 0;
					startPosition.y = 0;

					lDirection.x = 1;
					lDirection.y = 1;
					break;
				case Corner.LowerRight:
					startPosition.x = 1;
					startPosition.y = 0;

					lDirection.x = -1;
					lDirection.y = 1;

					break;
				case Corner.UpperLeft:
					startPosition.x = 0;
					startPosition.y = 1;

					lDirection.x = 1;
					lDirection.y = -1;
					break;
				case Corner.UpperRight:
					startPosition.x = 1;
					startPosition.y = 1;

					lDirection.x = -1;
					lDirection.y = -1;
					break;
			}

			if (startAxis == Axis.Vertical)
			{
				int tempPosX = posX;

				posX = posY;
				posY = tempPosX;
			}

			Vector2 totalPadding = Vector2.one - (originalGridSize * size);

			if (posX != 0) sizeWithPadding.x = size.x + totalPadding.x / (originalGridSize.x - 1);
			if (posY != 0) sizeWithPadding.y = size.y + totalPadding.y / (originalGridSize.y - 1);

			float xMin = lDirection.x * sizeWithPadding.x * posX + startPosition.x;
			float xMax = xMin + lDirection.x * size.x;

			float yMin = lDirection.y * sizeWithPadding.y * posY + startPosition.y;
			float yMax = yMin + lDirection.y * size.y;

			{
				var tempXMin = xMin;
				xMin = Mathf.Min(tempXMin, xMax);
				xMax = Mathf.Max(tempXMin, xMax);

				var tempYMin = yMin;
				yMin = Mathf.Min(tempYMin, yMax);
				yMax = Mathf.Max(tempYMin, yMax);
			}

			Vector2 defaultCenter = new Vector2(0.5f, 0.5f);
			min = new Vector2(xMin, yMin) + gridCenter - defaultCenter;
			max = new Vector2(xMax, yMax) + gridCenter - defaultCenter;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gridSize">the size of the grid</param>
		/// <param name="size">the size of the cell</param>
		/// <param name="sizeWithPadding">size + padding</param>
		private void GetCellSize(Vector2Int gridSize, out Vector2 size, out Vector2 sizeWithPadding, out Vector2 padding)
		{

			size = new Vector2();
			size.x = 1f / gridSize.x;
			size.y = 1f / gridSize.y;

			padding = this.padding * size;
			if (gridSize.y == 1) padding.y = 0;

			sizeWithPadding = new Vector2(size.x, size.y);
			size -= padding;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="posX"></param>
		/// <param name="posY"></param>
		[System.Obsolete]
		public void OldPosToWorld(int posX, int posY, out Vector2 min, out Vector2 max)
		{
			GetUnclampedSecondAxis(out int _, out int unclampedColCount);
			GetSecondAxis(out int __, out int colCount);

			colCount = Mathf.Max(minimumChildSecondAxis, colCount);
			unclampedColCount = Mathf.Max(minimumChildSecondAxis, unclampedColCount);

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

			float padding = this.padding.x;
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
				int startIndex = Mathf.Min(colCount, maximumChildSecondAxis + 2);

				//If there is no maxVisibleCol + 2, then the raw 0 will be the nextVisibleLine
				if (startIndex == maximumChildSecondAxis + 1) startIndex = 0;

				for (int i = 0; i < childByMainAxis; i++)
				{
					nextVisibleLine.Add(transform.GetChild(i + startIndex).gameObject);
				}

				int colCountClamped = Mathf.Min(colCount, maximumChildSecondAxis);

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
						DispatchAddedEvent(child.GetComponentsInChildren<IBetterGridElement>(), i, maximumChildSecondAxis);
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