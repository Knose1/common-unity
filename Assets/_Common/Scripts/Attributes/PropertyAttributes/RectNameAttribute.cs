using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.GitHub.Knose1.Common.Attributes.PropertyAttributes
{
	public class RectNameAttribute : PropertyAttribute 
	{
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
		public int[] displayOrder = new int[] {0,1,2,3};
	}
}