using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.GitHub.Knose1.Common.Attributes.PropertyAttributes
{
	public class RectNameAttribute : PropertyAttribute 
	{
		private const string LOG_PREFIX = "["+nameof(RectNameAttribute)+"]";

		public const int X_INDEX = 0;
		public const int Y_INDEX = 1;
		public const int W_INDEX = 2;
		public const int H_INDEX = 3;

		/// <summary>
		/// Name for x (display index : 0)
		/// </summary>
		public string x = "X";

		/// <summary>
		/// Name for y (display index : 1)
		/// </summary>
		public string y = "Y";

		/// <summary>
		/// Name for w (display index : 2)
		/// </summary>
		public string w = "W";

		/// <summary>
		/// Name for h (display index : 3)
		/// </summary>
		public string h = "H";

		/// <summary>
		/// The display order of the fields<br/>
		/// <br/>
		/// Default :<br/>
		/// 0 1<br/>
		/// 2 3
		/// </summary>
		public int[] displayOrder = GetDefaultOrder();

		public static int[] GetDefaultOrder() => new int[] { 0, 1, 2, 3 };
		
		public void CheckDisplayOrder()
		{
			if (displayOrder.Length != 4)
			{
				Debug.LogWarning(LOG_PREFIX+" "+nameof(displayOrder)+"'s length is not 4");
				displayOrder = GetDefaultOrder();
				return;
			}

			List<int> notInList = new List<int>();
			List<int> emptyOrderIndex = new List<int>() {0,1,2,3};
			List<int> displayOrderList = new List<int>(displayOrder);

			for (int i = 0; i <= 3; i++)
			{
				int index = displayOrderList.IndexOf(i);
				if (index == -1)
				{
					Debug.LogWarning(LOG_PREFIX+" "+i+" is not in "+nameof(displayOrder));
					notInList.Add(i);
					continue;
				}

				emptyOrderIndex.Remove(i);

			}

			foreach (var item in notInList)
			{
				displayOrder[emptyOrderIndex[0]] = item;
				emptyOrderIndex.RemoveAt(0);
			}
		}

		public Vector2Int GetPosition(int index)
		{
			List<int> displayOrderList = new List<int>(displayOrder);
			int indexOfI = displayOrderList.IndexOf(index);
			
			return new Vector2Int(indexOfI % 2, indexOfI / 2);
		}
	}
}