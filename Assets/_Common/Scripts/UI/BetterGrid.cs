///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 06/06/2020 12:19
///-----------------------------------------------------------------
//#define DEBUGME

using Com.GitHub.Knose1.Common.Attributes.PropertyAttributes;
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
	public class BetterGrid : MonoBehaviour
	{
		protected const string BETTER_GRID_DEBUG_TAG = "["+nameof(BetterGrid)+"]";

		private const char PATERN_SEPARATOR = '-'; //When modifying this constant, don't forget to update the regex patern
		private const string CHILD_BY_MAIN_AXIS_PATERN_REGEX_CHECK = "\\d-?";

		[System.Serializable]
		internal struct Align
		{
			public enum AlignVertical
			{
				Default = 0,
				Top = 1,
				Bottom = 2,
				Center = 3
			}
			public enum AlignHorizontal
			{
				Default = 0,
				Left = 1,
				Right = 2,
				Center = 3
			}

			[SerializeField] public int align;
			public Align(int align) => this.align = align;

			public static implicit operator int(Align a) => a.align;
		}

		/// <summary>
		/// COMPUTED The number of child for each line/column (depend on the axis)
		/// </summary>
		protected int[] childByMainAxiss;

		/// <summary>
		/// childByMainAxiss.Min()
		/// </summary>
		protected int minChildByMainAxis = 10;

		/// <summary>
		/// childByMainAxiss.Max()
		/// </summary>
		protected int maxChildByMainAxis = 10;

		/// <summary>
		/// The number of children missing on the last line to make a full line
		/// </summary>
		protected int missingChildrenOnLastLine = 0;
		
		[Header("Child Number")]
		[SerializeField, Tooltip("The number of child for each line/column (depend on the axis). The patern loops.")] string childByMainAxisPatern = "1";
		[SerializeField, Tooltip("The minimum column/line to show (depends on the 2nd axis).")] int minimumChildSecondAxis = 0;
		[SerializeField, Tooltip("The maximum column/line to show (depends on the 2nd axis).")] int maximumChildSecondAxis = 10;

		[Header("Shape")]
		[SerializeField, Tooltip("Child Alignment.")] Align mainAxisAlign = default;
		[SerializeField, Tooltip("The corner in which the first child is placed.")] Corner startCorner = default;
		[SerializeField, Tooltip("The start axis, the 2nd axis will be its opposite.")] Axis startAxis = default;

		[Header("Positioning")]
		[SerializeField, Tooltip("The spacing between childs in child size (0.25 means that the original size is divided by 4).")] Vector2 padding = default;
		[SerializeField, Tooltip("The center of the grid based on the anchors.")] Vector2 gridCenter = new Vector2(0.5f, 0.5f);

		[SerializeField,
		 Tooltip("The spacing between childs in anchor ratio. 0.5 means that layout will take half the available space."), 
		 RectName(x = "left", y = "bottom", w = "right", h = "top", displayOrder = new int[]{0,2,1,3})
		]
		Rect margin = default;

		public List<int> ChildByMainAxiss => childByMainAxiss.ToList();
		public int MinimumChildSecondAxis => minimumChildSecondAxis;
		public int MaximumChildSecondAxis => maximumChildSecondAxis;
		public int MainAxisAlign => mainAxisAlign;
		public Corner StartCorner => startCorner;
		public Axis StartAxis => startAxis;
		public Vector2 Padding => padding;
		public Vector2 GridCenter => gridCenter;
		public float MarginLeft => margin.x;
		public float MarginBottom => margin.y;
		public float MarginRight => margin.width;
		public float MarginTop => margin.height;

#if UNITY_EDITOR && DEBUGME
		[Header("Debug")]
		[SerializeField] Vector2Int posToIndex = default;
		[SerializeField] int posToIndexIndex = default;
		[Space()]
		[SerializeField] int indexToPos = default;
		[SerializeField] Vector2Int indexToPosPos = default;
#endif
		protected RectTransform rectTransform;


#if UNITY_EDITOR
		private void OnValidate()
		{
			//childByMainAxis = Mathf.Max(childByMainAxis, 1);

			minimumChildSecondAxis = Mathf.Clamp(minimumChildSecondAxis, 0, maximumChildSecondAxis);
			maximumChildSecondAxis = Mathf.Max(maximumChildSecondAxis, 2);

			childByMainAxisPatern = Regex.Matches(childByMainAxisPatern, CHILD_BY_MAIN_AXIS_PATERN_REGEX_CHECK).Cast<Match>()
				  .Aggregate("", (s, e) => s + e.Value, s => s);


			if (childByMainAxisPatern == string.Empty) childByMainAxisPatern = "2";
			


			margin.x		= Mathf.Clamp(margin.x		, 0, 0.5f);
			margin.y		= Mathf.Clamp(margin.y		, 0, 0.5f);
			margin.width	= Mathf.Clamp(margin.width	, 0, 0.5f);
			margin.height	= Mathf.Clamp(margin.height	, 0, 0.5f);

#if DEBUGME
			posToIndexIndex = GetIndexAt(posToIndex.x, posToIndex.y);

			GetPosFromIndex(indexToPos, out int posX, out int posY);
			indexToPosPos = new Vector2Int(posX, posY);
#endif
		}
#endif

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

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
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				ComputeChildByMainAxiss();
				if (!rectTransform) rectTransform = transform as RectTransform;
			}
