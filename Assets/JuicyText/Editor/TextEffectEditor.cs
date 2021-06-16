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

		protected override void OnEnable()
		{
			base.OnEnable();
			propTypingInterval = serializedObject.FindProperty("typingInterval");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();
			EditorGUILayout.PropertyField(propTypingInterval);
			serializedObject.ApplyModifiedProperties();
		}
	}
}