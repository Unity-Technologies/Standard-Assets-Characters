using StandardAssets.Characters.ThirdPerson.Configs;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	/// <summary>
	/// Passes the locomotion animation start event to the <see cref="ThirdPersonBrain"/>
	/// </summary>
	public class LocomotionAnimatorState : StateMachineBehaviour
	{
		[SerializeField, Tooltip("Movement config used during this state.")]
		GroundMovementConfig m_MovementConfig;
		public GroundMovementConfig movementConfig
		{
			get { return m_MovementConfig; }
		}
		
        /// <summary>
        /// Fires <see cref="ThirdPersonBrain.OnLocomotionAnimationEnter"/>.
        /// </summary>
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var thirdPersonBrain = animator.GetComponent<ThirdPersonBrain>();
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.OnLocomotionAnimationEnter(m_MovementConfig);
			}
		}
	}
}
