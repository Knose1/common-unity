using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Com.GitHub.Knose1.JuicyText.Editor
{
	[CustomEditor(typeof(TextEffect)), CanEditMultipleObjects()]
	public class TextEffectEditor : UnityEditor.UI.TextEditor
	{
		private SerializedProperty propTypingInterval;
		private SerializedProperty propStartTextOnStart;

		protected override void OnEnable()
		{
			base.OnEnable();
			propTypingInterval = serializedObject.FindProperty("typingInterval");
			propStartTextOnStart = serializedObject.FindProperty("startTextOnStart");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();
			EditorGUILayout.PropertyField(propTypingInterval);
			EditorGUILayout.PropertyField(propStartTextOnStart);
			serializedObject.ApplyModifiedProperties();
		}
	}
}