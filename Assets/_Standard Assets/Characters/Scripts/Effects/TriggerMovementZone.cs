using StandardAssets.Characters.Attributes;
using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Implementation of a movement zone using Triggers
	/// </summary>
	[RequireComponent(typeof(Collider))]
	public class TriggerMovementZone : MonoBehaviour
	{
		[SerializeField]
		protected MovementZoneId zoneId;
		
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
		
		/// <summary>
		/// Helper method for triggering movement events
		/// </summary>
		/// <param name="brain"></param>
		protected void Trigger(CharacterBrain brain)
		{
			if (brain == null)
			{
				return;
			}
			
			brain.ChangeMovementZone(zoneId);
		}
		
		/// <summary>
		/// Triggering movement event that resets event library to default 
		/// </summary>
		/// <param name="brain"></param>
		protected void ExitTrigger(CharacterBrain brain)
		{
			if (brain == null)
			{
				return;
			}
			
			brain.ChangeMovementZone(null);
		}
	}
}