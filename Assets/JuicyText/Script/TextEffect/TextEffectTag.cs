using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Com.GitHub.Knose1.Common.Utils;
using System;
using System.Reflection;
using Com.GitHub.Knose1.JuicyText.Attributes;

namespace Com.GitHub.Knose1.JuicyText
{
	public partial class TextEffect : Text
	{
		protected const string LETTER_ADDED_METHOD = "LetterAdded";
		protected const string UPDATE_METHOD = "Update";

		private static bool _inited = false;
		public static bool Inited => _inited;

		private static Type[] _runtimeTags;
		private static Type[] _letterAddingTags;

		public static Type[] RuntimeTags => _runtimeTags;
		public static Type[] LetterAddingTags => _letterAddingTags;

		protected static void InitTags()
		{
			if (_inited) return;

			List<Type> runtimeTags = new List<Type>();
			List<Type> letterAddingTags = new List<Type>();


			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			foreach (var type in types)
			{
				TextTagAttribute tagAtt = type.GetCustomAttribute<TextTagAttribute>();
				if (tagAtt is null) continue;

				if (tagAtt.TextTagUsage.Contains(TextTagUsage.Runtime)) runtimeTags.Add(type);
				if (tagAtt.TextTagUsage.Contains(TextTagUsage.LetterAdding)) letterAddingTags.Add(type);
			}

			_runtimeTags = runtimeTags.ToArray();
			_letterAddingTags = letterAddingTags.ToArray();

			_inited = true;
		}

		protected Type GetTagLetterAddingHandler(string tagName)
		{
			return LetterAddingTags.FirstOrDefault((t) => t.GetCustomAttribute<TextTagAttribute>().TagName == tagName);
		}

		protected Type GetTagRuntimeHandler(string tagName)
		{
			return RuntimeTags.FirstOrDefault((t) => t.GetCustomAttribute<TextTagAttribute>().TagName == tagName);
		}

		protected MethodInfo GetUpdateMethode(Type type)
		{
			return type.GetMethod(UPDATE_METHOD);
		}
		
		protected MethodInfo GetLetterAddedMethode(Type type)
		{
			return type.GetMethod(LETTER_ADDED_METHOD);
		}

		protected bool IsIEnumerator(MethodInfo method)
		{
			return method.ReturnType == typeof(IEnumerator);
		}
		
		protected bool IsVoid(MethodInfo method)
		{
			return method.ReturnType == typeof(void);
		}
		
		protected bool IsMeshQuad(MethodInfo method)
		{
			return method.ReturnType == typeof(MeshQuad);
		}
	}
}
