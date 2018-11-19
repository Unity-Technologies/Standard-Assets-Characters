using StandardAssets.Characters.Physics;
using UnityEditor;

namespace Editor
{
	/// <summary>
	/// Custom editor for rendering advance fields in the <see cref="OpenCharacterController"/>
	/// </summary>
	[CustomEditor(typeof(OpenCharacterController))]
	public class OpenCharacterControllerEditor : UnityEditor.Editor
	{
		// List of names of advanced fields
		readonly string[] m_AdvancedFields = 
		{
			"m_SkinWidth", 
			"m_MinMoveDistance", 
			"m_LocalHumanControlled",
			"m_CanSlideAgainstCeiling", 
			"m_SendColliderHitMessages", 
			"m_QueryTriggerInteraction"
		};

		// Tracks the whether the advanced foldout is open/collapse
		bool m_AdvancedFoldOut;
		

		/// <summary>
		/// Renders advanced fields
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawPropertiesExcluding(serializedObject, m_AdvancedFields);

			EditorGUILayout.Space();
			serializedObject.DrawFieldsUnderFoldout("Advanced", m_AdvancedFields, ref m_AdvancedFoldOut);
		}
	}
}
