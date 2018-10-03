using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	public class LocomotionAnimatorState : StateMachineBehaviour 
	{
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var thirdPersonBrain = animator.GetComponent<ThirdPersonBrain>();
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.OnLocomotionAnimationEnter();
			}
		}
	}
}
