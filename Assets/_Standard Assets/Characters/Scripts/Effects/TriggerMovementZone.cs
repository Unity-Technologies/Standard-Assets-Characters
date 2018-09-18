using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Implementation of a movement zone using Triggers
	/// </summary>
	[RequireComponent(typeof(Collider))]
	public class TriggerMovementZone : MovementZone
	{
		/// <summary>
		/// Change the movement event library on trigger enter
		/// </summary>
		/// <param name="other"></param>
		private void OnTriggerEnter(Collider other)
		{
			CharacterBrain brain = other.GetComponent<CharacterBrain>();
			if (brain != null)
			{
				Trigger(brain);
			}
		}
		
		/// <summary>
		/// Change the movement event library back to default/starting
		/// </summary>
		/// <param name="other"></param>
		private void OnTriggerExit(Collider other)
		{
			CharacterBrain brain = other.GetComponent<CharacterBrain>();
			if (brain != null)
			{
				ExitTrigger(brain);
			}
		}
	}
}