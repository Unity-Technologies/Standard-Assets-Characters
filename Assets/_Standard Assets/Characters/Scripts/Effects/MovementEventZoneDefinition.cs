using System;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	[Serializable]
	public class MovementEventZoneDefinition
	{
		[SerializeField, Tooltip("The ID of the zone used to play the effect")]
		protected string zoneId = "concrete";

		[SerializeField, Tooltip("The corresponding library of effects")]
		protected MovementEventLibrary zoneLibrary;
		
		public string id
		{
			get { return zoneId; }
		}

		public MovementEventLibrary library
		{
			get { return library; }
		}
	}
}