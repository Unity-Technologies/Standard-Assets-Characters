using UnityEngine;

namespace StandardAssets.Characters.Effects.Configs
{
	/// <summary>
	/// Configuration of default effects for different zones in a level
	/// </summary>
	[CreateAssetMenu(fileName = "Level Movement Zone Configuration",
		menuName = "Standard Assets/Characters/Level Movement Zone Configuration", order = 1)]
	public class LevelMovementZoneConfig : ScriptableObject
	{
		[SerializeField, Tooltip("Default movement event zone ID")]
		PhysicMaterial m_DefaultPhysicMaterial;
		
		[SerializeField, Tooltip("List of movement event libraries for different movement zones")]
		MovementEventZoneDefinitionList m_ZonesDefinition;

		/// <summary>
		/// Indexer to return a <see cref="MovementEventLibrary"/> based on a PhysicMaterial
		/// </summary>
		public MovementEventLibrary this[PhysicMaterial physicMaterial] { get { return m_ZonesDefinition[physicMaterial]; } }

		/// <summary>
		/// Gets the default <see cref="MovementEventLibrary"/>
		/// </summary>
		public MovementEventLibrary defaultLibrary { get { return this[m_DefaultPhysicMaterial]; } }
		
		/// <summary>
		/// Gets the default Ids
		/// </summary>
		public PhysicMaterial defaultPhysicMaterial { get { return m_DefaultPhysicMaterial; } }
	}
}