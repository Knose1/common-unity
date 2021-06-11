using Com.GitHub.Knose1.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using static Com.GitHub.Knose1.Editor.PolyGenerator.PolyGeneratorData;

namespace Com.GitHub.Knose1.Editor.PolyGenerator
{
	[Flags]
	public enum PolyToolSceneVisibility : int
	{
		Names = 1 << 0,
		Handles = 1 << 1,
		Lines = 1 << 2,
		Triangles = 1 << 3,
	}
	[Serializable]
	internal class PolygonGeneratorException : Exception
	{
		public PolygonGeneratorException() { }
		public PolygonGeneratorException(string message) : base(message) { }
		public PolygonGeneratorException(string message, Exception inner) : base(message, inner) { }
		protected PolygonGeneratorException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	public class PolyGeneratorWindow : EditorWindow
	{
		
		//--------------------------------//
		//           CONSTANTES           //
		//--------------------------------//
		private const string TITLE = "PolyGenerator";
		private const string GENERATE_TRIANGLES_BTN = "Generate Triangles";
		private const string SAVE_MESH_BTN = "Save Mesh";
		private const string LOAD_MESH_BTN = "Load Mesh";
		private const string REVERSE_POINTS_BTN = "Reverse Points";
		private const string SHOW_SETTINGS_BTN = "Show settings";
		private const string HIDE_SETTINGS_BTN = "Hide settings";
		public const float LABEL_DISTANCE = 1f;

		//--------------------------------//
		//             FIELDS             //
		//--------------------------------//
		public static PolyGeneratorWindow Instance { get; private set; }

		private Vector2 guiScrollPosition = default;
		private bool isShowingSettings = false;

		//--------------------------------//
		//             SHOWME             //
		//--------------------------------//
		[MenuItem("Window/" + nameof(PolyGeneratorWindow))]
		public static PolyGeneratorWindow ShowMe()
		{
			PolyGeneratorWindow get = GetWindow<PolyGeneratorWindow>();
			if (get)
			{
				get.Focus();
				return Instance = get;
			}

			PolyGeneratorWindow window = CreateWindow<PolyGeneratorWindow>();
			window.Show();
			window.Focus();

			return Instance = window;
		}

		//--------------------------------//
		//            MESSAGES            //
		//--------------------------------//
		private void Awake()
		{
			titleContent = new GUIContent(TITLE);
		}

		private void OnEnable()
		{
			//Set tool
			if (EditorTools.activeToolType != typeof(PolyTool))
				EditorTools.SetActiveTool<PolyTool>();

			SceneView.duringSceneGui += SceneView_duringSceneGui;
		}

		private void OnDisable()
		{
			SceneView.duringSceneGui -= SceneView_duringSceneGui;
		}

		private void OnDestroy()
		{
			Instance = null;
			if (!SceneView.lastActiveSceneView.maximized)
			{
				if (EditorTools.activeToolType == typeof(PolyTool))
				{
					Tools.current = Tool.None;
				}
			}
		}

		//--------------------------------//
		//             ONGUI              //
		//--------------------------------//
		private void OnGUI()
		{
			guiScrollPosition = EditorGUILayout.BeginScrollView(guiScrollPosition);

			//Create SerializedObject
			SerializedObject lSObj = new SerializedObject(DataInstance);
			//Padding top
			GUILayout.Space(10);
			//Update SerializedObject representation
			lSObj.Update();

			//Create list if null
			if (DataInstance.points == null) DataInstance.points = new List<Vector2>();
			if (DataInstance.triangles == null) DataInstance.triangles = new List<Triangle>();

			EditorGUI.BeginChangeCheck();

			//Iterate on properties
			SerializedProperty lProp = lSObj.GetIterator();
			lProp.NextVisible(true);
			lProp.NextVisible(false);
			do
			{
				EditorGUILayout.PropertyField(lProp);
			} while (lProp.NextVisible(false));
			EditorGUILayout.EndScrollView();

			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
			}

			//BUTTON //////////////////////////////
			GUILayout.Space(10);

			//HORIZONTAL //////////////////////////
			EditorGUILayout.BeginHorizontal();
			GUI.enabled = DataInstance.points.Count > 2;
			if (GUILayout.Button(GENERATE_TRIANGLES_BTN))
			{
				//GENERATE TRIANGLES //////////////////////////
				Undo.RecordObject(DataInstance, "Generate triangles");
				DataInstance.GenerateTriangles();
				SceneView.RepaintAll();
			}

			GUI.enabled = DataInstance.mesh != null && DataInstance.points.Count > 2 && DataInstance.triangles.Count > 0;
			if (GUILayout.Button(SAVE_MESH_BTN))
			{
				//SAVE MESH ///////////////////////////////////
				DataInstance.SaveMesh();
			}
			EditorGUILayout.EndHorizontal();


			GUI.enabled = DataInstance.mesh != null;
			GUILayout.Space(3);
			if (GUILayout.Button(LOAD_MESH_BTN))
			{
				//LOAD MESH ///////////////////////////////////
				Undo.RecordObject(DataInstance, "Load Mesh");
				DataInstance.points = DataInstance.mesh.vertices.Select((v) => (Vector2)v).ToList();
				DataInstance.triangles = Triangle.MakeListFrom(DataInstance.mesh.triangles.ToList());
				SceneView.RepaintAll();
			}
			GUILayout.Space(3);
			GUI.enabled = DataInstance.PointsCount > 2;
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(REVERSE_POINTS_BTN))
			{
				//REVERSE POINTS //////////////////////////////
				Undo.RecordObject(DataInstance, "Reverse points");
				DataInstance.points.Reverse();
				DataInstance.triangles = new List<Triangle>();
				SceneView.RepaintAll();
			}
			GUI.enabled = true;

			GUIStyle style = new GUIStyle(GUI.skin.button);
			if (GUILayout.Button(isShowingSettings ? HIDE_SETTINGS_BTN : SHOW_SETTINGS_BTN, style: style))
			{
				//SETTINGS ////////////////////////////////////
				isShowingSettings = !isShowingSettings;
			}
			EditorGUILayout.EndHorizontal();

			if (isShowingSettings)
			{
				EditorGUI.BeginChangeCheck();
				DataInstance.guizmos = (PolyToolSceneVisibility)EditorGUILayout.EnumFlagsField(ObjectNames.NicifyVariableName(nameof(DataInstance.guizmos)), DataInstance.guizmos);
				if (EditorGUI.EndChangeCheck())
				{
					SceneView.RepaintAll();
				}
			}


			//Apply modified properties
			lSObj.ApplyModifiedProperties();

			//Dispose the object and the iterator
			lProp.Dispose();
			lSObj.Dispose();

			//Update the field "pointsCount"
			DataInstance.UpdatePointCount();
		}

		//--------------------------------//
		//           SCENE VIEW           //
		//--------------------------------//
		private void SceneView_duringSceneGui(SceneView obj)
		{
			Repaint();
		}
	}
}
