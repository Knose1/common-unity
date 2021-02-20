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
			rectNameAttribute.CheckDisplayOrder();

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

			void BuildSubproperty(int index, SerializedProperty prop, string rectName)
			{
				Vector2Int fieldPosition = rectNameAttribute.GetPosition(index);

				GUIContent content = new GUIContent(rectName);
				float labelWidthF = EditorStyles.label.CalcSize(content).x;

				EditorGUIUtility.labelWidth = labelWidthF;
				Vector2 labelWidth = new Vector2(labelWidthF, 0);
				Rect rect = new Rect(sizeX + position.position + fieldPosition * moveXY, size);
				EditorGUI.PropertyField(rect, prop, content);
			}

			EditorGUI.LabelField(new Rect(position.position, size), label);
			BuildSubproperty(RectNameAttribute.X_INDEX, x, rectNameAttribute.x);
			BuildSubproperty(RectNameAttribute.Y_INDEX, y, rectNameAttribute.y);
			BuildSubproperty(RectNameAttribute.W_INDEX, w, rectNameAttribute.w);
			BuildSubproperty(RectNameAttribute.H_INDEX, h, rectNameAttribute.h);


			EditorGUI.EndProperty();
		}
	}
}