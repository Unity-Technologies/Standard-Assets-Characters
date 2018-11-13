using StandardAssets.Characters.Physics;
using UnityEditor;

namespace Editor
{
	[CustomEditor(typeof(OpenCharacterController))]
	public class OpenCharacterControllerEditor : UnityEditor.Editor
	{
		readonly string[] advancedFields = 
		{
			"m_SkinWidth", "m_MinMoveDistance", "m_LocalHumanControlled",
			"m_CanSlideAgainstCeiling", "m_SendColliderHitMessages", "m_QueryTriggerInteraction"
		};

		bool advancedFoldOut;

		public override void OnInspectorGUI()
		{
			DrawPropertiesExcluding(serializedObject, advancedFields);

			EditorGUILayout.Space();
			serializedObject.DrawFoldoutBoxedFields("Advanced Settings", advancedFields, ref advancedFoldOut);
		}
	}
}
