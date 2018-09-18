using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	[CreateAssetMenu(fileName = "Movement Zone IDs",
		menuName = "Standard Assets/Characters/Create Movement Zone IDs", order = 1)]
	public class MovementZoneIdDefinition : ScriptableObject
	{
		[SerializeField]
		protected string[] zoneIds;

		public string[] ids
		{
			get { return zoneIds; }
		}
	}
}