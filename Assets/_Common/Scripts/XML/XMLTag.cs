﻿using UnityEngine;
using System;
using System.Collections.Generic;

namespace Com.GitHub.Knose1.Common.XML
{
	public struct XMLAttribute : IEquatable<XMLAttribute>
	{
		public string name;
		public string value;

		public XMLAttribute(string name, string value)
		{
			this.name = name ?? throw new ArgumentNullException(nameof(name));
			this.value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public static bool operator ==(XMLAttribute left, string right)
		{
			return left.name == right;
		}

		public static bool operator !=(XMLAttribute left, string right)
		{
			return left.name != right;
		}

		public static bool operator ==(string left, XMLAttribute right)
		{
			return left == right.name;
		}

		public static bool operator !=(string left, XMLAttribute right)
		{
			return left != right.name;
		}

		public static bool operator ==(XMLAttribute left, XMLAttribute right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(XMLAttribute left, XMLAttribute right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj) => obj is XMLAttribute attribute && Equals(attribute);
		public bool Equals(XMLAttribute other) => name == other.name;
		public override int GetHashCode() => 363513814 + EqualityComparer<string>.Default.GetHashCode(name);
	}

	public class XMLTag : IEquatable<XMLTag>
	{
		public string name = default;
		public RangeInt tagStart = default;
		public RangeInt tagEnd = default;
		public bool isUnityDefault = default;
		public List<XMLTag> childs = null;
		public List<XMLAttribute> attributes = null;
		
		public string startTagString;
		public string endTagString;

		public XMLTag(string name) { this.name = name; }
		public XMLTag(string name, RangeInt tagStart, RangeInt tagEnd, bool isUnityDefault = false)
		{
			this.name = name;
			this.tagStart = tagStart;
			this.tagEnd = tagEnd;
			this.isUnityDefault = isUnityDefault;
			InitLists();
		}

		/// <summary>
		/// You don't need to use this methode.<br/>
		/// It's called in <see cref="XMLTag(string,RangeInt,RangeInt,bool)"/> constructor but not in <see cref="XMLTag(string)"/>
		/// </summary>
		public void InitLists()
		{
			this.childs = new List<XMLTag>();
			this.attributes = new List<XMLAttribute>();
		}

		/// <summary>
		/// Get if the index is between the start and end tag
		/// </summary>
		/// <param name="index">Index to check</param>
		/// <returns></returns>
		public bool IsInside(int index)
		{
			return tagStart.end < index && index < tagEnd.start; 
		}

		/// <summary>
		/// Get if the index is in the tag declaration
		/// </summary>
		/// <param name="index">Index to check</param>
		/// <returns></returns>
		public bool IsTagDeclaration(int index)
		{
			return tagStart.start <= index && index < tagStart.end || tagEnd.start <= index && index < tagEnd.end; 
		}

		/// <summary>
		/// Get if the index is in the tag declaration
		/// </summary>
		/// <param name="index">Index to check</param>
		/// <param name="isEndTag">
		///		True if the index is present in the end tag.<br/>
		///		False if the index is present in the start tag.
		/// </param>
		/// <returns></returns>
		public bool IsTagDeclaration(int index, out bool isEndTag)
		{
			isEndTag = false;
			return tagStart.start <= index && index < tagStart.end || (isEndTag = tagEnd.start <= index && index < tagEnd.end); 
		}

		/// <summary>
		/// Get the <see cref="XMLTag"/> where the index is in the tag declaration
		/// </summary>
		/// <param name="index">Index to check</param>
		/// <param name="tags">Tag list to iterate</param>
		/// <returns></returns>
		public static XMLTag GetTag(int index, List<XMLTag> tags)
		{
			foreach (var item in tags)
			{
				if (item.IsTagDeclaration(index))
				{
					return item;
				}
			}

			return null;
		}

		/// <summary>
		/// Get the <see cref="XMLTag"/> where the index is in the tag declaration
		/// </summary>
		/// <param name="index">Index to check</param>
		/// <param name="tags">Tag list to iterate</param>
		/// <param name="isEndTag">
		///		True if the index is present in the end tag.<br/>
		///		False if the index is present in the start tag.
		/// </param>
		/// <returns></returns>
		public static XMLTag GetTag(int index, List<XMLTag> tags, out bool isEndTag)
		{
			isEndTag = false;
			foreach (var item in tags)
			{
				if (item.IsTagDeclaration(index, out isEndTag))
				{
					return item;
				}
			}

			return null;
		}

		/// <summary>
		/// Get the tags in which the index is inside.<br/>
		/// In the return value, the index corrispond the the depth.
		/// </summary>
		/// <param name="index">Index to check</param>
		/// <param name="hierarchy">The hierarchy structure</param>
		/// <returns></returns>
		public static List<XMLTag> GetHierarchy(int index, List<XMLTag> hierarchy)
		{
			foreach (var item in hierarchy)
			{
				if (item.IsInside(index))
				{
					return item.GetHierarchy(index);
				}
			}

			return new List<XMLTag>();
		}

		/// <summary>
		/// Get the tags in which the index is inside.<br/>
		/// In the return value, the index corrispond the the depth.<br/>
		/// </summary>
		/// <param name="index">Index to check</param>
		/// <returns></returns>
		public List<XMLTag> GetHierarchy(int index)
		{
			List<XMLTag> toReturn = new List<XMLTag>();

			XMLTag hierarchy = this;

			if (!IsInside(index)) return toReturn;
			
			while (true)
			{
				toReturn.Add(hierarchy);

				bool @continue = false;
				foreach (var item in hierarchy.childs)
				{
					if (item.IsInside(index))
					{
						hierarchy = item;
						@continue = true;
						break;
					}
				}
				if (@continue)
					continue;
				
				return toReturn;
			}
		}

		public override bool Equals(object obj) => obj is XMLTag hierarchy && Equals(hierarchy);
		public bool Equals(XMLTag other) => !(other is null) && name == other.name;
		public override int GetHashCode() => 363513814 + EqualityComparer<string>.Default.GetHashCode(name);

		public static bool operator ==(XMLTag left, XMLTag right)
		{
			if (left is null) return right is null;
			return left.Equals(right);
		}

		public static bool operator !=(XMLTag left, XMLTag right)
		{
			return !(left == right);
		}

		public static implicit operator XMLTag(string name) => new XMLTag(name);
		public static implicit operator string(XMLTag xml) => xml.name;
	}
}