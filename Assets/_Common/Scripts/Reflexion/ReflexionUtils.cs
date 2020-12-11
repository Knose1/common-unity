using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Com.GitHub.Knose1.Common.Reflexion
{
	public static class ReflexionUtils
	{
		/// <summary>
		/// Return the MethodInfo of the delegate
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static MethodInfo GetDelegateMethodeDelcaration<T>() where T : Delegate
		{
			return typeof(T).GetMethod("Invoke");
		}

		public static bool IsRef(this ParameterInfo parameterInfo) => parameterInfo.ParameterType.IsByRef && !parameterInfo.IsOut;

		public static List<Type> GetParents(Type type)
		{
			Type maxClass = typeof(MonoBetterEditor);

			List<Type> types = new List<Type>();
			Type baseTp = type.BaseType;
			while (baseTp != null && baseTp.IsSubclassOf(maxClass))
			{
				types.Add(baseTp);
				baseTp = baseTp.BaseType;
			}

			return types;
		}
		public static FieldInfo GetField(Type type, string propertyName, BindingFlags bindingAttr)
		{
			FieldInfo info = type.GetField(propertyName, bindingAttr);
			if (info == null)
			{
				List<Type> parents = GetParents(type);
				foreach (var parent in parents)
				{
					info = parent.GetField(propertyName, bindingAttr);
					if (info != null) return info;
				}
			}

			return info;
		}
	}
}
