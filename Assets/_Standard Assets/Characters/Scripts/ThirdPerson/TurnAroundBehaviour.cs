using System;
using StandardAssets.Characters.Helpers;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Class to handle rapid turns
	/// </summary>
	public abstract class TurnAroundBehaviour
	{
		/// <summary>
		/// Value multiplicatively applied to the head look at turn angle
		/// </summary>
		public abstract float headTurnScale { get; }
		
		/// <summary>
		/// Event fired on completion of turnaround
		/// </summary>
		public event Action turnaroundComplete;
		
		/// <summary>
		/// Whether this object is currently handling a turnaround motion
		/// </summary>
		protected bool isTurningAround;

		public abstract void Init(ThirdPersonBrain brain);

		public abstract void Update();

		/// <summary>
		/// Gets the movement of the character
		/// </summary>
		/// <returns>Movement to apply to the character</returns>
		public abstract Vector3 GetMovement();

		/// <summary>
		/// Starts a turnaround
		/// </summary>
		/// <param name="angle">Target y rotation in degrees</param>
		public void TurnAround(float angle)
		{
			if (isTurningAround)
			{
				return;
			}

			isTurningAround = true;
			StartTurningAround(angle);
		}

		/// <summary>
		/// Called on completion of turnaround. Fires <see cref="turnaroundComplete"/>  event.
		/// </summary>
		protected void EndTurnAround()
		{
			isTurningAround = false;
			FinishedTurning();
			if (turnaroundComplete != null)
			{
				turnaroundComplete();
			}
		}

		protected abstract void FinishedTurning();

		protected abstract void StartTurningAround(float angle);
	}
	
	/// <summary>
	/// Enum used to describe a turnaround type.
	/// </summary>
	public enum TurnaroundType
	{
		None,
		Blendspace,
		Animation
	}
	
	/// <summary>
	/// Animation extension of TurnaroundBehaviour. Rotates the character to the target angle while playing an animation.
	/// </summary>
	/// <remarks>This turnaround type should be used to improve fidelity at the cost of responsiveness.</remarks>
	[Serializable]
	public class AnimationTurnAroundBehaviour : TurnAroundBehaviour
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
				case State.WaitingForTransition: // wait a a frame for transition
					state = State.Transitioning;
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
			targetAngle = angle.Wrap180();
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
	
	/// <summary>
	/// Blendspace extension of TurnaroundBehaviour. Rotates the character to the target angle using blendspace.
	/// </summary>
	[Serializable]
	public class BlendspaceTurnAroundBehaviour : TurnAroundBehaviour
	{
		[SerializeField, Tooltip("Should the turnaround be configured or just use defaults?")]
		protected bool configureBlendspace;

		[SerializeField, Tooltip("The configuration settings of the turnaround.")]
		protected BlendspaceProperties configuration;

		private bool isSmallTurn;
		private float turningTime;
		private float currentForwardSpeed;
		private float currentTurningSpeed;
		private float targetAngle;

		private Vector3 startRotation;
		private Vector3 movementVector;

		private ThirdPersonBrain thirdPersonBrain;
		private Transform transform;

		// defaults used if configureBlendspace is false
		private const float k_DefaultTurnTime = 0.2f, k_DefaultHeadTurnScale = 1.0f;
		private float defaultTurnClassificationAngle = 150.0f;
		private AnimationCurve defaultRotationCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
		private AnimationCurve defaultForwardCurve = AnimationCurve.Linear(0.0f, 0.1f, 1.0f, 0.1f);
		private AnimationCurve defaultTurn180MovementCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);
		private AnimationCurve defaultTurn90MovementCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

		private float timeToTurn
		{
			get { return configureBlendspace ? configuration.turnTime : k_DefaultTurnTime; }
		}

		private AnimationCurve forwardSpeed
		{
			get { return configureBlendspace ? configuration.forwardSpeedOverTime : defaultForwardCurve; }
		}

		private BlendspaceCalculation forwardSpeedCalculation
		{
			get { return configureBlendspace ? configuration.forwardSpeedCalc : BlendspaceCalculation.Additive; }
		}

		private float classificationAngle
		{
			get { return configureBlendspace ? configuration.classificationAngle : defaultTurnClassificationAngle; }
		}

		private AnimationCurve turnMovementOverTime
		{
			get
			{
				if (isSmallTurn)
				{
					return configureBlendspace ? configuration.turn90MovementOverTime : defaultTurn90MovementCurve;
				}

				return configureBlendspace ? configuration.turn180MovementOverTime : defaultTurn180MovementCurve;
			}
		}

		private AnimationCurve rotationOverTime
		{
			get { return configureBlendspace ? configuration.rotationOverTime : defaultRotationCurve; }
		}

		/// <inheritdoc/>
		public override float headTurnScale
		{
			get { return configureBlendspace ? configuration.headTurnScale : k_DefaultHeadTurnScale; }
		}

		public override void Init(ThirdPersonBrain brain)
		{
			transform = brain.transform;
			thirdPersonBrain = brain;
		}

		/// <summary>
		/// Evaluates the turn and rotates the character.
		/// </summary>
		public override void Update()
		{
			if (isTurningAround)
			{
				EvaluateTurn();
				turningTime += Time.deltaTime;
				if (turningTime >= timeToTurn)
				{
					EndTurnAround();
				}
			}
		}

		/// <inheritdoc/>
		public override Vector3 GetMovement()
		{
			float normalizedTime = turningTime / timeToTurn;
			return movementVector * turnMovementOverTime.Evaluate(normalizedTime) * Time.deltaTime;
		}

		/// <summary>
		/// Updates the <see cref="animationController"/> with a new forward and turning speed.
		/// </summary>
		protected override void FinishedTurning()
		{
			turningTime = timeToTurn;
			EvaluateTurn();
			thirdPersonBrain.UpdateForwardSpeed(currentForwardSpeed, Time.deltaTime);
			thirdPersonBrain.UpdateTurningSpeed(currentTurningSpeed, Time.deltaTime);
		}

		/// <summary>
		/// Starts the blendspace turnaround.
		/// </summary>
		/// <param name="angle">The target angle.</param>
		protected override void StartTurningAround(float angle)
		{
			isSmallTurn = Mathf.Abs(angle) < classificationAngle;
			targetAngle = angle.Wrap180();
			turningTime = 0.0f;
			currentForwardSpeed = thirdPersonBrain.animatorForwardSpeed;
			currentTurningSpeed = thirdPersonBrain.animatorTurningSpeed;

			startRotation = transform.eulerAngles;

			ThirdPersonMotor motor = thirdPersonBrain.thirdPersonMotor;
			if (motor != null)
			{
				Vector3 rotatedVector = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
				movementVector = rotatedVector * motor.cachedForwardVelocity;
				movementVector.y = 0.0f;
			}
		}

		private void EvaluateTurn()
		{
			float normalizedTime = turningTime / timeToTurn;

			float forwardSpeedValue = forwardSpeed.Evaluate(normalizedTime);

			if (forwardSpeedCalculation == BlendspaceCalculation.Multiplicative)
			{
				forwardSpeedValue = forwardSpeedValue * currentForwardSpeed;
			}
			else
			{
				forwardSpeedValue = forwardSpeedValue + currentForwardSpeed;
			}

			thirdPersonBrain.UpdateForwardSpeed(forwardSpeedValue, Time.deltaTime);

			Vector3 newRotation =
				startRotation + new Vector3(0.0f, rotationOverTime.Evaluate(normalizedTime) * targetAngle, 0.0f);
			transform.rotation = Quaternion.Euler(newRotation);
		}
		
		/// <summary>
		/// Data class used to store configuration settings used by <see cref="BlendspaceTurnAroundBehaviour"/>.
		/// </summary>
		[Serializable]
		protected class BlendspaceProperties
		{
			[SerializeField, Tooltip("Duration of the turnaround.")]
			protected float timeToTurn = 0.2f;

			[SerializeField, Tooltip("Curve used to evaluate rotation throughout turnaround.")]
			protected AnimationCurve rotationDuringTurn = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

			[SerializeField, Tooltip("Curve used to evaluate forward speed throughout turnaround.")]
			protected AnimationCurve forwardSpeed = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

			[SerializeField, Tooltip("Method to apply forward speed during turnaround.")]
			protected BlendspaceCalculation forwardSpeedCalculation = BlendspaceCalculation.Multiplicative;

			[SerializeField, Tooltip("An angle less than this is classified as a small turn.")]
			protected float turnClassificationAngle = 150.0f;

			[SerializeField, Tooltip("Curve used to evaluate movement throughout a 180° turnaround.")]
			protected AnimationCurve movementDuring180Turn = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

			[SerializeField, Tooltip("Curve used to evaluate movement throughout a 90° turnaround.")]
			protected AnimationCurve movementDuring90Turn = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

			[SerializeField, Tooltip("Head look at angle scale during animation.")]
			protected float headTurnMultiplier = 1.0f;

			/// <summary>
			/// Gets the turn duration in seconds.
			/// </summary>
			public float turnTime
			{
				get { return timeToTurn; }
			}

			/// <summary>
			/// Gets the curve to evaluate forward speed over time.
			/// </summary>
			public AnimationCurve forwardSpeedOverTime
			{
				get { return forwardSpeed; }
			}

			/// <summary>
			/// Gets the method of applying forward speed.
			/// </summary>
			public BlendspaceCalculation forwardSpeedCalc
			{
				get { return forwardSpeedCalculation; }
			}

			/// <summary>
			/// Gets the angle used for small turn classification.
			/// </summary>
			public float classificationAngle
			{
				get { return turnClassificationAngle; }
			}

			/// <summary>
			/// Gets the curve used to evaluate movement throughout a 180° turnaround.
			/// </summary>
			public AnimationCurve turn180MovementOverTime
			{
				get { return movementDuring180Turn; }
			}

			/// <summary>
			/// Gets the curve used to evaluate movement throughout a 90° turnaround.
			/// </summary>
			public AnimationCurve turn90MovementOverTime
			{
				get { return movementDuring90Turn; }
			}

			/// <summary>
			/// Gets the curve used to evaluate rotation over time.
			/// </summary>
			public AnimationCurve rotationOverTime
			{
				get { return rotationDuringTurn; }
			}

			/// <summary>
			/// Gets the head turn scale to be applied during a turnaround.
			/// </summary>
			public float headTurnScale
			{
				get { return headTurnMultiplier; }
			}
		}
		
		/// <summary>
		/// Enum describing a mathematics operation.
		/// </summary>
		public enum BlendspaceCalculation
		{
			Additive,
			Multiplicative
		}
	}
}