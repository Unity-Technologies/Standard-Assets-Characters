using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	public class LandAnimatorState : StateMachineBehaviour
	{
		[FormerlySerializedAs("adjustAnimationSpeedBasedOnForwardSpeed")]
		[SerializeField]
		bool m_AdjustAnimationSpeedBasedOnForwardSpeed;
		
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var thirdPersonBrain = animator.GetComponent<ThirdPersonBrain>();
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.OnLandAnimationExit();
			}
		}
		
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var thirdPersonBrain = animator.GetComponent<ThirdPersonBrain>();
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.OnLandAnimationEnter(m_AdjustAnimationSpeedBasedOnForwardSpeed);
			}
		}
	}
}