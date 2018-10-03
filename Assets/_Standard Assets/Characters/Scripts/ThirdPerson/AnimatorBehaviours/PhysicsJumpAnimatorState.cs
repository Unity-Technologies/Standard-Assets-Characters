
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	public class PhysicsJumpAnimatorState : StateMachineBehaviour 
	{
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var thirdPersonBrain = animator.GetComponent<ThirdPersonBrain>();
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.OnPhysicsJumpAnimationExit();
			}
		}
		
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var thirdPersonBrain = animator.GetComponent<ThirdPersonBrain>();
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.OnPhysicsJumpAnimationEnter();
			}
		}
	}
}
