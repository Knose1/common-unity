using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using Com.GitHub.Knose1.Editor.ToolEditor;
using System.Collections.Generic;
using Com.GitHub.Knose1.Common.Utils;
using System.Linq;
using Com.GitHub.Knose1.Editor.Utils;
using static Com.GitHub.Knose1.Editor.PolyGenerator.PolyGeneratorWindow;

namespace Com.GitHub.Knose1.Editor.PolyGenerator
{
	[EditorTool("Polygone Tool")]
	class PolyTool : EditorContextMenuTool
	{
		private PolyGeneratorData datas;
		private PolyGeneratorWindow targetWindow;
		private GUIContent m_IconContent;

		private void OnEnable()
		{
			EditorTools.activeToolChanged += EditorTools_activeToolChanged;
			m_IconContent = new GUIContent()
			{
				image = base.toolbarIcon.image,
				text = "Polygone Tool",
				tooltip = "Polygone Tool"
			};
		}


		private void OnDisable()
		{
			EditorTools.activeToolChanged -= EditorTools_activeToolChanged;
		}

		private void EditorTools_activeToolChanged()
		{
			if (EditorTools.IsActiveTool(this))
			{
				ShowMe();
			}
		}
		
		public override void OnToolGUI(EditorWindow window)
		{
			Event currentEvent = Event.current;
			Vector2 mousePos = currentEvent.mousePosition;

			//Set scene in 2D mode
			SceneView currentDrawingSceneView = SceneView.currentDrawingSceneView;
			if (!currentDrawingSceneView.in2DMode) currentDrawingSceneView.in2DMode = true;

			//Get window and datas
			targetWindow = PolyGeneratorWindow.Instance;
			datas = PolyGeneratorData.DataInstance;
			
			//On press "F" (Focus)
			if (Selection.activeGameObject == null)
			{
				if (currentEvent.type == EventType.KeyDown)
				{
					if (currentEvent.keyCode == KeyCode.F)
					{
						Vector2 lCenter = datas.GetCenter();
						float lSize = Mathf.Sqrt(datas.points.Max((v) => (v - lCenter).sqrMagnitude));
						
						currentDrawingSceneView.pivot = lCenter;
						currentDrawingSceneView.size = lSize;

						currentEvent.Use();
					}
				}
			}

			//GUIZMO SETTINGS ////////////////////////////
			//Util functions
			void LToggle(bool hasFlag, PolyToolSceneVisibility flag)
			{
				if (hasFlag) FlagEnumUtils.Remove(ref datas.guizmos, flag);
				else FlagEnumUtils.Add(ref datas.guizmos, flag);
			}

			ToolMenuItem LGenerateMenu(PolyToolSceneVisibility flag)
			{
				bool hasFlag = FlagEnumUtils.Contains(datas.guizmos, flag);
				return new ToolMenuItem("Show " + flag, hasFlag, () => { LToggle(hasFlag, flag); });
			}

			var enumerator = Enum.GetValues(typeof(PolyToolSceneVisibility)).Cast<PolyToolSceneVisibility>();

			menuItems = new List<ToolMenuItem>();
			foreach (var item in enumerator)
			{
				menuItems.Add(LGenerateMenu(item));
			}

			menuItems.Add(ToolMenuItem.Separator());

			//ADD POINT ///////////////////////////////////
			menuItems.Add(new ToolMenuItem("Add Point", false, () => { 
				Undo.RecordObject(datas, "Added point"); 
				datas.points.Add(SceneViewUtils.EditorPointToWorldPoint(mousePos, currentDrawingSceneView));
				if (targetWindow) targetWindow.Repaint(); 
			}));

			//GENERATE TRIANGLES //////////////////////////
			bool canGenerateTriangles = datas.points.Count > 2;
			menuItems.Add(new ToolMenuItem("Generate triangles", false, () => {
				Undo.RecordObject(datas, "Generate triangles");
				datas.GenerateTriangles();
				SceneView.RepaintAll();
			}, canGenerateTriangles));

			//SAVE MESH ///////////////////////////////////
			bool canSaveMesh = datas.mesh != null && datas.points.Count > 2 && datas.triangles.Count > 0;
			menuItems.Add(new ToolMenuItem("Save Mesh", false, () => {
				datas.SaveMesh();
			}, canSaveMesh));
			
			//START DRAWING /////////////////////////////////////////////////////////////////
			if (datas.points == null) datas.points = new List<Vector2>();
			if (datas.triangles == null) datas.triangles = new List<Triangle>();

			if (datas.PointsCount > 0)
			{
				//DRAW TRIANGLES
				DrawTriangles(datas);

				//DRAW POINT HANDLES
				DrawPointHandles(datas);
				Handles.color = Color.white;
			}

			base.OnToolGUI(window);
		}


