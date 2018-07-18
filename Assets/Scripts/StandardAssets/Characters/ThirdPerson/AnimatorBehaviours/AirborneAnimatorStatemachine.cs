using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	public class AirborneAnimatorStatemachine : StateMachineBehaviour 
	{
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var animationController = animator.GetComponent<ThirdPersonAnimationController>();
			if (animationController != null)
			{
				animationController.AirborneStateEnter();
			}
		}
		
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var animationController = animator.GetComponent<ThirdPersonAnimationController>();
			if (animationController != null)
			{
				animationController.AirborneStateExit();
			}
		}
	}
}
