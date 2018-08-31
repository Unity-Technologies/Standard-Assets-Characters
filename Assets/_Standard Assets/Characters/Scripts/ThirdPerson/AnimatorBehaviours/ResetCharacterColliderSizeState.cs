using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	/// <summary>
	/// Allows an animation state to reset the OpenCharacterController's capsule height and offset.
	/// </summary>
	public class ResetCharacterColliderSizeState : StateMachineBehaviour
	{
		/// <summary>
		/// Reset the height?
		/// </summary>
		[Tooltip("Reset the height?")]
		[SerializeField]
		private bool resetHeight = true;
		
		/// <summary>
		/// Reset the offset/center?
		/// </summary>
		[Tooltip("Reset the offset/center?")]
		[SerializeField]
		private bool resetOffset = true;

		/// <summary>
		/// Preserve the foot position when only resetting the height? (This is ignored when resetting the center.)
		/// </summary>
		[Tooltip("Preserve the foot position when only resetting the height? (This is ignored when resetting the center.)")]
		[SerializeField]
		private bool preserveFootPosition = true;

		private OpenCharacterControllerPhysics openCharacterControllerPhysics;
		private OpenCharacterController openCharacterController;
		
		/// <inheritdoc />
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (openCharacterControllerPhysics == null)
			{
				openCharacterControllerPhysics = animator.GetComponentInChildren<OpenCharacterControllerPhysics>();
				if (openCharacterControllerPhysics == null)
				{
					return;
				} 
				openCharacterController = openCharacterControllerPhysics.GetOpenCharacterController();
				if (openCharacterController == null)
				{
					return;
				}
			}

			HandleReset();
		}
		
		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			// Have to call this every frame, in case we're busy transitioning from another state that's updating the size as well.
			HandleReset();
		}

		private void HandleReset()
		{
			if (openCharacterController == null)
			{
				return;
			}

			if (resetHeight &&
			    resetOffset)
			{
				openCharacterController.ResetHeightAndCenter(true, false);
			}
			else if (resetHeight)
			{
				openCharacterController.ResetHeight(preserveFootPosition, true, false);
			}
			else if (resetOffset)
			{
				openCharacterController.ResetCenter(true, false);
			}
		}
	}
}