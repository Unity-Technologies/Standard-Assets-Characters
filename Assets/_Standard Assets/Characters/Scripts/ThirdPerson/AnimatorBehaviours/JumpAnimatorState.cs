using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	public class JumpAnimatorState : StateMachineBehaviour 
	{
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var brain = animator.GetComponent<ThirdPersonBrain>();
			if (brain != null)
			{
				brain.thirdPersonMotor.OnJumpAnimationComplete();
			}
		}
	}
}
