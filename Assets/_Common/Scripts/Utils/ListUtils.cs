
using System;
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
		/*                Sum               */
		/*----------------------------------*/
		public static int SumFromTo(this IEnumerable<int> t, int from = 0, int to = -1)
		{
			if (to < 0)
			{
				to = t.Count() - 1;
			}

			int toReturn = 0;
			IEnumerator<int> enumerator = t.GetEnumerator();

			for (int i = from; i <= to; i++)
			{
				enumerator.MoveNext();
				toReturn += enumerator.Current;
			}

			enumerator.Dispose();

			return toReturn;
		}

		/*----------------------------------*/
		/*           Map delegate           */
		/*----------------------------------*/
		public static IEnumerable<T2> Map<T, T2>(this IEnumerable<T> t, Func<T, T2> mapper)
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
		public static List<T2> Map<T, T2>(this List<T> t, Func<T, T2> mapper)
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
		public static IEnumerable<T2> Map<T, T2>(this IEnumerable<T> t, Func<T,int,T2> mapper)
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

		public static List<T2> Map<T, T2>(this List<T> t, Func<T,int,T2> mapper)
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

		

		/*-----------------------------------*/
		/*               Join                */
		/*-----------------------------------*/
		public static string ToJoinString<T>(this IEnumerable<T> t, string joinString = ",")
		{
			string toReturn = "";
			IEnumerator<T> enumerator = t.GetEnumerator();

			while (enumerator.MoveNext())
			{
				toReturn += enumerator.Current.ToString() + joinString;
			}
			enumerator.Dispose();

			return toReturn.Substring(toReturn.Length - joinString.Length, joinString.Length);
		}

		public static string ToJoinString<T>(this IEnumerable<T> t, Func<T, string> toStringFunc, string joinString = ",")
		{
			string toReturn = "";
			IEnumerator<T> enumerator = t.GetEnumerator();

			while (enumerator.MoveNext())
			{
				toReturn += toStringFunc(enumerator.Current) + joinString;
			}
			enumerator.Dispose();

			return toReturn.Substring(toReturn.Length - joinString.Length, joinString.Length);
		}

		public static string ToJoinString<T>(this IEnumerable<T> t, Func<T, int, string> toStringFunc, string joinString = ",")
		{
			string toReturn = "";
			IEnumerator<T> enumerator = t.GetEnumerator();

			int index = -1;
			while (enumerator.MoveNext())
			{
				index += 1;
				toReturn += toStringFunc(enumerator.Current, index) + joinString;
			}
			enumerator.Dispose();

			return toReturn.Substring(toReturn.Length - joinString.Length, joinString.Length);
		}


	}
}