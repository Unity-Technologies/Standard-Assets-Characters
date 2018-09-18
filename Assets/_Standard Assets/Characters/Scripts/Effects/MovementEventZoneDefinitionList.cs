using System;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	[Serializable]
	public class MovementEventZoneDefinitionList
	{
		[SerializeField, Tooltip("List of movement event libraries for different movement zones")]
		protected MovementEventZoneDefinition[] movementZoneLibraries;
		
		public MovementEventLibrary this[string zoneId]
		{
			get
			{
				foreach (MovementEventZoneDefinition movementEventZoneDefinition in movementZoneLibraries)
				{
					if (movementEventZoneDefinition.id == zoneId)
					{
						return movementEventZoneDefinition.library;
					}
				}

				return null;
			}
		}
	}
}