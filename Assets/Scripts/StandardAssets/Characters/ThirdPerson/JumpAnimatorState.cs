using StandardAssets.Characters.Physics;
using StandardAssets.Characters.ThirdPerson;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class JumpAnimatorState : StateMachineBehaviour 
	{
		/// <summary>
		/// Updates the predicted fall distance during animator state
		/// </summary>
		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var animationController = animator.GetComponent<ThirdPersonAnimationController>();
			var baseCharacterPhysics = animator.GetComponent<BaseCharacterPhysics>();
			if (animationController == null || baseCharacterPhysics == null)
			{
				return;
			}
			animationController.UpdatePredictedFallDistance(baseCharacterPhysics.GetPredicitedFallDistance());
		}
	}
}
