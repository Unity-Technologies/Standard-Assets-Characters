using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.ThirdPerson
{
	public class RapidTurnAnimatorState : StateMachineBehaviour
	{
		[FormerlySerializedAs("parameterName")]
		public string clipOffsetParameter = "";

		public string clipSpeedParameter = "";
		
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var animationController = animator.GetComponent<ThirdPersonAnimationController>();
			if (animationController != null)
			{
				animationController.OnRapidTurnComplete();
			}
		}

		private void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
		{
			if (animatorStateInfo.normalizedTime >= 1 - Time.deltaTime * animator.GetFloat(clipSpeedParameter) - animator.GetFloat(clipOffsetParameter))
			{
				animator.SetTrigger("EXIT");
			}
		}
	}
}
