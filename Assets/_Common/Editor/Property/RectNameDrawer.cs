using Com.GitHub.Knose1.Common.Attributes.PropertyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Com.GitHub.Knose1.Common.Editor.Property
{
	[CustomPropertyDrawer(typeof(RectNameAttribute))]
	public class RectNameDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			RectNameAttribute rectNameAttribute = attribute as RectNameAttribute;

			SerializedPropertyType propertyType = property.propertyType;

			SerializedProperty x,y,w,h = null;

			bool isInt = false;
			switch (propertyType)
			{
				case SerializedPropertyType.Rect:
				case SerializedPropertyType.RectInt:
					isInt = propertyType == SerializedPropertyType.RectInt;

					x = property.FindPropertyRelative("x");
					y = property.FindPropertyRelative("y");
					w = property.FindPropertyRelative("w");
					h = property.FindPropertyRelative("h");
					break;
				default:
					GUILayout.Label(nameof(RectNameAttribute)+" only works with Rect and RectInt");
					break;
			}
		}
	}
}