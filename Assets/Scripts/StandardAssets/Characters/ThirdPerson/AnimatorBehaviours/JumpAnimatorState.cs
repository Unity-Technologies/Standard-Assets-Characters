using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	public class JumpAnimatorState : StateMachineBehaviour 
	{
		/// <summary>
		/// Updates the predicted fall distance during animator state
		/// </summary>
		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
//			var animationController = animator.GetComponent<ThirdPersonBrain>().animationControl;
//			var baseCharacterPhysics = animator.GetComponent<BaseCharacterPhysics>();
//			if (animationController == null || baseCharacterPhysics == null)
//			{
//				return;
//			}
//			animationController.UpdatePredictedFallDistance(baseCharacterPhysics.GetPredicitedFallDistance());
		}

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
//			var motor = animator.GetComponent<ThirdPersonBrain>().rootMotionThirdPersonMotor;
//			if (motor != null)
//			{
//				motor.OnJumpAnimationComplete();
//			}
		}
	}
}
