using System;
using StandardAssets.Characters.Attributes;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	[Serializable]
	public class MovementEventZoneDefinition
	{
		[MovementZoneId, SerializeField, Tooltip("The ID of the zone used to play the effect")]
		protected string zoneId = "concrete";

		[SerializeField, Tooltip("The corresponding library of effects")]
		protected MovementEventLibrary zoneLibrary;
		
		public string id
		{
			get { return zoneId; }
		}

		public MovementEventLibrary library
		{
			get { return zoneLibrary; }
		}
	}
}