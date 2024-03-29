using Com.GitHub.Knose1.Common.Twinning.Curve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.GitHub.Knose1.Common.Twinning.Curve
{
	/// <summary>
	/// Since the linear curve consist of returning the clamped x value, it's more optimised to use GetLinearValue of this class
	/// </summary>
	public class Linear : Curve
	{
		public Linear()
		{
			minX = 0;
			maxX = 3;

			SetDefaultY();
		}
		protected override float GetCurve(float x)
		{
			return x;
		}

		/// <summary>
		/// Since the linear curve consist of returning the clamped x value, it's more optimised to use this function
		/// </summary>
		public float GetLinearValue(float lerpX)
		{
			if (lerpX < 0) lerpX = 0;
			else if (lerpX > 1) lerpX = 1;

			return lerpX;
		}
	}
}