#endif


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
			}
		}

		/// <summary>
		/// Compute the variable childByMainAxiss
		/// </summary>
		private void ComputeChildByMainAxiss()
		{
			string patern = childByMainAxisPatern;
			if (patern[patern.Length - 1] == PATERN_SEPARATOR) patern = patern.Remove(patern.Length - 1);

			childByMainAxiss = patern
				.Split(PATERN_SEPARATOR)					//Split the string
				.Map((string item) => int.Parse(item))		//from string[] to int[]
				.Where((int i) => i != 0)					//Remove the 0
				.ToArray();                                 //ToArray

			if (childByMainAxiss.Length == 0) childByMainAxiss = new int[] { 1 };

			minChildByMainAxis = childByMainAxiss.Min();
			maxChildByMainAxis = childByMainAxiss.Max();
		}

		/// <summary>
		/// Get the total number of Y axis
		/// </summary>
		/// <param name="length">The number of child</param>
		/// <param name="columnCount">The number of column</param>
		public void GetUnclampedSecondAxis(out int length, out int columnCount)
		{
			length = transform.childCount;
			
			GetPosFromIndex(length, out int posX, out columnCount);

			//NumberOfMissingChild = childsOnTheRow - numberOfChildsOnTheRow
			missingChildrenOnLastLine = GetChildsOnRow(columnCount) - (posX + 1); //+1 since posX starts at 0

			//Since posY starts at 0;
			columnCount += 1;
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
			int index = GetIndexAt(x, y);
			if (index < 0) return null;

			return transform.GetChild(index).gameObject;
		}

		/// <summary>
		/// Get the index of a tile at a certain position of the grid
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int GetIndexAt(int x, int y)
		{
			GetUnclampedSecondAxis(out _, out int colCount);

			int xMax = GetChildsOnRow(y);

			if (x < 0 || y < 0 || x >= xMax || y >= colCount)
			{
				Debug.LogWarning(BETTER_GRID_DEBUG_TAG + " You are trying to get a child out of bounds");
				return -1;
			}

			// if y = 4 and x = 0
			//
			// y | count	| indexes
			//   |			|
			// 0 | 5		| 00 01 02 03 04
			// 1 | 4		| 05 06 07 08
			// 2 | 6		| 09 10 11 12 13 14
			// 3 | 5		| 15 16 17 18 19 
			// 4 | 4		| 20 21 22 23
			// 5 | 6		| 24 25 26 27 28 29
			//
			// index = 5 + 4 + 6 + 5 + x

			int index;
			if (y == 0)
			{
				index = x;
			}
			else
			{
				int childAxiss = childByMainAxiss.Length; //3 (5 and 4 and 6)

				//If it's 1 or 2 (in our example) set to 0
				int totalSum = 0;
				if (y >= childAxiss) totalSum = childByMainAxiss.SumFromTo();

				//y / childAxiss means "how many time do we see '5,4,6' before arriving to y
				//
				//In our example we see it 1 time
				//
				// 5 4 6
				// 5
				//
				// 
				int paternRepeatSum = totalSum * ((y - 1) / childAxiss);
				index = childByMainAxiss.SumFromTo(to: (y - 1) % childAxiss) + paternRepeatSum + x;
			}

			return index;
		}

		public int GetChildsOnRow(int y) => childByMainAxiss[y % childByMainAxiss.Length];

		/// <summary>
		/// Get the position of a child by its index
		/// </summary>
		/// <param name="index">Child index</param>
		/// <param name="posX">X position in the grid</param>
		/// <param name="posY">Y position in the grid</param>
		public void GetPosFromIndex(int index, out int posX, out int posY)
		{
			posY = 0;

			int childByMainAxissLength = childByMainAxiss.Length;
			int elementsNotInGrid = index;
			int i = 0;
			while (elementsNotInGrid > 0)
			{
				int childByMainAxis = childByMainAxiss[i];
				if (elementsNotInGrid >= childByMainAxis)
				{
					posY += 1;
					elementsNotInGrid -= childByMainAxis;
				}
				else break;

				i += 1;
				i %= childByMainAxissLength;
			}

			posX = elementsNotInGrid;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="posX"></param>
		/// <param name="posY"></param>
		public void PosToWorld(int posX, int posY, out Vector2 min, out Vector2 max)
		{
			GetUnclampedSecondAxis(out _, out int unclampedColCount);
			GetSecondAxis(out _, out int colCount);

			colCount = Mathf.Max(minimumChildSecondAxis, colCount); //Base the layout on the minimum child 2nd axis
			//unclampedColCount = Mathf.Max(minimumChildSecondAxis, unclampedColCount); //Base the layout on the minimum child 2nd axis

			//Get the grid size
			Vector2Int gridSize = new Vector2Int(maxChildByMainAxis, colCount);
			
			//Exchange the size if vertical
			if (startAxis == Axis.Vertical)
				gridSize = new Vector2Int(gridSize.y, gridSize.x);

			//Get the startPosition of the layout and the direction lerping
			Vector2 startPosition = default;
			Vector2 direction = default;

			switch (startCorner)
			{
				case Corner.LowerLeft:
					startPosition.x = 0;
					startPosition.y = 0;

					direction.x = 1;
					direction.y = 1;
					break;
				case Corner.LowerRight:
					startPosition.x = 1;
					startPosition.y = 0;

					direction.x = -1;
					direction.y = 1;

					break;
				case Corner.UpperLeft:
					startPosition.x = 0;
					startPosition.y = 1;

					direction.x = 1;
					direction.y = -1;
					break;
				case Corner.UpperRight:
					startPosition.x = 1;
					startPosition.y = 1;

					direction.x = -1;
					direction.y = -1;
					break;
			}

			if (startAxis == Axis.Vertical)
			{
				int tempPosX = posX;
				posX = posY;
				posY = tempPosX;
			}

			//Get the Cell size
			GetCellSize(new Vector2Int(posX, posY), gridSize, out Vector2 size, out Vector2 sizeWithPadding, out Vector2 padding);

			float xMin = direction.x * sizeWithPadding.x * posX + startPosition.x;
			float xMax = xMin + direction.x * size.x;

			float yMin = direction.y * sizeWithPadding.y * posY + startPosition.y;
			float yMax = yMin + direction.y * size.y;

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


			if (startAxis == Axis.Vertical)
			{
				int missingChildrenOnLine;
				if (posX + 1 == unclampedColCount)
					missingChildrenOnLine = missingChildrenOnLastLine;
				else
					missingChildrenOnLine = maxChildByMainAxis - GetChildsOnRow(posX);

				float spacing = (padding.y + size.y) * missingChildrenOnLine;
				switch (mainAxisAlign)
				{
					case (int)Align.AlignVertical.Default: break;
						
					case (int)Align.AlignVertical.Bottom:
						if (startCorner == Corner.LowerLeft || startCorner == Corner.LowerRight) break;
						min.y -= spacing;
						max.y -= spacing;
						break;
					case (int)Align.AlignVertical.Top:
						if (startCorner == Corner.UpperLeft || startCorner == Corner.UpperRight) break;
						min.y += spacing;
						max.y += spacing;
						break;
					case (int)Align.AlignVertical.Center:
						min.y += spacing / 2 * direction.y;
						max.y += spacing / 2 * direction.y;
						break;
				}
			}
			else
			{
				int missingChildrenOnLine;
				if (posY + 1 == unclampedColCount)
					missingChildrenOnLine = missingChildrenOnLastLine;
				else
					missingChildrenOnLine = maxChildByMainAxis - GetChildsOnRow(posY);

				float spacing = (padding.x + size.x) * missingChildrenOnLine;
				switch (mainAxisAlign)
				{
					case (int)Align.AlignHorizontal.Default: break;

					case (int)Align.AlignHorizontal.Left:
						if (startCorner == Corner.LowerLeft || startCorner == Corner.UpperLeft) break;
						min.x -= spacing;
						max.x -= spacing;
						break;
					case (int)Align.AlignHorizontal.Right:
						if (startCorner == Corner.LowerRight || startCorner == Corner.UpperRight) break;
						min.x += spacing;
						max.x += spacing;
						break;
					case (int)Align.AlignHorizontal.Center:
						min.x += spacing / 2 * direction.x;
						max.x += spacing / 2 * direction.x;
						break;
				}
			}

			ComputeMargin(ref min, ref max);
		}

		/// <summary>
		/// Compute the margin of the layout
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		protected void ComputeMargin(ref Vector2 min, ref Vector2 max)
		{
			Rect gridRect = new Rect(0,0,1,1);
			Vector2 size = max - min;

			gridRect.min = margin.position;
			gridRect.max = Vector2.one - margin.size;

			min = min * gridRect.size + gridRect.position;
			size *= gridRect.size;

			max = min + size;
		}

		/// <summary>
		/// Get the size of a cell
		/// </summary>
		/// <param name="gridSize">the size of the grid</param>
		/// <param name="size">the size of the cell</param>
		/// <param name="sizeWithPadding">size + padding</param>
		protected void GetCellSize(Vector2Int pos, Vector2Int gridSize, out Vector2 size, out Vector2 sizeWithPadding, out Vector2 padding)
		{
			size = new Vector2
			{
				x = 1f / gridSize.x,
				y = 1f / gridSize.y
			};

			//Make the padding relative to the element size (so 0.5 means that the size is 0)
			padding = this.padding * size;
			if (gridSize.y == 1) padding.y = 0; //If there is only one y element, there is no padding
			if (gridSize.x == 1) padding.x = 0; //If there is only one x element, there is no padding

			sizeWithPadding = new Vector2(size.x, size.y);
			size -= padding;

			//Doesn't count the margin
			Vector2 emptySpace = Vector2.one - (gridSize * size);

			// The padding is a left padding. So don't count it when pos is 0
			// 
			// padding = emptySpace / (childrenCount - 1)
			//
			// (childrenCount - 1) is the spacing between each elements
			//
			padding.x = emptySpace.x / (gridSize.x - 1);
			padding.y = emptySpace.y / (gridSize.y - 1);
			if (pos.x != 0) sizeWithPadding.x = size.x + padding.x;
			if (pos.y != 0) sizeWithPadding.y = size.y + padding.y;
		}



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

#if UNITY_EDITOR
	// Align drawer
	[UnityEditor.CustomPropertyDrawer(typeof(BetterGrid.Align))]
	internal class AlignDrawer : UnityEditor.PropertyDrawer
	{

		// Draw the property inside the given rect
		public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			UnityEditor.EditorGUI.BeginProperty(position, label, property);

			// Draw label
			position = UnityEditor.EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			// Don't make child fields be indented
			var indent = UnityEditor.EditorGUI.indentLevel;
			UnityEditor.EditorGUI.indentLevel = 0;

			// Calculate rects
			var prop = property.FindPropertyRelative("align");
			BetterGrid bg = property.serializedObject.targetObject as BetterGrid;
			bool isVertical = bg.StartAxis == Axis.Vertical;

			if (isVertical)
			{
				prop.intValue = Convert.ToInt32(UnityEditor.EditorGUI.EnumPopup(position, GUIContent.none, (BetterGrid.Align.AlignVertical)prop.intValue));
			}
			else
			{
				prop.intValue = Convert.ToInt32(UnityEditor.EditorGUI.EnumPopup(position, GUIContent.none, (BetterGrid.Align.AlignHorizontal)prop.intValue));
			}

			// Set indent back to what it was
			UnityEditor.EditorGUI.indentLevel = indent;

			UnityEditor.EditorGUI.EndProperty();
		}
	}
#endif
}