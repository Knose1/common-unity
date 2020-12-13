
using System.Collections.Generic;
using System.Linq;

namespace Com.GitHub.Knose1.Common.Utils
{
	public static class ListUtils
	{
		public delegate T2 MapDelegate<T, T2>(T input);
		public delegate T2 MapDelegateFull<T, T2>(T input, int index);

		public static List<T> ToList<T>(params T[] i) => i.ToList();

		public static T TryGetOrAddValue<T>(this List<T> list, int i, T defaultValue = default)
		{
			if (list is null) list = new List<T>();

			try
			{
				var _ = list[i];
			}
			catch (System.Exception)
			{
				list.Add(defaultValue);
			}

			return list[i];
		}

		/*----------------------------------*/
		/*           Map delegate           */
		/*----------------------------------*/
		public static IEnumerable<T2> Map<T, T2>(this IEnumerable<T> t, MapDelegate<T, T2> mapper)
		{
			List<T2> toReturn = new List<T2>();
			IEnumerator<T> enumerator = t.GetEnumerator();
			while (enumerator.MoveNext())
			{
				toReturn.Add(mapper(enumerator.Current));
			}
			enumerator.Dispose();

			return toReturn;
		}
		public static List<T2> Map<T, T2>(this List<T> t, MapDelegate<T, T2> mapper)
		{
			List<T2> toReturn = new List<T2>();
			IEnumerator<T> enumerator = t.GetEnumerator();
			while (enumerator.MoveNext())
			{
				toReturn.Add(mapper(enumerator.Current));
			}
			enumerator.Dispose();

			return toReturn;
		}

		/*-----------------------------------*/
		/*         Map delegate full         */
		/*-----------------------------------*/
		public static IEnumerable<T2> Map<T, T2>(this IEnumerable<T> t, MapDelegateFull<T,T2> mapper)
		{
			List<T2> toReturn = new List<T2>();
			IEnumerator<T> enumerator = t.GetEnumerator();

			int index = -1;
			while (enumerator.MoveNext())
			{
				toReturn.Add(mapper(enumerator.Current, ++index));
			}
			enumerator.Dispose();

			return toReturn;
		}

		public static List<T2> Map<T, T2>(this List<T> t, MapDelegateFull<T,T2> mapper)
		{
			List<T2> toReturn = new List<T2>();
			IEnumerator<T> enumerator = t.GetEnumerator();

			int index = -1;
			while (enumerator.MoveNext())
			{
				toReturn.Add(mapper(enumerator.Current, ++index));
			}
			enumerator.Dispose();

			return toReturn;
		}
	}
}