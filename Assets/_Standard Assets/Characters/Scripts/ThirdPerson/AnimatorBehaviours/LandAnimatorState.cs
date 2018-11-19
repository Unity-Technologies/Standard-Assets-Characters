using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	/// <summary>
	/// Allow the animation state to pass Landing animation start and end events to the <see cref="ThirdPersonBrain"/> 
	/// </summary>
	public class LandAnimatorState : StateMachineBehaviour
	{
		[SerializeField, Tooltip("Should the animator state speed be modified based on forward speed? Recommended for " +
			 "a roll animation")]
		bool m_AdjustAnimationSpeedBasedOnForwardSpeed;

		
        /// <summary>
        /// Fires <see cref="ThirdPersonBrain.OnLandAnimationExit"/>.
        /// </summary>
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var thirdPersonBrain = animator.GetComponent<ThirdPersonBrain>();
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.OnLandAnimationExit();
			}
		}
		
        /// <summary>
        /// Fires <see cref="ThirdPersonBrain.OnLandAnimationEnter"/>.
        /// </summary>
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var thirdPersonBrain = animator.GetComponent<ThirdPersonBrain>();
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.OnLandAnimationEnter(m_AdjustAnimationSpeedBasedOnForwardSpeed);
			}
		}
	}
}