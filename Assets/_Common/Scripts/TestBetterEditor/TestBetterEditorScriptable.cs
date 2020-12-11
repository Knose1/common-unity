using Com.GitHub.Knose1;
using Com.GitHub.Knose1.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Com.Github.Knose1.Common.TestBetterEditor
{
	[CreateAssetMenu(fileName=nameof(TestBetterEditorScriptable), menuName ="Test/BetterEditor/"+nameof(TestBetterEditorScriptable))]
	public class TestBetterEditorScriptable : ScriptableBetterEditor
	{
		[Folder("Fields checked")]
		public bool boolValue = true;
		public float floatValue = 1.1f;
		public int intValue = 1;
		[EndFolder(EndFolderAttribute.Position.After)]
		public uint uintValue = 1;
		
		[Header("Disable")]
		[Folder("Common")]
		[Disabled()] public float disabled;
		//[DisabledIfComponent(false, typeof(Collider), true)] public float disabledOnCollider;
		[DisabledInEditor(false)] public float disabledInEditor;
		[DisabledInRuntime(false)] public float disabledInRuntime;
		[DisabledOnBool(false, nameof(boolValue), true)] public float disabledOnBool;
		[EndFolder()]

		[Space()]
		[Folder("Float")]
		[DisabledOnFloat(false, nameof(floatValue), NumberComparisionType.equal, 1.1f)] public float disabledOnEqualFloat;
		[DisabledOnFloat(false, nameof(floatValue), NumberComparisionType.equalInferior, 1.1f)] public float disabledOnEqualInferiorFloat;
		[DisabledOnFloat(false, nameof(floatValue), NumberComparisionType.equalSuperior, 1.1f)] public float disabledOnEqualSuperiorFloat;
		[DisabledOnFloat(false, nameof(floatValue), NumberComparisionType.notEqual, 1.1f)] public float disabledOnNotEqualFloat;
		[DisabledOnFloat(false, nameof(floatValue), NumberComparisionType.inferior, 1.1f)] public float disabledOnInferiorFloat;
		[DisabledOnFloat(false, nameof(floatValue), NumberComparisionType.superior, 1.1f)] public float disabledOnSuperiorFloat;
		[EndFolder()]

		[Space()]
		[Folder("Int")]
		[DisabledOnInt(false, nameof(intValue), NumberComparisionType.equal, 1)] public float disabledOnEqualInt;
		[DisabledOnInt(false, nameof(intValue), NumberComparisionType.equalInferior, 1)] public float disabledOnEqualInferiorInt;
		[DisabledOnInt(false, nameof(intValue), NumberComparisionType.equalSuperior, 1)] public float disabledOnEqualSuperiorInt;
		[DisabledOnInt(false, nameof(intValue), NumberComparisionType.notEqual, 1)] public float disabledOnNotEqualInt;
		[DisabledOnInt(false, nameof(intValue), NumberComparisionType.inferior, 1)] public float disabledOnInferiorInt;
		[DisabledOnInt(false, nameof(intValue), NumberComparisionType.superior, 1)] public float disabledOnSuperiorInt;
		[EndFolder()]

		[Space()]
		[Folder("UInt")]
		[DisabledOnUInt(false, nameof(uintValue), NumberComparisionType.equal, 1)] public float disabledOnEqualUInt;
		[DisabledOnUInt(false, nameof(uintValue), NumberComparisionType.equalInferior, 1)] public float disabledOnEqualInferiorUInt;
		[DisabledOnUInt(false, nameof(uintValue), NumberComparisionType.equalSuperior, 1)] public float disabledOnEqualSuperiorUInt;
		[DisabledOnUInt(false, nameof(uintValue), NumberComparisionType.notEqual, 1)] public float disabledOnNotEqualUInt;
		[DisabledOnUInt(false, nameof(uintValue), NumberComparisionType.inferior, 1)] public float disabledOnInferiorUInt;
		[DisabledOnUInt(false, nameof(uintValue), NumberComparisionType.superior, 1)] public float disabledOnSuperiorUInt;
		[EndFolder()]

		[Folder("Function")]
		[DisabledOnFunction(false, nameof(DisableFunction))] public float disabledOnFunction;

		[Folder("CustomGui")]
		[CustomGUI(nameof(CustomGUI_GUI))] public float customGUI;
		
		[EndFolder()]
		[EndFolder(EndFolderAttribute.Position.After)]
		public Hello customGUIClass;
		
#if UNITY_EDITOR
		private static void CustomGUI_GUI(UnityEngine.Object target, CustomGUIAttribute.EditorGuiInfo editorGuiInfo)
		{
			UnityEditor.EditorGUILayout.Space();
			UnityEditor.EditorGUILayout.HelpBox("Hello", UnityEditor.MessageType.Info);
			UnityEditor.EditorGUILayout.PropertyField(editorGuiInfo.property);
		}
#endif

		private static bool DisableFunction(TestBetterEditorScriptable instance)
		{
			return instance.boolValue == true && instance.floatValue == 1.1f && instance.intValue == 1 && instance.uintValue == 1;
		}
	}
}
