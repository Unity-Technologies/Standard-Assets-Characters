using StandardAssets.Characters.Physics;
using UnityEditor;

namespace Editor
{
	/// <summary>
	/// COMMENT TODO
	/// </summary>
	[CustomEditor(typeof(OpenCharacterController))]
	public class OpenCharacterControllerEditor : UnityEditor.Editor
	{
		// COMMENT TODO
		readonly string[] advancedFields = 
		{
			"m_SkinWidth", 
			"m_MinMoveDistance", 
			"m_LocalHumanControlled",
			"m_CanSlideAgainstCeiling", 
			"m_SendColliderHitMessages", 
			"m_QueryTriggerInteraction"
		};

		// COMMENT TODO
		bool advancedFoldOut;
		

		/// <summary>
		/// COMMENT TODO
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawPropertiesExcluding(serializedObject, advancedFields);

			EditorGUILayout.Space();
			serializedObject.DrawFieldsUnderFoldout("Advanced", advancedFields, ref advancedFoldOut);
		}
	}
}
