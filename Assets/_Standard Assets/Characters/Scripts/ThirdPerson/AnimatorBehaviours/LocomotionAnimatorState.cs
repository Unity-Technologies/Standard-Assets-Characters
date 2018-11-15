using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	/// <summary>
	/// Passes the locomotion animation start event to the <see cref="ThirdPersonBrain"/>
	/// </summary>
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
