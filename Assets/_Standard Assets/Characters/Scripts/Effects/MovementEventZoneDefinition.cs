using System;
using StandardAssets.Characters.Attributes;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Defines which zone ID matches to which <see cref="MovementEventLibrary"/>
	/// </summary>
	[Serializable]
	public class MovementEventZoneDefinition
	{
		[MovementZoneId, SerializeField, Tooltip("The ID of the zone used to play the effect")]
		protected string zoneId = "concrete";

		[SerializeField, Tooltip("The corresponding library of effects")]
		protected MovementEventLibrary zoneLibrary;
		
		/// <summary>
		/// Gets the zoneId
		/// </summary>
		public string id
		{
			get { return zoneId; }
		}

		/// <summary>
		/// Gets the <see cref="MovementEventLibrary"/>
		/// </summary>
		public MovementEventLibrary library
		{
			get { return zoneLibrary; }
		}
	}
}