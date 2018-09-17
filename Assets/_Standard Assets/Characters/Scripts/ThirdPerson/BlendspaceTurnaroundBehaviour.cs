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
		protected BlendspaceTurnaroundConfiguration configuration;

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

		private Calculation forwardSpeedCalculation
		{
			get { return configureBlendspace ? configuration.forwardSpeedCalc : Calculation.Additive; }
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

			if (forwardSpeedCalculation == Calculation.Multiplicative)
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
	}
}