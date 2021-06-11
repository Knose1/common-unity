using System.Collections.Generic;
using System;
using System.Linq;
using Com.GitHub.Knose1.Common.Utils;

namespace Com.GitHub.Knose1.Editor.PolyGenerator
{
	[Serializable]
	public struct Line : IEquatable<Line>
	{
		public int p1;
		public int p2;

		public Line(int p1, int p2)
		{
			this.p1 = p1;
			this.p2 = p2;
		}

		public override bool Equals(object obj)
		{
			return obj is Line line && Equals(line);
		}

		public bool Equals(Line other)
		{
			return (p1 == other.p1 && p2 == other.p2) || (p1 == other.p2 && p2 == other.p1);
		}

		public override int GetHashCode()
		{
			var hashCode = 1369944177;
			hashCode = hashCode * -1521134295 + p1.GetHashCode();
			hashCode = hashCode * -1521134295 + p2.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(Line left, Line right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Line left, Line right)
		{
			return !(left == right);
		}

		public static List<int> LineListToIndex(List<Line> lines, List<int> toAdd)
		{
			List<int> toReturn = new List<int>();
			foreach (var line in lines)
			{
				if (!toReturn.Contains(line.p1)) toReturn.Add(line.p1);
				if (!toReturn.Contains(line.p2)) toReturn.Add(line.p2);
			}

			foreach (var item in toAdd)
			{
				if (!toReturn.Contains(item)) toReturn.Add(item);
			}

			toReturn.Sort();
			return toReturn;
		}
	}
}
