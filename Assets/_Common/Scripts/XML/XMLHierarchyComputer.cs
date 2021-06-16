using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Com.GitHub.Knose1.Common.XML
{

	/// <summary>
	/// Computes the XML Hierarchy
	/// </summary>
	public class XMLHierarchyComputer
	{
		public readonly string[] UNITY_BUILT_IN = new string[] { "b", "i", "size", "color", "material"};

		/// <summary>
		/// The text to compute
		/// </summary>
		public string text;
		/// <summary>
		/// The text without the XML tags
		/// </summary>
		public string UnTagedText => XMLReader.tagFind.Replace(text, "");

		private List<XMLTag> _xmlHierarchy = new List<XMLTag>();
		public List<XMLTag> XMLHierarchy => _xmlHierarchy;

		private List<XMLTag> _tagList = new List<XMLTag>();
		public List<XMLTag> TagList => _tagList;

		public XMLHierarchyComputer(string text) 
		{
			this.text = text;
		}

		/// <summary>
		/// Get the XML hierarchy
		/// </summary>
		public void Compute()
		{
			_xmlHierarchy.Clear();
			_tagList.Clear();

			//Get the XML
			XMLReader xl = new XMLReader(text);
			
			List<XMLTag> unCompletedXML = new List<XMLTag>();
			while (xl.Next())
			{
				while (ComputeXL(xl, ref unCompletedXML)) { }
			}

		}

		private bool ComputeXL(XMLReader xl, ref List<XMLTag> unCompletedXML)
		{
			bool isStartElement = xl.IsStartElement;
			
			string name = xl.Name;
			
			if (isStartElement)
			{
				XMLTag balise = new XMLTag(name, new RangeInt(xl.Index, 0), default, UNITY_BUILT_IN.Contains(name));
				balise.startTagString = xl.Raw;

				if (unCompletedXML.Count > 0) unCompletedXML.Last().childs.Add(balise);
				unCompletedXML.Add(balise);

				int attributesCount = xl.ArgCount;
				for (int i = 0; i < attributesCount; i++)
				{
					xl.MoveToAttribute(i);
					balise.attributes.Add(new XMLAttribute(xl.Name, xl.Value));
				}
				xl.Next();
				balise.tagStart.length = xl.Index - balise.tagStart.start;


			}
			else
			{
				int index = unCompletedXML.FindLastIndex( (x) => x == name) ;
				if (index == -1)
				{
					xl.Next();
					return xl.Name != "";
				}

				var balise = unCompletedXML[index];
				balise.endTagString = xl.Raw;
				balise.tagEnd.start = xl.Index;
				xl.Next();
				balise.tagEnd.length = xl.Index - balise.tagEnd.start;

				unCompletedXML.RemoveAt(index);
				_tagList.Add(balise);

				if (unCompletedXML.Count == 0)
					_xmlHierarchy.Add(balise);
			}

			return xl.Name != "";
		}
	}
}