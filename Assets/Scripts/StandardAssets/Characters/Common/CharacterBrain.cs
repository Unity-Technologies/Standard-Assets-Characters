using StandardAssets.Characters.Effects;
using UnityEngine;

namespace StandardAssets.Characters.Common
{
	/// <summary>
	/// Abstract bass class for character brains
	/// </summary>
	public abstract class CharacterBrain : MonoBehaviour
	{
		[SerializeField]
		protected CameraAnimationManager cameraAnimations;
		
		public abstract MovementEventHandler movementEventHandler { get; }

		protected void SetAnimation(string animation)
		{
			if (cameraAnimations == null)
			{
				Debug.LogWarning("No camera animation manager setup");
				return;
			}
			
			cameraAnimations.SetAnimation(animation);
		}
	}
}