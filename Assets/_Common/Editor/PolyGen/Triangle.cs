using System.Collections.Generic;
using System;
using Com.GitHub.Knose1.Common.Utils;

namespace Com.GitHub.Knose1.Editor.PolyGenerator
{
	[Serializable]
	public struct Triangle : IEquatable<Triangle>
	{
		public int p1;
		public int p2;
		public int p3;
		public bool drawDebug;

		public Triangle(int p1, int p2, int p3, bool drawDebug=true)
		{
			this.p1 = p1;
			this.p2 = p2;
			this.p3 = p3;
			this.drawDebug = drawDebug;
		}

		public static List<Triangle> MakeListFrom(List<int> list)
		{
			List<Triangle> toReturn = new List<Triangle>();

			int count = list.Count;
			for (int i = 0; i < count; i += 3)
			{
				if (count - i < 3) break;
				toReturn.Add(new Triangle(list[i], list[i + 1], list[i + 2]));
			}

			return toReturn;
		}

		public static List<int> TriangleListToIndex(List<Triangle> triangles)
		{
			List<int> toReturn = new List<int>();
			foreach (var triangle in triangles)
			{
				toReturn.Add(triangle.p1);
				toReturn.Add(triangle.p2);
				toReturn.Add(triangle.p3);
			}

			return toReturn;
		}

		//EQUALS ////////////////////////////////////////////
		public override bool Equals(object obj) => obj is Triangle triangle && Equals(triangle);
		public bool Equals(Triangle other)
		{
			return p1 == other.p1 && p2 == other.p2 && p3 == other.p3 // strictEqual 
				|| p1 == other.p2 && p2 == other.p3 && p3 == other.p1 // clockEqual1
				|| p1 == other.p3 && p2 == other.p1 && p3 == other.p2 // clockEqual2
				|| p1 == other.p1 && p2 == other.p3 && p3 == other.p2 // counterClockEqual1
				|| p1 == other.p2 && p2 == other.p1 && p3 == other.p3 // counterClockEqual2
				|| p1 == other.p3 && p2 == other.p2 && p3 == other.p1 // counterClockEqual3
				;
		}

		public override int GetHashCode()
		{
			var hashCode = -901473683;
			hashCode = hashCode * -1521134295 + p1.GetHashCode();
			hashCode = hashCode * -1521134295 + p2.GetHashCode();
			hashCode = hashCode * -1521134295 + p3.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(Triangle left, Triangle right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Triangle left, Triangle right)
		{
			return !(left == right);
		}
	}
}
