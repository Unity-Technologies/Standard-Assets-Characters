using Cinemachine;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Listens to impulses broadcasted by Cinemachine Impulse Source and moves a weapon object 
	/// </summary>
	public class WeaponImpulseListener : MonoBehaviour
	{
		[SerializeField, Tooltip("The object to be moved by the impulse. Cannot be the same object as the listener")]
		protected GameObject objectToMove;
		[SerializeField, Tooltip("Impulse events on channels not included in the mask will be ignored."), CinemachineImpulseChannelProperty]
		protected int channelMask = 1;
		[SerializeField, Tooltip("Gain to apply to the Impulse signal.  1 is normal strength.  Setting this to 0 completely mutes the signal.")]
		protected float gain = 1f;
		[SerializeField, Tooltip("Enable this to perform distance calculation in 2D (ignore Z)")]
		protected bool use2DDistance = false;

		/// <summary>
		/// Handles the Impulses from the Cinemachine Input Sources
		/// </summary>
		private void Update()
		{
			Vector3 position = Vector3.zero;
			Quaternion rotation = Quaternion.identity;
			if (CinemachineImpulseManager.Instance.GetImpulseAt(transform.position, use2DDistance, channelMask, out position, out rotation))
			{		
				objectToMove.transform.position = transform.position + position * -gain;
				rotation = Quaternion.SlerpUnclamped(Quaternion.identity, rotation, -gain);
				objectToMove.transform.rotation = transform.rotation * rotation;
			}
		}
	}
}