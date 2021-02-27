using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.GitHub.Knose1.Common
{
	public class HardReference<T> where T : struct
	{
		internal T _value;
		public HardReference() : this(default) {}
		public HardReference(T value) => this._value = value;
		public void Set(T value) => this._value = value;
		public T Get() => _value;

		public static implicit operator T(HardReference<T> reference) => reference._value;
	}

	public static class HardReferenceHelper
	{
		public static List<T> Convert<T>(this List<HardReference<T>> reference) where T : struct
		{
			List<T> toReturn = new List<T>();
			using (var enumerator = reference.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					toReturn.Add(enumerator.Current);
				}
			}

			return toReturn;
		}

		public static T[] Convert<T>(this HardReference<T>[] reference) where T : struct
		{
			List<T> toReturn = new List<T>();
			var enumerator = reference.GetEnumerator();

			while (enumerator.MoveNext())
			{
				toReturn.Add((enumerator.Current as HardReference<T>)._value);
			}

			return toReturn.ToArray();
		}
	}
}
