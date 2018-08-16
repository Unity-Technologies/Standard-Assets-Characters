using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	public class FallingLoopAnimatorState : StateMachineBehaviour 
	{
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var animationControl = animator.GetComponent<ThirdPersonBrain>().animationControl;
			if (animationControl != null)
			{
				animationControl.OnFallingLoopAnimationEnter();
			}
		}
	}
}
