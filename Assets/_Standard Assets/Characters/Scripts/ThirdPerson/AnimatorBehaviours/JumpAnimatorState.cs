
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	/// <summary>
	/// Allow the animation state to pass Jump animation start and end events to the <see cref="ThirdPersonBrain"/> 
	/// </summary>
	public class JumpAnimatorState : StateMachineBehaviour 
	{
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var thirdPersonBrain = animator.GetComponent<ThirdPersonBrain>();
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.OnJumpAnimationExit();
			}
		}
		
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var thirdPersonBrain = animator.GetComponent<ThirdPersonBrain>();
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.OnJumpAnimationEnter();
			}
		}
	}
}
