using Com.Github.Knose1.Common.Twinning.Curve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Github.Knose1.Common.Twinning.Curve
{
	public class Exp : Curve
	{
		public Exp(float maxX = 10)
		{
			minX = 0;
			this.maxX = maxX;

			SetDefaultY();
		}

		protected override float GetCurve(float x)
		{
			return (float)Math.Exp(x);
		}
	}
}