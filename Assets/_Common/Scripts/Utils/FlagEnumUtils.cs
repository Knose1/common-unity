using System;
using System.Collections.Generic;

namespace Com.GitHub.Knose1.Common.Utils
{
	public static class FlagEnumUtils
	{
		public static TEnum ToEnum<TEnum> (ulong value)	where TEnum : Enum => (TEnum)Enum.ToObject(typeof(TEnum), value);
		public static TEnum ToEnum<TEnum> (uint value)	where TEnum : Enum => (TEnum)Enum.ToObject(typeof(TEnum), value);
		public static TEnum ToEnum<TEnum> (ushort value)where TEnum : Enum => (TEnum)Enum.ToObject(typeof(TEnum), value);
		public static TEnum ToEnum<TEnum> (sbyte value)	where TEnum : Enum => (TEnum)Enum.ToObject(typeof(TEnum), value);
		public static TEnum ToEnum<TEnum> (object value)where TEnum : Enum => (TEnum)Enum.ToObject(typeof(TEnum), value);
		public static TEnum ToEnum<TEnum> (long value)	where TEnum : Enum => (TEnum)Enum.ToObject(typeof(TEnum), value);
		public static TEnum ToEnum<TEnum> (int value)	where TEnum : Enum => (TEnum)Enum.ToObject(typeof(TEnum), value);
		public static TEnum ToEnum<TEnum> (byte value)	where TEnum : Enum => (TEnum)Enum.ToObject(typeof(TEnum), value);
		public static TEnum ToEnum<TEnum> (short value)	where TEnum : Enum => (TEnum)Enum.ToObject(typeof(TEnum), value);

		public static void Add<TFlag>(ref this TFlag refFlag, params TFlag[] flagsToAdd) where TFlag : struct, Enum
		{
			CheckAttributeTypeError<TFlag>();
			
			int a = Convert.ToInt32(refFlag);

			for (int i = flagsToAdd.Length - 1; i >= 0; i--)
			{
				int b = Convert.ToInt32(flagsToAdd[i]);
				a |= b;
			}

			refFlag = ToEnum<TFlag>(a);
		}

		public static void Remove<TFlag>(ref this TFlag refFlag, params TFlag[] flagsToRemove) where TFlag : struct, Enum
		{
			CheckAttributeTypeError<TFlag>();
			
			int a = Convert.ToInt32(refFlag);

			for (int i = flagsToRemove.Length - 1; i >= 0; i--)
			{
				int b = Convert.ToInt32(flagsToRemove[i]);
				a ^= b;
			}

			refFlag = ToEnum<TFlag>(a);
		}

		
		public static IEnumerator<TFlag> GetEnumerator<TFlag>(this TFlag flags) where TFlag : Enum
		{
			CheckAttributeTypeError<TFlag>();
			Type t = typeof(TFlag);
			Array a = Enum.GetValues(t);

			foreach (TFlag item in a)
			{
				if (flags.ContainsAll(item))
				{
					yield return item;
				}

			}
		}

		/// <summary>
		/// Return true if flags contains every flagsToTest
		/// <see href="https://stackoverflow.com/questions/52263055/cannot-apply-operator-for-generic-enum-parameters"/>
		/// </summary>
		/// <typeparam name="TFlag"></typeparam>
		/// <param name="flags"></param>
		/// <param name="flagsToTest"></param>
		/// <returns></returns>
		public static bool ContainsAll<TFlag>(this TFlag flags, TFlag flagsToTest) where TFlag : Enum
		{
			CheckAttributeTypeError<TFlag>();
			Check0ValueError(flagsToTest);

			int a = Convert.ToInt32(flags);
			int b = Convert.ToInt32(flagsToTest);
			
			return (a & b) == b;
		}

		/// <summary>
		/// Return true if flags has flagsToTest things in it
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool Contains<TFlag>(this TFlag flags, TFlag flagsToTest) where TFlag : Enum
		{
			CheckAttributeTypeError<TFlag>();
			Check0ValueError(flagsToTest);

			int a = Convert.ToInt32(flags);
			int b = Convert.ToInt32(flagsToTest);

			return (a & b) != 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i">Index</param>
		/// <returns></returns>
		public static int GetAtIndex(int i) => 1 << i;
	
		/* Error */
		private static void CheckAttributeTypeError<TFlag>() where TFlag : Enum
		{
			if (!Attribute.IsDefined(typeof(TFlag), typeof(FlagsAttribute)))
				throw new InvalidOperationException("The given enum type is not decorated with Flag attribute.");
		}
		
		private static void Check0ValueError<TFlag>(TFlag value) where TFlag : Enum
		{
			if (value.Equals(0)) throw new ArgumentOutOfRangeException(nameof(value), "Value must not be 0");
		}
	}
}
