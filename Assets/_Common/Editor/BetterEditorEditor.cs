using Com.GitHub.Knose1.Common.Attributes;
using Com.GitHub.Knose1.Common.Attributes.Abstract;
using Com.GitHub.Knose1.Common.Reflexion;
using Com.GitHub.Knose1.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Com.GitHub.Knose1.Common.Editor
{
	[CustomEditor(typeof(ScriptableBetterEditor), true)]
	public class ScriptableBetterEditorEditor : BetterEditorEditor<ScriptableBetterEditor>
	{
		public override void ShowScript()
		{
			EditorGUILayout.ObjectField(SCRIPT_FIELD, MonoScript.FromScriptableObject((ScriptableBetterEditor)target), typeof(ScriptableBetterEditor), false);
		}

		public override void OnInspectorGUI()
		{
			BetterGui();
		}
	}

	[CustomEditor(typeof(MonoBetterEditor), true), CanEditMultipleObjects()]
	public class MonoBetterEditorEditor : BetterEditorEditor<MonoBetterEditor>
	{
		public override void ShowScript()
		{
			EditorGUILayout.ObjectField(SCRIPT_FIELD, MonoScript.FromMonoBehaviour((MonoBetterEditor)target), typeof(MonoBetterEditor), false);
		}

		public override void OnInspectorGUI()
		{
			BetterGui();
		}
	}

	public abstract class BetterEditorEditor<InspectedType> : UnityEditor.Editor where InspectedType : UnityEngine.Object
	{
		private const float MIN_FOLD = 0;
		private const float MAX_FOLD = 1;


		protected const string SCRIPT_FIELD = "Script";
		protected const BindingFlags GET_FIELD_ATTR =
			BindingFlags.Default |
			BindingFlags.Instance |
			BindingFlags.Static |
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.CreateInstance |
			BindingFlags.GetField |
			BindingFlags.SetField |
			BindingFlags.GetProperty |
			BindingFlags.SetProperty |
			BindingFlags.PutDispProperty |
			BindingFlags.PutRefDispProperty |
			BindingFlags.ExactBinding |
			BindingFlags.SuppressChangeType |
			BindingFlags.OptionalParamBinding;

		private static float lastTime = 0;
		private List<bool> opened = new List<bool>();
		private List<float> fadeRatio = new List<float>();
		private List<bool> fadeFlipFlop = new List<bool>();
		private int depth = 0;

		public abstract void ShowScript();

		public void BetterGui()
		{
			Debug.Log("---------");
			float deltaTime = Time.realtimeSinceStartup - lastTime;
			if (!(target is InspectedType))
			{
				base.OnInspectorGUI();
				return;
			}

			serializedObject.Update();

			//Script
			GUI.enabled = false;
			ShowScript();
			GUI.enabled = true;
			////////

			Type type = target.GetType();
			SerializedProperty property = serializedObject.GetIterator();
			if (property.NextVisible(true))
			{
				int index = 0;
				List<int> indexByDepth = new List<int>();
				List<FolderAttribute.FolderType> openedFolderTypes = new List<FolderAttribute.FolderType>();
				bool opened = true;
				do
				{
					FieldInfo field = ReflexionUtils.GetField(type, property.name, GET_FIELD_ATTR);
					if (field == null)
					{
						//Debug.LogWarning(property.name + " has been ignored");
						continue;
					}

					DisabledInEditorAttribute       disabledInEditor    = (DisabledInEditorAttribute)    Attribute.GetCustomAttribute(field, typeof(DisabledInEditorAttribute));
					DisabledInRuntimeAttribute      disabledInRuntime   = (DisabledInRuntimeAttribute)   Attribute.GetCustomAttribute(field, typeof(DisabledInRuntimeAttribute));
					DisabledIfComponentAttribute    disabledIfComponent = (DisabledIfComponentAttribute) Attribute.GetCustomAttribute(field, typeof(DisabledIfComponentAttribute));
					CustomGUIAttribute              customeGuiAttr      = (CustomGUIAttribute)           Attribute.GetCustomAttribute(field, typeof(CustomGUIAttribute));
					DisabledOn[]                    disabledOnList      = (DisabledOn[])Attribute.GetCustomAttributes(field, typeof(DisabledOn));

					FolderBase[]                    folders             = (FolderBase[])Attribute.GetCustomAttributes(field, typeof(FolderBase));

					Attribute[] disabledList = Attribute.GetCustomAttributes(field, typeof(Disabled));

					Disabled disabled = null;
					for (int i = disabledList.Length - 1; i >= 0; i--)
					{
						if (disabledList[i].GetType() == typeof(Disabled))
						{
							disabled = (Disabled)disabledList[i];
						}
					}

					bool isDisabled = false;
					bool isHide = false;

					bool updateIsDisabled(bool pIsDisabled)
					{
						isDisabled = isDisabled || pIsDisabled;
						return pIsDisabled;
					}
					void updateIsHide(bool pIsDisabled, bool pIsHide)
					{
						isHide = isHide || (pIsDisabled && pIsHide);
					}

					//*****************//
					// GET IS DISABLED //
					//*****************//
					if (disabled != null)
						updateIsHide(updateIsDisabled(true), false);

					if (disabledInEditor != null)
					{
						updateIsHide(updateIsDisabled(!Application.isPlaying), disabledInEditor.hide);
					}

					if (disabledInRuntime != null)
					{
						updateIsHide(updateIsDisabled(Application.isPlaying), disabledInRuntime.hide);
					}

					for (int i = disabledOnList.Length - 1; i >= 0; i--)
					{

						if (isDisabled && isHide) break;

						DisabledOn disabledOn = disabledOnList[i];

						FieldInfo lField2 = null;
						if (disabledOn.RequiredField || disabledOn.onField != string.Empty)
						{
							lField2 = ReflexionUtils.GetField(type, disabledOn.onField, GET_FIELD_ATTR);
							if (lField2 == null) throw new Exception("No field :\"" + disabledOn.onField + "\"");
						}
						updateIsHide(updateIsDisabled(disabledOn.OnCondition(lField2, target)), disabledOn.hide);
					}

					if (disabledIfComponent != null)
					{
						if (typeof(InspectedType).IsSubclassOf(typeof(MonoBehaviour)))
						{
							updateIsHide(updateIsDisabled(disabledIfComponent.Test(((MonoBehaviour)target).gameObject)), disabledIfComponent.hide);
						}
						else
						{
							Debug.LogWarning($"Can't use \"{nameof(DisabledIfComponentAttribute)}\" in non {nameof(MonoBehaviour)} scripts");
						}
					}

					/* Folder start && end before */
					List<EndFolderAttribute> afterFolders = new List<EndFolderAttribute>();

					int foldersAttributelength = folders.Length;

					//function LCloseFolder
					void LCloseFolder()
					{
						if (depth == 0)
						{
							Debug.LogWarning("Can't end a folder when depth is 0 " + target.GetType());
						}
						else
						{
							depth -= 1;
							bool notLastDepth = depth != 0;
							bool thisOpened = opened;
							opened = !notLastDepth;

							int indexDepth = indexByDepth[depth]; //current depth
							var thisType = openedFolderTypes[indexDepth];
							if (notLastDepth)
							{
								int parentIndexDepth = indexByDepth[depth - 1]; //parent depth
								opened = this.opened[parentIndexDepth];
							}

							Debug.Log("end: "+depth+" "+thisType);
							if (thisType.Contains(FolderAttribute.FolderType.Fade))
								EditorGUILayout.EndFadeGroup();

							if (thisType.Contains(FolderAttribute.FolderType.Header))
								EditorGUILayout.EndFoldoutHeaderGroup();

							else if (thisType.Contains(FolderAttribute.FolderType.CheckBox))
								EditorGUILayout.EndFoldoutHeaderGroup();



							indexByDepth.RemoveAt(depth);
							EditorGUI.indentLevel -= 1;
						}
					}
					////////////////////

					for (int i = 0; i < foldersAttributelength; i++)
					{
						FolderBase folder = folders[i];
						if (folder is EndFolderAttribute)
						{
							EndFolderAttribute endFolder = folder as EndFolderAttribute;
							switch (endFolder.position)
							{
								case EndFolderAttribute.Position.Before:
									LCloseFolder();
									break;
								case EndFolderAttribute.Position.After:
									afterFolders.Add(endFolder);
									break;
							}
							continue;
						}

						if (folder is FolderAttribute)
						{
							FolderAttribute foldAttr = folder as FolderAttribute;


							indexByDepth.TryGetOrAddValue(depth, index);
							var thisType = foldAttr.folderType;
							bool thisOpened = this.opened.TryGetOrAddValue(index, foldAttr.openedByDefault);
							openedFolderTypes.TryGetOrAddValue(index, FolderAttribute.FolderType.None);
							float thisFade  = this.fadeRatio.TryGetOrAddValue(index, thisOpened ? MAX_FOLD : MIN_FOLD);
							bool thisFadeFlipFlop  = this.fadeFlipFlop.TryGetOrAddValue(index, !thisOpened);

							//Why use of &&
							// If opened is false, opened will keep its true value
							// If opened is true but the user close the Foldout, opened is now false

							opened = opened && (this.opened[index] = LGetFolder());
							depth += 1;
							index += 1;
							EditorGUI.indentLevel += 1;
							continue;

							bool LGetFolder()
							{
								FolderAttribute.FolderType added = 0;

								bool openCondition = thisFade != 0;
								bool openedInput = true;

								string foldName = foldAttr.name;
								float foldTime = foldAttr.foldTime;

								if (thisType.Contains(FolderAttribute.FolderType.Default))
								{
									openedInput = EditorGUILayout.Foldout(openCondition, foldName);
									added.Add(FolderAttribute.FolderType.Default);
								}

								else if (thisType.Contains(FolderAttribute.FolderType.Header))
								{
									openedInput = EditorGUILayout.BeginFoldoutHeaderGroup(openCondition, foldName);
									added.Add(FolderAttribute.FolderType.Header);
								}

								else if (thisType.Contains(FolderAttribute.FolderType.CheckBox))
								{
									openedInput = EditorGUILayout.BeginFoldoutHeaderGroup(openCondition, foldName);
									added.Add(FolderAttribute.FolderType.CheckBox);
								}

								if (openedInput != openCondition)
									thisFadeFlipFlop = fadeFlipFlop[index] = !thisFadeFlipFlop;

								if (thisType.Contains(FolderAttribute.FolderType.Fade))
								{
									if (openedInput)
									{
										EditorGUILayout.BeginFadeGroup(thisFade);
										added.Add(FolderAttribute.FolderType.Fade);
									}
									fadeRatio[index] = Mathf.Clamp(fadeRatio[index] + (thisFadeFlipFlop ? -1 : 1) * deltaTime / foldTime, MIN_FOLD, MAX_FOLD);
								}

								Debug.Log("start: "+depth +" "+thisType);
								openedFolderTypes[index] = added;
								return openedInput;
							}
						}



					}
					/**/

					if (opened)
					{
						//If is disabled and hidden
						if (isDisabled && isHide)
							continue;


						GUI.enabled = !isDisabled;
						if (customeGuiAttr != null)
						{
							customeGuiAttr.DrawGui(target, target, serializedObject, property);
						}
						else
						{
							CustomGUIAttribute customClassGUIAttribute = field.FieldType.GetCustomAttribute<CustomGUIAttribute>();
							if (customClassGUIAttribute != null)
							{
								customClassGUIAttribute.DrawGui(field.GetValue(target), target, serializedObject, property);
							}
							else EditorGUILayout.PropertyField(property);
						}
						GUI.enabled = true;
					}

					/* Folder end after */
					foreach (var _ in afterFolders)
					{
						LCloseFolder();
					}
					/**/

				} while (property.NextVisible(false));
			}

			serializedObject.ApplyModifiedProperties();
			foreach (var item in fadeRatio)
			{
				if (item != 0 && item != 1)
				{
					this.Repaint();
				}
			}

			if (depth > 0)
			{
				Debug.LogWarning($"You've forgot to close '{depth}' folders in {type}");
			}

			EditorGUI.indentLevel -= depth;
			depth = 0;

			lastTime = Time.realtimeSinceStartup;
		}
	}
}