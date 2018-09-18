using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// An abstract representation of MovementZone
	/// </summary>
	public abstract class MovementZone : MonoBehaviour
	{
		[SerializeField]
		protected string zoneId;

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
			
			//handler.SetStartingMovementEventLibrary();
		}
	}
}