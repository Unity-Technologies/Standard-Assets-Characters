using StandardAssets.Characters.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Implementation of a movement zone using Trigger
	/// </summary>
	[RequireComponent(typeof(Collider))]
	public class MovementZoneTrigger : MonoBehaviour
	{
		[FormerlySerializedAs("zoneId")]
		[SerializeField, Tooltip("ID used to correspond with a zone definition")]
		MovementZoneId m_ZoneId;
		
		/// <summary>
		/// Change the movement event library on trigger enter
		/// </summary>
		/// <param name="other"></param>
		void OnTriggerEnter(Collider other)
		{
			var brain = other.GetComponent<CharacterBrain>();
			if (brain != null)
			{
				Trigger(brain);
			}
		}
		
		/// <summary>
		/// Change the movement event library back to default/starting
		/// </summary>
		/// <param name="other"></param>
		void OnTriggerExit(Collider other)
		{
			var brain = other.GetComponent<CharacterBrain>();
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
			
			brain.ChangeMovementZone(m_ZoneId);
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