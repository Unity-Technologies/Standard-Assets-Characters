using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	public class JumpAnimatorState : StateMachineBehaviour 
	{
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var motor = animator.GetComponent<ThirdPersonBrain>().currentMotor as RootMotionThirdPersonMotor;
			if (motor != null)
			{
				motor.OnJumpAnimationComplete();
				
			}
		}
	}
}
