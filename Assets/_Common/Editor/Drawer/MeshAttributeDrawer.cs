using UnityEditor;
using UnityEngine;

namespace Com.GitHub.Knose1.Editor.Drawer
{
	[CustomPropertyDrawer(typeof(MeshAttribute))]
	public class MeshAttributeDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => base.GetPropertyHeight(property, label);
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.ObjectReference)
			{
				GUI.Label(position, "Type is not \"Mesh\"");
				return;
			}

			if (!(property.type == "PPtr<$Mesh>"))
			{
				GUI.Label(position, "Type is not \"Mesh\"");
				return;
			}

			Rect objField = position;
			objField.width /= 2;

			Rect btnField = position;
			btnField.x = objField.x;
			btnField.x += objField.width;
			btnField.width /= 2;

			float lLbW = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(label).x + 3;

			EditorGUI.ObjectField(objField, property, label);

			EditorGUIUtility.labelWidth = lLbW;

			if (GUI.Button(btnField, "Create"))
			{
				CreateMesh(property);
			}
		}

		public void CreateMesh(SerializedProperty property)
		{
			string filePath =
		EditorUtility.SaveFilePanelInProject("Save Mesh", "Mesh", "asset", "");
			if (filePath == "") return;

			AssetDatabase.CreateAsset(property.objectReferenceValue = new Mesh(), filePath);
		}
	}
}