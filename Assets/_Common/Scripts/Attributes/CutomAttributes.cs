using Com.GitHub.Knose1.Common.Attributes.Abstract;
using Com.GitHub.Knose1.Common.Reflexion;
using Com.GitHub.Knose1.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Com.GitHub.Knose1.Common.Attributes.Abstract
{
	public abstract class DisabledHide : Disabled
	{
#if UNITY_EDITOR
		public readonly bool hide;
#endif
		public DisabledHide(bool hide)
		{
#if UNITY_EDITOR
			this.hide = hide;
#endif
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public abstract class DisabledOn : DisabledHide
	{
#if UNITY_EDITOR
		/// <summary>
		/// If the "<see cref="onField"/>" is required or not
		/// </summary>
		public bool RequiredField { get; protected set; } = true;

		/// <summary>
		/// The field to compare with
		/// </summary>
		public readonly string onField;
#endif

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hide">To hide or not</param>
		/// <param name="onField">The field to compare with</param>
		public DisabledOn(bool hide, string onField) : base(hide)
		{
#if UNITY_EDITOR
			this.onField = onField;
#endif
		}

#if UNITY_EDITOR
		public abstract bool OnCondition(FieldInfo field, object instance);
#endif
	}

	public abstract class DisabledOnNumber : DisabledOn
	{
#if UNITY_EDITOR
		public NumberComparisionType comparisionType;
#endif

		public DisabledOnNumber(bool hide, string onField, NumberComparisionType comparisionType) : base(hide, onField)
		{
#if UNITY_EDITOR
			this.comparisionType = comparisionType;
#endif
		}
	}

}

namespace Com.GitHub.Knose1.Common.Attributes
{
	public enum ClassicComparisionType
	{
		equal,
		notEqual
	}

	public enum NumberComparisionType
	{
		equal,
		notEqual,
		superior,
		inferior,
		equalSuperior,
		equalInferior
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class Disabled : Attribute { }

	public class DisabledInRuntimeAttribute : DisabledHide
	{
		public DisabledInRuntimeAttribute(bool hide) : base(hide) { }
	}

	public class DisabledInEditorAttribute : DisabledHide
	{
		public DisabledInEditorAttribute(bool hide) : base(hide) { }
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public class DisabledIfComponentAttribute : DisabledHide
	{
#if UNITY_EDITOR
		public readonly Type component;
		public readonly bool toBeOrNotToBe;
#endif

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hide">If true, hide the field in the inspector</param>
		/// <param name="component">The component to check</param>
		/// <param name="toBeOrNotToBe">If true, disable if the component is present</param>
		public DisabledIfComponentAttribute(bool hide, Type component, bool toBeOrNotToBe) : base(hide)
		{
#if UNITY_EDITOR
			this.component = component;
			this.toBeOrNotToBe = toBeOrNotToBe;
#endif
		}

#if UNITY_EDITOR
		public bool Test(GameObject gameObject)
		{
			return toBeOrNotToBe == gameObject.GetComponent(component);
		}
#endif
	}

	public class DisabledOnFloatAttribute : DisabledOnNumber
	{
#if UNITY_EDITOR
		public readonly float value;
#endif
		public DisabledOnFloatAttribute(bool hide, string onField, NumberComparisionType comparisionType, float value) : base(hide, onField, comparisionType)
		{
#if UNITY_EDITOR
			this.value = value;
#endif
		}
#if UNITY_EDITOR
		public override bool OnCondition(FieldInfo field, object instance)
		{
			float fieldValue = (float)field.GetValue(instance);

			switch (comparisionType)
			{
				case NumberComparisionType.equal:
					return value == fieldValue;

				case NumberComparisionType.notEqual:
					return value != fieldValue;

				case NumberComparisionType.superior:
					return value < fieldValue;

				case NumberComparisionType.inferior:
					return value > fieldValue;

				case NumberComparisionType.equalSuperior:
					return value <= fieldValue;

				case NumberComparisionType.equalInferior:
					return value >= fieldValue;

				default:
					return false;
			}
		}
#endif
	}

	public class DisabledOnIntAttribute : DisabledOnNumber
	{
#if UNITY_EDITOR
		public readonly int value;
#endif
		public DisabledOnIntAttribute(bool hide, string onField, NumberComparisionType comparisionType, int value) : base(hide, onField, comparisionType)
		{
#if UNITY_EDITOR
			this.value = value;
#endif
		}

#if UNITY_EDITOR
		public override bool OnCondition(FieldInfo field, object instance)
		{
			int fieldValue = (int)field.GetValue(instance);

			switch (comparisionType)
			{
				case NumberComparisionType.equal:
					return value == fieldValue;

				case NumberComparisionType.notEqual:
					return value != fieldValue;

				case NumberComparisionType.superior:
					return value < fieldValue;

				case NumberComparisionType.inferior:
					return value > fieldValue;

				case NumberComparisionType.equalSuperior:
					return value <= fieldValue;

				case NumberComparisionType.equalInferior:
					return value >= fieldValue;

				default:
					return false;
			}
		}
#endif
	}

	public class DisabledOnUIntAttribute : DisabledOnNumber
	{
#if UNITY_EDITOR
		public readonly uint value;
#endif
		public DisabledOnUIntAttribute(bool hide, string onField, NumberComparisionType comparisionType, uint value) : base(hide, onField, comparisionType)
		{
#if UNITY_EDITOR
			this.value = value;
#endif
		}

#if UNITY_EDITOR
		public override bool OnCondition(FieldInfo field, object instance)
		{
			uint fieldValue = (uint)field.GetValue(instance);

			switch (comparisionType)
			{
				case NumberComparisionType.equal:
					return value == fieldValue;

				case NumberComparisionType.notEqual:
					return value != fieldValue;

				case NumberComparisionType.superior:
					return value < fieldValue;

				case NumberComparisionType.inferior:
					return value > fieldValue;

				case NumberComparisionType.equalSuperior:
					return value <= fieldValue;

				case NumberComparisionType.equalInferior:
					return value >= fieldValue;

				default:
					return false;
			}
		}
#endif
	}

	public class DisabledOnBoolAttribute : DisabledOn
	{
#if UNITY_EDITOR
		public readonly bool value;
#endif
		public DisabledOnBoolAttribute(bool hide, string onField, bool value) : base(hide, onField)
		{
#if UNITY_EDITOR
			this.value = value;
#endif
		}

#if UNITY_EDITOR
		public override bool OnCondition(FieldInfo field, object instance) => value == (bool)field.GetValue(instance);
#endif
	}

	public class DisabledOnFunctionAttribute : DisabledOn
	{
#if UNITY_EDITOR
		const string DEBUG = "["+nameof(DisabledOnFunctionAttribute)+"]";
		public delegate bool DisablingFunction(object instance);
		public readonly string function;

		private static readonly MethodInfo delegateInfo;
#endif

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hide"></param>
		/// <param name="function"><see cref="DisablingFunction"/></param>
		public DisabledOnFunctionAttribute(bool hide, string function) : base(hide, "")
		{
#if UNITY_EDITOR
			RequiredField = false;
			this.function = function;
#endif
		}

#if UNITY_EDITOR
		static DisabledOnFunctionAttribute()
		{
			delegateInfo = ReflexionUtils.GetDelegateMethodeDelcaration<DisablingFunction>();
		}

		public override bool OnCondition(FieldInfo field, object instance)
		{
			MethodComparition? _comparition = null;
			Type instanceType = instance.GetType();
			MethodInfo methodInfo = instanceType.GetMethod(function,BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

			bool err = false;
			string debug = DEBUG+$" Method \"{function}\" :";
			if (methodInfo == null)
			{
				Debug.LogWarning(debug + " Not found, make sure the methode is static");
				err = true;
			}
			else
			{
				MethodComparition comparition = new MethodComparition(methodInfo, delegateInfo, new object[] { instance });
				_comparition = comparition;

				MethodComparition.ReturnComparition returnComparition = comparition.returnComparition;
				if (!returnComparition.IsCompatible())
				{
					Debug.LogWarning(debug + $" Wrong return type, expecting {returnComparition.model} but got {returnComparition.method}");
					err = true;
				}

				MethodComparition.ParameterComparition[] parameters = comparition.parameterComparitions;
				int length1 = parameters.Length;
				for (int i = 0; i < length1; i++)
				{
					MethodComparition.ParameterComparition parameter = parameters[i];
					if (parameter.IsMissing())
					{
						Debug.LogWarning(debug + $" Missing argument of type {parameter.model.ParameterType} {parameter.model.Name}");
						err = true;
						continue;
					}

					if (parameter.IsExtra())
					{
						Debug.LogWarning(debug + $" Extra argument found : {parameter.method.ParameterType} {parameter.method.Name}");
						err = true;
						continue;
					}

					if (!parameter.IsCompatible())
					{
						Debug.LogWarning(debug + $" {parameter.method.ParameterType} {parameter.method.Name} can't be casted as {parameter.model.ParameterType} {parameter.model.Name}");
						err = true;
					}
				}
			}


			if (err)
			{
				return DefaultFunction(instance);
			}

			return (bool)_comparition.Value.Call(instance);
		}

		private static bool DefaultFunction(object instance) => false;
#endif
	}



	[System.AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class CustomGUIAttribute : Attribute
	{
		/// <summary>
		/// Struct used for safery
		/// </summary>
		public struct EditorGuiInfo {
#if UNITY_EDITOR
			public readonly UnityEditor.SerializedObject serializedObject;
			public readonly UnityEditor.SerializedProperty property;

			public EditorGuiInfo(SerializedObject serializedObject, SerializedProperty property)
			{
				this.serializedObject = serializedObject;
				this.property = property;
			}
#endif
		}
#if UNITY_EDITOR
		const string DEBUG = "["+nameof(CustomGUIAttribute)+"]";
#endif

#if UNITY_EDITOR
		public delegate void CustomGUIFunction(UnityEngine.Object target, EditorGuiInfo editorGuiInfo);
		private static readonly MethodInfo delegateInfo;
		public readonly string guiFunction;
#endif
		/// <summary>
		/// 
		/// </summary>
		/// <param name="guiFunction"><see cref="CustomGUIFunction"/></param>
		public CustomGUIAttribute(string guiFunction)
		{
#if UNITY_EDITOR
			this.guiFunction = guiFunction;
#endif
		}

#if UNITY_EDITOR
		static CustomGUIAttribute()
		{
			delegateInfo = ReflexionUtils.GetDelegateMethodeDelcaration<CustomGUIFunction>();
		}
#endif

#if UNITY_EDITOR
		/// <summary>
		/// 
		/// </summary>
		/// <param name="classType">The target where to find <see cref="guiFunction"/></param>
		/// <param name="target"></param>
		/// <param name="serializedObject"></param>
		/// <param name="property"></param>
		public virtual void DrawGui(object classTarget, UnityEngine.Object target, UnityEditor.SerializedObject serializedObject, UnityEditor.SerializedProperty property)
		{
			MethodComparition? _comparition = null;
			Type classType = classTarget.GetType();
			MethodInfo methodInfo = classType.GetMethod(guiFunction, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

			string debug = DEBUG+$" Method \"{guiFunction}\" :";
			if (methodInfo == null)
			{
				Debug.LogError(debug + " Not found on type " + classType);
				return;
			}
			else
			{
				MethodComparition comparition = new MethodComparition(methodInfo, delegateInfo, new object[] { target, new EditorGuiInfo(serializedObject, property) });
				_comparition = comparition;

				MethodComparition.ReturnComparition returnComparition = comparition.returnComparition;
				if (!returnComparition.IsCompatible())
				{
					Debug.LogWarning(debug + $" Wrong return type, expecting {returnComparition.model} but got {returnComparition.method}");
					return;
				}

				MethodComparition.ParameterComparition[] parameters = comparition.parameterComparitions;
				int length1 = parameters.Length;
				for (int i = 0; i < length1; i++)
				{
					MethodComparition.ParameterComparition parameter = parameters[i];
					if (parameter.IsMissing())
					{
						Debug.LogWarning(debug + $" Missing argument of type {parameter.model.ParameterType} {parameter.model.Name}");
						return;
					}

					if (parameter.IsExtra())
					{
						Debug.LogWarning(debug + $" Extra argument found : {parameter.method.ParameterType} {parameter.method.Name}");
						return;
					}

					if (!parameter.IsCompatible(out bool methodSubOfModel, out bool parameterSubOfMethod, out bool parameterSubOfModel))
					{

						Debug.LogWarning(debug + $" {parameter.method.ParameterType} {parameter.method.Name} can't be casted as {parameter.model.ParameterType} {parameter.model.Name}");
						return;
					}
				}
			}

			_comparition.Value.Call(classTarget);
		}
#endif

	}

	[System.AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
	public abstract class FolderBase : Attribute {}

	public class FolderAttribute : FolderBase
	{
		[Flags]
		public enum FolderType
		{
			Default = 1 << 1,
			Header = 1 << 2,
			CheckBox  = 1 << 3,
			Fade  = 1 << 4,
		}

#if UNITY_EDITOR
		public readonly string name;
		public readonly bool foldedByDefault;
		public readonly FolderType folderType;
		public readonly float fadeTime;
#endif
		public FolderAttribute(string name, bool foldedByDefault = false, bool isHeader = false, float fadeTime = 1)
		{
#if UNITY_EDITOR
			this.name = name;
			this.foldedByDefault = foldedByDefault;
			this.folderType = isHeader ? FolderType.Header : FolderType.Default;
			this.fadeTime = fadeTime;

			if (fadeTime != 0)
			{
				this.folderType |= FolderType.Fade;
			}
#endif
		}
	}

	public class EndFolderAttribute : FolderBase
	{
		public readonly Position position;

		public enum Position
		{
			Before,
			After
		}
		public EndFolderAttribute(Position position = Position.Before){
			this.position = position;
		}
	}
}
