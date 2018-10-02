using System;
using StandardAssets.Characters.Attributes;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <inheritdoc />
	/// <summary>
	/// Blendspace extension of TurnaroundBehaviour. Rotates the character to the target angle using blendspace.
	/// </summary>
	[Serializable]
	public class BlendspaceTurnaroundBehaviour : TurnaroundBehaviour
	{
		[SerializeField, Tooltip("Should the turnaround be configured or just use defaults?")]
		protected bool configureBlendspace;

		[VisibleIf("configureBlendspace")]
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
		private ThirdPersonAnimationController animationController;
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
			animationController = brain.animationControl;
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
			animationController.UpdateForwardSpeed(currentForwardSpeed, Time.deltaTime);
			animationController.UpdateTurningSpeed(currentTurningSpeed, Time.deltaTime);
		}

		/// <summary>
		/// Starts the blendspace turnaround.
		/// </summary>
		/// <param name="angle">The target angle.</param>
		protected override void StartTurningAround(float angle)
		{
			isSmallTurn = Mathf.Abs(angle) < classificationAngle;
			targetAngle = MathUtilities.Wrap180(angle);
			turningTime = 0.0f;
			currentForwardSpeed = animationController.animatorForwardSpeed;
			currentTurningSpeed = animationController.animatorTurningSpeed;

			startRotation = transform.eulerAngles;

			RootMotionThirdPersonMotor motor = thirdPersonBrain.currentMotor as RootMotionThirdPersonMotor;
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

			animationController.UpdateForwardSpeed(forwardSpeedValue, Time.deltaTime);

			Vector3 newRotation =
				startRotation + new Vector3(0.0f, rotationOverTime.Evaluate(normalizedTime) * targetAngle, 0.0f);
			transform.rotation = Quaternion.Euler(newRotation);
		}
		
		/// <summary>
		/// Data class used to store configuration settings used by <see cref="BlendspaceTurnaroundBehaviour"/>.
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