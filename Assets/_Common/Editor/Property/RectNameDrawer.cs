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
		private const int PADDING = 2;
		private const int MARGIN_BOTTOM = 2;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) * 2 + PADDING + MARGIN_BOTTOM;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.height -= PADDING + MARGIN_BOTTOM;

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
					w = property.FindPropertyRelative("width");
					h = property.FindPropertyRelative("height");
					break;
				default:
					EditorGUI.LabelField(position, nameof(RectNameAttribute) + " only works with Rect and RectInt");
					return;
			}

			EditorGUI.BeginProperty(position, label, property);

			Vector2 size = position.size / 2;
			size.x /= 2;

			Vector2 sizeX = new Vector2(size.x, 0);
			Vector2 moveXY = new Vector2(size.x + 10, size.y + PADDING);

			void BuildSubproperty(Vector2Int fieldPosition, SerializedProperty prop, string rectName)
			{
				GUIContent content = new GUIContent(rectName);
				float labelWidthF = EditorStyles.label.CalcSize(content).x;

				EditorGUIUtility.labelWidth = labelWidthF;
				Vector2 labelWidth = new Vector2(labelWidthF, 0);
				Rect rect = new Rect(sizeX + position.position + fieldPosition * moveXY, size);
				EditorGUI.PropertyField(rect, prop, content);
			}

			EditorGUI.LabelField(new Rect(position.position, size), label);
			BuildSubproperty(new Vector2Int(0,0), x, rectNameAttribute.x);
			BuildSubproperty(new Vector2Int(1,0), y, rectNameAttribute.y);
			BuildSubproperty(new Vector2Int(0,1), w, rectNameAttribute.w);
			BuildSubproperty(new Vector2Int(1,1), h, rectNameAttribute.h);


			EditorGUI.EndProperty();
		}
	}
}