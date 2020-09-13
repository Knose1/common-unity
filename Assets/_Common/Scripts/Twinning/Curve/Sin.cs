using Com.GitHub.Knose1.Common.Twinning.Curve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.GitHub.Knose1.Common.Twinning.Curve
{
	public class Sin : Curve
	{
		public Sin()
		{
			minX = 0;
			maxX = (float)Math.PI / 2;

			SetDefaultY();
		}

		protected override float GetCurve(float x)
		{
			return (float)Math.Sin(x + Math.PI / 2);
		}
	}
}