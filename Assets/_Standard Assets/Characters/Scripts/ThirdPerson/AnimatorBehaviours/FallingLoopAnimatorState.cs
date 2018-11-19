using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	/// <summary>
	/// Allows the animation state to call OnFallingLoopAnimationEnter on the <see cref="ThirdPersonBrain"/> 
	/// </summary>
	public class FallingLoopAnimatorState : StateMachineBehaviour 
	{
        /// <summary>
        /// Fires <see cref="ThirdPersonBrain.OnFallingLoopAnimationEnter"/>.
        /// </summary>
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var thirdPersonBrain = animator.GetComponent<ThirdPersonBrain>();
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.OnFallingLoopAnimationEnter();
			}
		}
	}
}
