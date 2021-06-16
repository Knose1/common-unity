//-///////////////////////////////////////////////////////////-//
//                                                             //
// This script expose utils methode for C# reflextion for tag  //
// effects. It also gets every types that uses the attribute   //
// TextTag on init.                                            //
//                                                             //
//-///////////////////////////////////////////////////////////-//

using Com.GitHub.Knose1.Common.Utils;
using Com.GitHub.Knose1.JuicyText.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;

namespace Com.GitHub.Knose1.JuicyText
{
	public partial class TextEffect : Text
	{
		/// <summary>
		/// The <see cref="TextTagUsage.LetterAdding"/> method name
		/// </summary>
		protected const string LETTER_ADDED_METHOD = "LetterAdded";
		/// <summary>
		/// The <see cref="TextTagUsage.Runtime"/> method name
		/// </summary>
		protected const string UPDATE_METHOD = "Update";


		private static bool _inited = false;
		/// <summary>
		/// If the class has been inited or not
		/// </summary>
		public static bool Inited => _inited;

		private static Type[] _runtimeTags;
		private static Type[] _letterAddingTags;

		/// <summary>
		/// The tags that runs in the quad update
		/// </summary>
		public static Type[] RuntimeTags => _runtimeTags;

		/// <summary>
		/// The tags that runs when a quad is generated
		/// </summary>
		public static Type[] LetterAddingTags => _letterAddingTags;

		/// <summary>
		/// Get the tag types
		/// </summary>
		protected static void InitTags()
		{
			if (_inited) return;

			//Init the lists
			List<Type> runtimeTags = new List<Type>();
			List<Type> letterAddingTags = new List<Type>();

			//Iterate on every types
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			foreach (var type in types)
			{
				//Check if type has our attribute
				TextTagAttribute tagAtt = type.GetCustomAttribute<TextTagAttribute>();
				if (tagAtt is null) continue;

				//Dispatch in one or the two list depending on the tag usage
				if (tagAtt.TextTagUsage.Contains(TextTagUsage.Runtime)) runtimeTags.Add(type);
				if (tagAtt.TextTagUsage.Contains(TextTagUsage.LetterAdding)) letterAddingTags.Add(type);
			}

			//Save the lists
			_runtimeTags = runtimeTags.ToArray();
			_letterAddingTags = letterAddingTags.ToArray();

			_inited = true;
		}

		/// <summary>
		/// Get a <see cref="TextTagUsage.LetterAdding"/> tag handeler.<br/>
		/// Return null if not found.
		/// </summary>
		/// <param name="tagName">Tag to search</param>
		/// <returns></returns>
		protected Type GetTagLetterAddingHandler(string tagName)
		{
			return LetterAddingTags.FirstOrDefault((t) => t.GetCustomAttribute<TextTagAttribute>().TagName == tagName);
		}

		/// <summary>
		/// Get a <see cref="TextTagUsage.Runtime"/> tag handeler.<br/>
		/// Return null if not found.
		/// </summary>
		/// <param name="tagName">Tag to search</param>
		/// <returns></returns>
		protected Type GetTagRuntimeHandler(string tagName)
		{
			return RuntimeTags.FirstOrDefault((t) => t.GetCustomAttribute<TextTagAttribute>().TagName == tagName);
		}

		/// <summary>
		/// Get the update methode of a tag handeler.<br/>
		/// Return null if not found.
		/// </summary>
		/// <param name="type">Tag type</param>
		/// <returns></returns>
		protected MethodInfo GetUpdateMethode(Type type)
		{
			return type.GetMethod(UPDATE_METHOD);
		}

		/// <summary>
		/// Get the LetterAdded methode of a tag handeler.<br/>
		/// Return null if not found.
		/// </summary>
		/// <param name="type">Tag type</param>
		/// <returns></returns>
		protected MethodInfo GetLetterAddedMethode(Type type)
		{
			return type.GetMethod(LETTER_ADDED_METHOD);
		}

		/// <summary>
		/// Return true if the method returns an <see cref="IEnumerator"/>
		/// </summary>
		/// <param name="method">Method</param>
		/// <returns></returns>
		protected bool IsIEnumerator(MethodInfo method)
		{
			return method.ReturnType == typeof(IEnumerator);
		}

		/// <summary>
		/// Return true if the method returns <see cref="void"/>
		/// </summary>
		/// <param name="method">Method</param>
		/// <returns></returns>
		protected bool IsVoid(MethodInfo method)
		{
			return method.ReturnType == typeof(void);
		}

		/// <summary>
		/// Return true if the method returns a <see cref="MeshQuad"/>
		/// </summary>
		/// <param name="method">Method</param>
		/// <returns></returns>
		protected bool IsMeshQuad(MethodInfo method)
		{
			return method.ReturnType == typeof(MeshQuad);
		}
	}
}
