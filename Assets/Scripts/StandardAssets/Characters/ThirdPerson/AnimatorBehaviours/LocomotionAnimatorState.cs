using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	public class LocomotionAnimatorState : StateMachineBehaviour 
	{
		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
		{
			var animationController = animator.GetComponent<ThirdPersonBrain>().animationControl;
			if (animationController != null)
			{
				animationController.LocomotionStateUpdate();
			}
		}
	}
}
