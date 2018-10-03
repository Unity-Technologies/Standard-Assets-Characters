using System;
using UnityEngine;
using Util;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace StandardAssets.Characters.ThirdPerson
{
	/// <inheritdoc />
	/// <summary>
	/// Animation extension of TurnaroundBehaviour. Rotates the character to the target angle while playing an animation.
	/// </summary>
	/// <remarks>This turnaround type should be used to improve fidelity at the cost of responsiveness.</remarks>
	[Serializable]
	public class AnimationTurnaroundBehaviour : TurnaroundBehaviour
	{
		private enum State
		{
			Inactive,
			WaitingForTransition,
			Transitioning,
			TurningAnimation,
			TransitioningOut
		}
		
		/// <summary>
		/// Model to store data per animation turnaround
		/// </summary>
		[Serializable]
		protected class AnimationInfo
		{
			[Tooltip("Animation state name.")]
			public string name;
			[Tooltip("Animation play speed.")]
			public float speed = 1.0f;
			[Tooltip("Head look at angle scale during animation.")]
			public float headTurnScale = 1.0f;

			public AnimationInfo(string name)
			{
				this.name = name;
			}
		}

		// the data for each animation turnaround
		[SerializeField, Tooltip("Data for run 180 left turn animation")]
		protected AnimationInfo runLeftTurn = new AnimationInfo("RunForwardTurnLeft180");
		[SerializeField, Tooltip("Data for run 180 right turn animation")]
		protected AnimationInfo runRightTurn = new AnimationInfo("RunForwardTurnRight180_Mirror");
		[SerializeField, Tooltip("Data for sprint 180 left turn animation")]
		protected AnimationInfo sprintLeftTurn = new AnimationInfo("RunForwardTurnLeft180");
		[SerializeField, Tooltip("Data for sprint 180 right turn animation")]
		protected AnimationInfo sprintRightTurn = new AnimationInfo("RunForwardTurnRight180_Mirror");
		[SerializeField, Tooltip("Data for idle 180 left turn animation")]
		protected AnimationInfo idleLeftTurn = new AnimationInfo("IdleTurnLeft180");
		[SerializeField, Tooltip("Data for idle 180 right turn animation")]
		protected AnimationInfo	idleRightTurn = new AnimationInfo("IdleTurnRight180_Mirror");

		[SerializeField, Tooltip("Curve used to determine rotation during animation")] 
		protected AnimationCurve rotationCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

		[SerializeField, Tooltip("Value used to determine if a run turn should be used")]
		protected float normalizedRunSpeedThreshold = 0.1f;
		
		[SerializeField, Tooltip("Duration of the cross fade into turn animation")] 
		protected float crossfadeDuration = 0.125f;

		private float targetAngle, // target y rotation angle in degrees
			cachedAnimatorSpeed, // speed of the animator prior to starting an animation turnaround
			cacheForwardSpeed; // forwards speed of the motor prior to starting an animation turnaround
		private Quaternion startRotation; // rotation of the character as turnaround is started
		private AnimationInfo currentAnimationInfo; // currently selected animation info
		private ThirdPersonBrain thirdPersonBrain;
		private Transform transform; // character's transform
		private State state; // state used to determine where to retrieve animator normalized time from

		/// <inheritdoc />
		public override float headTurnScale
		{
			get
			{
				return currentAnimationInfo == null ? 1.0f : currentAnimationInfo.headTurnScale;
			}
		}

		private Animator animator
		{
			get { return thirdPersonBrain.animator; }
		}

		/// <inheritdoc/>
		public override void Init(ThirdPersonBrain brain)
		{
			thirdPersonBrain = brain;
			transform = brain.transform;
		}

		/// <summary>
		/// Rotates the character toward <see cref="targetAngle"/> using the animation's normalized progress/>
		/// </summary>
		public override void Update()
		{
			if (!isTurningAround)
			{
				return;
			}

			// check if next or current state normalized time is appropriate.
			float currentStateNormalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
			
			float normalizedTime = 0.0f;
			switch (state)
			{
				case State.WaitingForTransition: // waiting for transition to start use 0 until start
					if (animator.IsInTransition(0))
					{
						state = State.Transitioning;
					}
					break;
				case State.Transitioning: // transitioning into animation use next state time until transition is complete.
					if (!animator.IsInTransition(0))
					{
						state = State.TurningAnimation;
						normalizedTime = currentStateNormalizedTime;
					}
					else
					{
						// transitioning, use next state's normalized time.
						normalizedTime = animator.GetNextAnimatorStateInfo(0).normalizedTime;
					}
					break;
				case State.TurningAnimation: // playing turn use current state until turn is complete
					normalizedTime = currentStateNormalizedTime;
					if (normalizedTime >= 1.0f)
					{
						state = State.TransitioningOut;
						return;
					}

					break;
				case State.TransitioningOut: // transition out of turn don't rotate just wait for transition end
					if (!animator.IsInTransition(0))
					{
						state = State.Inactive;
						animator.speed = cachedAnimatorSpeed;
						EndTurnAround();
					}
					return; // don't rotate character
			}
			
			thirdPersonBrain.UpdateForwardSpeed(cacheForwardSpeed, float.MaxValue);
			
			float rotationProgress = rotationCurve.Evaluate(normalizedTime);
			transform.rotation = Quaternion.AngleAxis(rotationProgress * targetAngle, Vector3.up) * startRotation;
		}

		/// <inheritdoc />
		public override Vector3 GetMovement()
		{
			if (currentAnimationInfo == idleLeftTurn || currentAnimationInfo == idleRightTurn)
			{
				return Vector3.zero;
			}
			return animator.deltaPosition;
		}

		protected override void FinishedTurning()
		{
		}

		/// <summary>
		/// Using the target angle and <see cref="ThirdPersonAnimationController.isRightFootPlanted"/> selects the
		/// appropriate animation to cross fade into.
		/// </summary>
		/// <param name="angle">The target angle in degrees.</param>
		protected override void StartTurningAround(float angle)
		{
			targetAngle = MathUtilities.Wrap180(angle);
			currentAnimationInfo = GetCurrent(thirdPersonBrain.animatorForwardSpeed, angle > 0.0f,
				!thirdPersonBrain.isRightFootPlanted);

			startRotation = transform.rotation;
			animator.CrossFade(currentAnimationInfo.name, crossfadeDuration, 0, 0.0f);

			cachedAnimatorSpeed = animator.speed;
			animator.speed = currentAnimationInfo.speed;

			cacheForwardSpeed = thirdPersonBrain.animatorForwardSpeed;

			state = State.WaitingForTransition;
		}

		/// <summary>
		/// Determines which animation should be played
		/// </summary>
		/// <param name="forwardSpeed">Character's normalized forward speed</param>
		/// <param name="turningClockwise">Is the character turning clockwise</param>
		/// <param name="leftFootPlanted">Is the character's left foot currently planted</param>
		/// <returns>The determined AnimationInfo</returns>
		private AnimationInfo GetCurrent(float forwardSpeed, bool turningClockwise, bool leftFootPlanted)
		{
			// idle turn
			if (forwardSpeed < normalizedRunSpeedThreshold)
			{
				return turningClockwise ? idleRightTurn : idleLeftTurn;
			}
			
			// < 180 turn
			if (targetAngle < 170.0f || targetAngle > 190.0f)
			{
				return CurrentRun(forwardSpeed, turningClockwise);
			}
			
			// 180 turns should be based on the grounded foot
			targetAngle = Mathf.Abs(targetAngle); 
			if (!leftFootPlanted) 
			{ 
				targetAngle *= -1.0f; 
			} 
			return CurrentRun(forwardSpeed, leftFootPlanted);
		}

		/// <summary>
		/// Determines if the run or sprint AnimationInfo should be selected
		/// </summary>
		/// <param name="forwardSpeed">Character's normalized forward speed</param>
		/// <param name="turningClockwise">Is the character turning clockwise</param>
		/// <returns>The determined AnimationInfo</returns>
		private AnimationInfo CurrentRun(float forwardSpeed, bool turningClockwise)
		{
			if (turningClockwise)
			{
				return forwardSpeed <= 1.0f ? runRightTurn : sprintRightTurn;
			}
			return forwardSpeed <= 1.0f ? runLeftTurn : sprintLeftTurn;
		}
	}
}