using StandardAssets.Characters.Attributes;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// A project wide list of Zone IDs
	/// </summary>
	[CreateAssetMenu(fileName = "Movement Zone IDs",
		menuName = "Standard Assets/Characters/Create Movement Zone IDs", order = 1)]
	public class MovementZoneIdDefinition : ScriptableObject
	{
		[SerializeField, Tooltip("All the zone IDs for the project")]
		protected string[] zoneIds;

		/// <summary>
		/// Gets the array of zoneIds for the project
		/// </summary>
		public string[] ids
		{
			get { return zoneIds; }
		}
	}
}