using UnityEditor;
using Com.GitHub.Knose1.Common.AnimationUtils;

namespace Com.GitHub.Knose1.Common.Editor
{
	[CustomEditor(typeof(AnimatorSequencer), true)]
	public class AnimatorSequencerEditor : UnityEditor.Editor
	{
		private const string EXECUTE_ON_START = "executeOnStart";
		private const string SEQUENCE_TYPE = "sequenceType";
		private const string COMPUTE_ANIMATORS_ON_EXECUTE = "computeAnimatorsOnExecute";
		private const string ANIMATOR_SEQUENCE_FUNCTION = "animatorSequenceFunction";
		private const string ANIMATOR_SEQUENCE_FUNCTION_DEBUG = "animatorSequenceFunctionDebug";
		private const string ANIMATOR_SEQUENCE_FUNCTION_PARAMETER = "animatorSequenceFunctionParameter";
		private const string ANIMATOR_SEQUENCE_LIST = "animatorSequenceList";

		SerializedProperty executeOnStart;
		SerializedProperty sequenceType;
		SerializedProperty computeAnimatorsOnExecute;
		SerializedProperty animatorSequenceFunction;
		SerializedProperty animatorSequenceFunctionDebug;
		SerializedProperty animatorSequenceFunctionParameter;
		SerializedProperty animatorSequenceList;


		private void OnEnable()
		{
			executeOnStart						= serializedObject.FindProperty( EXECUTE_ON_START );
			sequenceType						= serializedObject.FindProperty( SEQUENCE_TYPE );
			computeAnimatorsOnExecute			= serializedObject.FindProperty( COMPUTE_ANIMATORS_ON_EXECUTE );
			animatorSequenceFunction			= serializedObject.FindProperty( ANIMATOR_SEQUENCE_FUNCTION );
			animatorSequenceFunctionDebug		= serializedObject.FindProperty( ANIMATOR_SEQUENCE_FUNCTION_DEBUG );
			animatorSequenceFunctionParameter	= serializedObject.FindProperty( ANIMATOR_SEQUENCE_FUNCTION_PARAMETER );
			animatorSequenceList				= serializedObject.FindProperty( ANIMATOR_SEQUENCE_LIST );

		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(executeOnStart);
			EditorGUILayout.PropertyField(computeAnimatorsOnExecute);
			EditorGUILayout.PropertyField(sequenceType);

			AnimatorSequencer.SequenceType type = (AnimatorSequencer.SequenceType)sequenceType.enumValueIndex;

			switch (type)
			{
				case AnimatorSequencer.SequenceType.MathFunction:
					EditorGUILayout.PropertyField(animatorSequenceFunction);
					EditorGUILayout.PropertyField(animatorSequenceFunctionDebug);
					EditorGUILayout.PropertyField(animatorSequenceFunctionParameter);
					break;
				case AnimatorSequencer.SequenceType.List:
					EditorGUILayout.PropertyField(animatorSequenceList);
					break;
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
