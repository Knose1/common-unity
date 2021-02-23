using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Com.GitHub.Knose1.Common.AnimationUtils
{
	[Serializable]
	public struct AnimatorParameter
	{
		public enum ParameterType
		{
			Float,
			Int,
			Bool,
			Trigger
		}

		[SerializeField] public string paramName;
		[SerializeField] public ParameterType type;

		[SerializeField] public float floatValue;
		[SerializeField] public int intValue;
		[SerializeField] public bool boolValue;

		public AnimatorParameter(string name, ParameterType type) : this()
		{
			this.paramName = name;
			this.type = type;
		}

		public void Call(Animator animator)
		{
			switch (type)
			{
				case ParameterType.Float:
					animator.SetFloat(paramName, floatValue);
					break;
				case ParameterType.Int:
					animator.SetInteger(paramName, intValue);
					break;
				case ParameterType.Bool:
					animator.SetBool(paramName, boolValue);
					break;
				case ParameterType.Trigger:
					animator.SetTrigger(paramName);
					break;
			}
		}
	}

#if UNITY_EDITOR
	// Align drawer
	[UnityEditor.CustomPropertyDrawer(typeof(AnimatorParameter))]
	internal class AnimatorParameterDrawer : UnityEditor.PropertyDrawer
	{
		private const string PROPERTY_NAME = "paramName";
		private const string PROPERTY_TYPE = "type";
		private const string PROPERTY_FLOAT = "floatValue";
		private const string PROPERTY_INT = "intValue";
		private const string PROPERTY_BOOL = "boolValue";

		public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
		{
			float toReturn = base.GetPropertyHeight(property, label);
			if (property.FindPropertyRelative(PROPERTY_TYPE).enumValueIndex == (int)AnimatorParameter.ParameterType.Trigger)
				return toReturn * 2; //We have 2 property to draw

			return toReturn * 3; //We have 3 property to draw
		}

		// Draw the property inside the given rect
		public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			UnityEditor.EditorGUI.BeginProperty(position, label, property);

			// Draw label
			position = UnityEditor.EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			// Don't make child fields be indented
			var lIndent = UnityEditor.EditorGUI.indentLevel;
			UnityEditor.EditorGUI.indentLevel = 0;

			// Calculate rects
			var lTypeProperty = property.FindPropertyRelative(PROPERTY_TYPE);

			Rect propNameRect = position;
			Rect typeRect = position;
			Rect valueRect = position;
			if (lTypeProperty.enumValueIndex == (int)AnimatorParameter.ParameterType.Trigger)
				propNameRect.height = typeRect.height /= 2;
			else
				propNameRect.height = valueRect.height = typeRect.height /= 3;

			typeRect.y  = propNameRect.y + propNameRect.height;
			valueRect.y = typeRect.y + typeRect.height;

			UnityEditor.EditorGUI.PropertyField(propNameRect, property.FindPropertyRelative(PROPERTY_NAME));
			UnityEditor.EditorGUI.PropertyField(typeRect, lTypeProperty);

			switch ((AnimatorParameter.ParameterType)lTypeProperty.enumValueIndex)
			{
				case AnimatorParameter.ParameterType.Float:
					UnityEditor.EditorGUI.PropertyField(valueRect, property.FindPropertyRelative(PROPERTY_FLOAT));
					break;
				case AnimatorParameter.ParameterType.Int:
					UnityEditor.EditorGUI.PropertyField(valueRect, property.FindPropertyRelative(PROPERTY_INT));
					break;
				case AnimatorParameter.ParameterType.Bool:
					UnityEditor.EditorGUI.PropertyField(valueRect, property.FindPropertyRelative(PROPERTY_BOOL));
					break;
			}


			// Set indent back to what it was
			UnityEditor.EditorGUI.indentLevel = lIndent;

			UnityEditor.EditorGUI.EndProperty();
		}
	}
#endif
}