		private void DrawPointHandles(PolyGeneratorData target)
		{
			if (FlagEnumUtils.Contains(target.guizmos, PolyToolSceneVisibility.Handles | PolyToolSceneVisibility.Lines | PolyToolSceneVisibility.Names))
			{
				target.UpdatePointCount();
				targetMenuItems = new List<ToolTargetMenuItem>();
				for (int i = target.PointsCount - 1; i >= 0; i--)
				{
					EditorGUI.BeginChangeCheck();

					int previousPoint = target.WrapIndex(i-1);
					int nextPoint = target.WrapIndex(i+1);

					Handles.color = i == 0 ? Color.red : Color.white;
					Vector2 position = target.points[i];

					float lhandleSize = HandleUtility.GetHandleSize(position)/15;
					if (FlagEnumUtils.Contains(target.guizmos, PolyToolSceneVisibility.Handles))
					{
						Vector3 snap = EditorSnapSettings.move;
						position = Handles.FreeMoveHandle(position, Quaternion.identity, lhandleSize, snap, Handles.DotHandleCap);


						if (EditorGUI.EndChangeCheck())
						{
							Event currentEvent = Event.current;
							bool isControl = currentEvent.isKey && Event.current.keyCode == KeyCode.LeftControl && Event.current.keyCode == KeyCode.RightControl;
							if (isControl && snap.x != 0 && snap.y != 0)
							{
								Vector2 sn2 = snap;
								position = position/sn2;
								position.x = Mathf.Floor(position.x);
								position.y = Mathf.Floor(position.y);
								position = position*snap;
							}
							
							Undo.RecordObject(datas, "Move polygone handle_" + i);
							target.points[i] = position;
						}

						GenerateHandlerContextMenu(target, i, position);
					}

					Handles.color = i == 1 ? Color.red : Color.white;

					Vector2 previousposition = target.points[previousPoint];
					Vector2 nextposition = target.points[nextPoint];
					Vector2 centerNextPre = (nextposition+previousposition)/2;

					Vector2 toNext = nextposition - position;
					Vector2 toPrevious = previousposition - position;
					Vector2 toCenterNextPre = centerNextPre - position;

					Vector2 labelPos = toCenterNextPre.normalized * LABEL_DISTANCE + position;

					if (FlagEnumUtils.Contains(target.guizmos, PolyToolSceneVisibility.Names))
						Handles.Label(labelPos, new GUIContent(i.ToString()));
					if (FlagEnumUtils.Contains(target.guizmos, PolyToolSceneVisibility.Lines))
						Handles.DrawLine(position, previousposition);

				}
			}

			Handles.color = Color.white;
		}

		private void GenerateHandlerContextMenu(PolyGeneratorData target, int i, Vector2 position)
		{
			targetMenuItems.Add(new ToolTargetMenuItem(
			new List<ToolMenuItem>()
			{
				new ToolMenuItem("Delete Point", false, () => {Undo.RecordObject(target, "Removed point"); target.points.RemoveAt(i);}),
				new ToolMenuItem("Insert Point", false, () => {Undo.RecordObject(target, "Insert point"); target.points.Insert(i+1, position + Vector2.one);})
			},
			position,
			$"Point {i}"
			));
		}

		private void DrawTriangles(PolyGeneratorData target)
		{
			if (FlagEnumUtils.ContainsAll(target.guizmos, PolyToolSceneVisibility.Triangles))
			{
				int count = target.triangles.Count;
				for (int i = count - 1; i >= 0; i--)
				{
					Triangle triangle = target.triangles[i];
					if (!triangle.drawDebug) continue;
					int p1 = triangle.p1;
					int p2 = triangle.p2;
					int p3 = triangle.p3;
					if (target.PointsCount <= p1 || target.PointsCount <= p2 || target.PointsCount <= p3)
					{
						target.triangles.RemoveAt(i);
						continue;
					}


					Color color = Color.HSVToRGB((float)i/count,1,0.5f);
					color.a = 0.5f;
					Handles.color = color;
					Handles.DrawAAConvexPolygon(target.points[p1], target.points[p2], target.points[p3]);
				}

				Handles.color = Color.white;
			}
		}

		public override GUIContent toolbarIcon
		{
			get { return m_IconContent; }
		}
	}
}