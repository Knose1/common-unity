using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Com.GitHub.Knose1.Editor.Utils
{
	public static class SceneViewUtils
	{
		public static Vector3 EditorPointToWorldPoint(Vector2 mousePosition, SceneView view)
		{
			Vector2 screenPos = HandleUtility.GUIPointToScreenPixelCoordinate(mousePosition);
			screenPos.y -= 50;
			Vector3 pos = view.camera.ScreenToWorldPoint(screenPos);

			return pos;
		}
		
		public static Vector3 WorldPointToEditorPoint(Vector3 worldPosition, SceneView view)
		{
			return HandleUtility.WorldToGUIPoint(worldPosition);
		}
	}
}
