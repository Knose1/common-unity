using Com.GitHub.Knose1.Editor.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Com.GitHub.Knose1.Editor.ToolEditor
{
	public struct ToolMenuItem
	{
		public readonly bool isSeparator;
		public readonly GUIContent content;
		public readonly bool on;
		public readonly GenericMenu.MenuFunction func;
		public readonly bool enabled;

		public ToolMenuItem(string content, bool on, GenericMenu.MenuFunction func, bool enabled = true) : this(new GUIContent(content), on, func, enabled) {}
		public ToolMenuItem(GUIContent content, bool on, GenericMenu.MenuFunction func, bool enabled = true) : this(false)
		{
			this.content = content ?? throw new ArgumentNullException(nameof(content));
			this.on = on;
			this.func = func ?? throw new ArgumentNullException(nameof(func));
			this.enabled = enabled;
		}

		private ToolMenuItem(bool isSeparator) : this()
		{
			this.isSeparator = isSeparator;
		}

		public static ToolMenuItem Separator() => new ToolMenuItem(true);
	}

	public struct ToolTargetMenuItem
	{
		public List<ToolMenuItem> items;
		public Vector3 position;
		public string name;
		public float threshold;

		public ToolTargetMenuItem(List<ToolMenuItem> items, Vector3 position, string name = "", float threshold = 10)
		{
			this.items = items ?? throw new ArgumentNullException(nameof(items));
			this.position = position;
			this.name = name;
			this.threshold = threshold;
		}
	}

	abstract public class EditorContextMenuTool : EditorTool
	{
		protected List<ToolMenuItem> menuItems = new List<ToolMenuItem>();
		protected List<ToolTargetMenuItem> targetMenuItems = new List<ToolTargetMenuItem>();
		
		
		[Shortcut("Open Context Menu", KeyCode.Space, ShortcutModifiers.Action)]
		public static void OpenContextMenu(ShortcutArguments args)
		{
			Type activeToolType = EditorTools.activeToolType;
			Type myType = typeof(EditorContextMenuTool);
			if (activeToolType == myType || activeToolType.IsSubclassOf(myType))
			{
				if (EditorWindow.focusedWindow is SceneView)
				{
					var objects = Resources.FindObjectsOfTypeAll(activeToolType);
					if (objects.Length > 0) ((EditorContextMenuTool)objects[0]).haveToShowMenu = true;
				}
			}
			
		}

		private bool haveToShowMenu;
		public override void OnToolGUI(EditorWindow window)
		{
			if (haveToShowMenu) ShowMenu();
		}

		protected void ShowMenu()
		{
			haveToShowMenu = false;
			var menu = new GenericMenu();
			GenerateMenu(menu, Event.current.mousePosition);
			menu.ShowAsContext();
		}

		protected virtual void GenerateMenu(GenericMenu menu, Vector2 mousePosition)
		{
			foreach (var menuItem in menuItems)
			{
				AddItemToMenu(menu, menuItem);
				
			}

			int targetMenuItemsCount = targetMenuItems.Count;
			int shownInMenuCount = 0;
			for (int i = 0; i < targetMenuItemsCount; i++)
			{
				var targetMenuItem = targetMenuItems[i];
				float threshold = targetMenuItem.threshold;
				Vector2 pos = SceneViewUtils.WorldPointToEditorPoint(targetMenuItem.position, SceneView.currentDrawingSceneView);

				if ((pos - mousePosition).sqrMagnitude < threshold * threshold)
				{
					if (shownInMenuCount == 0 && menuItems.Count > 0) menu.AddSeparator("");

					List<ToolMenuItem> items = targetMenuItem.items;
					foreach (var menuItem in items)
					{
						AddItemToMenu(menu, menuItem, targetMenuItem.name);
					}

					shownInMenuCount += 1;
				}
			}
		}

		protected static void AddItemToMenu(GenericMenu menu, ToolMenuItem item, string parent="")
		{
			if (item.isSeparator)
			{
				menu.AddSeparator("");
				return;
			}

			GUIContent lContent = item.content;
			if (parent != string.Empty)
			{
				lContent = new GUIContent(lContent);
				lContent.text = parent+"/"+lContent.text;
			}
			
			if (item.enabled) menu.AddItem(lContent, item.on, item.func);
			else menu.AddDisabledItem(lContent, item.on);
		}
	}
}
