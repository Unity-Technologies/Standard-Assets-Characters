
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	public class PhysicsJumpAnimatorState : StateMachineBehaviour 
	{
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var animationController = animator.GetComponent<ThirdPersonBrain>().animationControl;
			if (animationController != null)
			{
				animationController.OnPhysicsJumpAnimationExit();
			}
		}
		
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var animationController = animator.GetComponent<ThirdPersonBrain>().animationControl;
			if (animationController != null)
			{
				animationController.OnPhysicsJumpAnimationEnter();
			}
		}
	}
}
