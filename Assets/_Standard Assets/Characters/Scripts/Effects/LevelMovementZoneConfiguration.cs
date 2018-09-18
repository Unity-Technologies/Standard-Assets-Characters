using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	[CreateAssetMenu(fileName = "Level Movement Zone Configuration",
		menuName = "Standard Assets/Characters/Level Movement Zone Configuration", order = 1)]
	public class LevelMovementZoneConfiguration : ScriptableObject
	{
		[SerializeField, Tooltip("List of movement event libraries for different movement zones")]
		protected MovementEventZoneDefinitionList zonesDefinition;

		public MovementEventLibrary this[string zoneId]
		{
			get
			{
				return zonesDefinition[zoneId];
			}
		}
	}
}