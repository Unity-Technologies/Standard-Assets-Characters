using StandardAssets.Characters.Common;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	/// <summary>
	/// Allows an animation state to reset the OpenCharacterController's capsule height and offset.
	/// </summary>
	public class ResetCharacterColliderSizeState : StateMachineBehaviour
	{
		[SerializeField, Tooltip("Reset the height?")]
		bool m_ResetHeight = true;
		
		[SerializeField, Tooltip("Reset the offset/center?")]
		bool m_ResetOffset = true;

		[SerializeField, Tooltip("Preserve the foot position when only resetting the height? (This is ignored when resetting the center.)")]
		bool m_PreserveFootPosition = true;

		// Reference to the OpenCharacterController in order to adjust the collider.
		OpenCharacterController m_OpenCharacterController;
		
		/// <summary>
		/// Caches a reference to the <see cref=" OpenCharacterController"/>.
		/// </summary>
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (m_OpenCharacterController == null)
			{
				m_OpenCharacterController = animator.GetComponent<OpenCharacterController>();
			}
			ResetControllerCollider();
		}
		
        /// <summary>
        /// Updates the <see cref=" OpenCharacterController"/> collider.
        /// </summary>
		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			// Have to call this every frame, in case we're busy transitioning from another state that's updating the size as well.
			ResetControllerCollider();
		}

		// Resets the m_OpenCharacterController collider.
		void ResetControllerCollider()
		{
			if (m_OpenCharacterController == null)
			{
				return;
			}

			if (m_ResetHeight &&
			    m_ResetOffset)
			{
				m_OpenCharacterController.ResetHeightAndCenter(true, false);
			}
			else if (m_ResetHeight)
			{
				m_OpenCharacterController.ResetHeight(m_PreserveFootPosition, true, false);
			}
			else if (m_ResetOffset)
			{
				m_OpenCharacterController.ResetCenter(true, false);
			}
		}
	}
}