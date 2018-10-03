using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	public class LandAnimatorState : StateMachineBehaviour
	{
		[SerializeField]
		protected bool adjustAnimationSpeedBasedOnForwardSpeed;
		
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var thirdPersonBrain = animator.GetComponent<ThirdPersonBrain>();
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.OnLandAnimationExit();
			}
		}
		
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var thirdPersonBrain = animator.GetComponent<ThirdPersonBrain>();
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.OnLandAnimationEnter(adjustAnimationSpeedBasedOnForwardSpeed);
			}
		}
	}
}