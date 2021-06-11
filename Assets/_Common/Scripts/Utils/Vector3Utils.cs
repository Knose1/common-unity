using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Com.GitHub.Knose1
{
	public static class Vector3Utils
	{
		private delegate Vector3 MiddleDelegate(params Vector3[] vectors);

		public static List<Vector3> ToVector3List(params Vector3[] v) => v.ToList();

		public static bool Equals(this Vector3 a, Vector3 b, float threshold)
		{
			return (a - b).sqrMagnitude <= threshold * threshold;
		}

		public static bool Contains(this List<Vector3> vector, Vector3 item, float threshold)
		{
			for (int i = vector.Count - 1; i >= 0; i--)
			{
				if (vector[i].Equals(item, threshold)) return true;
			}
			return false;
		}

		public static int IndexOf(this List<Vector3> vector, Vector3 item, float threshold)
		{
			for (int i = vector.Count - 1; i >= 0; i--)
			{
				if (vector[i].Equals(item, threshold)) return i;
			}
			return -1;
		}

		public static Vector3 Sum(this IEnumerable<Vector3> vectors)
		{
			Vector3 toReturn = default;
			using (var enumerator = vectors.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					toReturn += enumerator.Current;
				}
			} 

			return toReturn;
		}

		public static Vector3[] ToLocal(Vector3 pivot, Vector3[] vectors)
		{
			Vector3[] vectorsClone = (Vector3[])vectors.Clone();
			for (int i = vectors.Length - 1; i >= 0; i--)
			{
				vectorsClone[i] = vectorsClone[i] - pivot;
			}

			return vectorsClone;
		}
		
		public static Vector3[] ToGlobal(Vector3 pivot, Vector3[] vectors)
		{
			Vector3[] vectorsClone = (Vector3[])vectors.Clone();
			for (int i = vectors.Length - 1; i >= 0; i--)
			{
				vectorsClone[i] = vectorsClone[i] + pivot;
			}

			return vectorsClone;
		}

		public static Vector3 Middle(params Vector3[] vectors)
		{
			int length = vectors.Length;
			if (length == 0) return Vector3.zero;
			if (length == 1) return vectors[0];

			Vector3 sumVector = Sum(vectors);

			sumVector = new Vector3(sumVector.x/length, sumVector.y/length, sumVector.z/length);

			return sumVector;
		}

		public static Vector3 Multp(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
		}
	}
}
