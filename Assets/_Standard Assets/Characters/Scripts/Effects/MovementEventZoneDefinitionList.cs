using System;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// A set of <see cref="MovementEventLibrary"/> for different zone IDs
	/// </summary>
	[Serializable]
	public class MovementEventZoneDefinitionList
	{
		[SerializeField, Tooltip("List of movement event libraries for different movement zones")]
		protected MovementEventZoneDefinition[] movementZoneLibraries;
		
		/// <summary>
		/// Gets the Gets the <see cref="MovementEventLibrary"/> for a specified zoneId for a specified zoneId
		/// </summary>
		/// <param name="zoneId">The zoneId needed to look up the <see cref="MovementEventLibrary"/></param>
		/// <value>Gets the <see cref="MovementEventLibrary"/> for a specified zoneId. returns null if the zoneId does not have an associated <see cref="MovementEventLibrary"/></value>
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