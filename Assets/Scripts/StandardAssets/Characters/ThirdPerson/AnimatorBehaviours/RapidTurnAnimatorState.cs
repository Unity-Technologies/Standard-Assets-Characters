using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	public class RapidTurnAnimatorState : StateMachineBehaviour 
	{
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var animationController = animator.GetComponent<ThirdPersonAnimationController>();
			if (animationController != null)
			{
				animationController.OnRapidTurnComplete();
			}
		}
	}
}
