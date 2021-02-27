using Com.GitHub.Knose1;
using Com.GitHub.Knose1.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Com.GitHub.Knose1.Common.TestBetterEditor
{
	[CustomGUI(nameof(CustomGui)), Serializable]
	public class Hello
	{
		[NonSerialized] public bool fadeG;
		[NonSerialized] public Vector3 sum;
		private static readonly Vector3 vector3 = default;
		[SerializeField] Vector3 v1 = vector3;
		[SerializeField] Vector3 v2 = vector3;
		[SerializeField] Vector3 v3 = vector3;
		[SerializeField, Range(0,1)] float f = 1;

		bool flipFlop;
		private void CustomGui(UnityEngine.Object target, CustomGUIAttribute.EditorGuiInfo editorGuiInfo)
		{
			f = UnityEditor.EditorGUILayout.Slider("f", f, 0, 1);

			bool openCondition = f != 0;
			bool openedInput;
			if (openedInput = UnityEditor.EditorGUILayout.Foldout(openCondition, editorGuiInfo.property.displayName))
			{
				UnityEditor.EditorGUI.indentLevel += 1;
				fadeG = UnityEditor.EditorGUILayout.BeginFadeGroup(f);

				v1 = UnityEditor.EditorGUILayout.Vector3Field(nameof(v1), v1);
				v2 = UnityEditor.EditorGUILayout.Vector3Field(nameof(v2), v2);
				v3 = UnityEditor.EditorGUILayout.Vector3Field(nameof(v3), v3);

				bool isEnabled = GUI.enabled;
				GUI.enabled = false;
				UnityEditor.EditorGUILayout.Vector3Field("sum", sum = v1 + v2 + v3);
				GUI.enabled = isEnabled;

				UnityEditor.EditorGUILayout.EndFadeGroup();

				GUI.enabled = false;
				UnityEditor.EditorGUILayout.Toggle("fadeG", fadeG);
				GUI.enabled = isEnabled;

				UnityEditor.EditorGUI.indentLevel -= 1;
			}
			//UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();
			if (openedInput != openCondition) flipFlop = !flipFlop;

			if (!flipFlop) f += Time.deltaTime / 5;
			if (flipFlop) f -= Time.deltaTime / 5;

			f = Mathf.Clamp(f, 0, 1);
		}
	}

	public class TestBetterEditor : MonoBetterEditor
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
		[DisabledIfComponent(false, typeof(Collider), true)] public float disabledOnCollider;
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
		[EndFolder(EndFolderAttribute.Position.After)]
		[EndFolder(EndFolderAttribute.Position.After)]
		[CustomGUI(nameof(CustomGUI_GUI))] public float customGUI;
		
		//public Hello customGUIClass;
		
#if UNITY_EDITOR
		private static void CustomGUI_GUI(UnityEngine.Object target, CustomGUIAttribute.EditorGuiInfo editorGuiInfo)
		{
			UnityEditor.EditorGUILayout.Space();
			UnityEditor.EditorGUILayout.HelpBox("Hello", UnityEditor.MessageType.Info);
			UnityEditor.EditorGUILayout.PropertyField(editorGuiInfo.property);
		}
#endif

		private static bool DisableFunction(TestBetterEditor instance)
		{
			return instance.boolValue == true && instance.floatValue == 1.1f && instance.intValue == 1 && instance.uintValue == 1;
		}
	}
}
