using System;
using Attributes;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class BlendspaceTurnaroundBehaviour : TurnaroundBehaviour
	{
		private const float k_DefaultTurnTime = 0.2f, k_DefaultTurnSpeed = 0f, k_DefaultHeadTurnScale = 1f;

		[SerializeField]
		protected bool configureBlendspace;

		[ConditionalInclude("configureBlendspace")]
		[SerializeField]
		protected BlendspaceTurnaroundConfiguration configuration;

		private Vector3 startRotation;

		private Vector3 movementVector;
		private float turningTime = 0f;
		private float currentForwardSpeed;
		private float currentTurningSpeed;
		private float targetAngle;
		private ThirdPersonBrain thirdPersonBrain;
		private ThirdPersonAnimationController animationController;
		private Transform transform;

		private AnimationCurve defaultRotationCurve = AnimationCurve.Linear(0, 0, 1, 1);

		private AnimationCurve defaultForwardCurve = AnimationCurve.Linear(0, 0.1f, 1, 0.1f);

		private AnimationCurve defaultTurn180MovementCurve = AnimationCurve.Linear(0, 1, 1, 1);

		private AnimationCurve defaultTurn90MovementCurve = AnimationCurve.Linear(0, 1, 1, 1);

		private float defaultTurnClassificationAngle = 150f;

		private bool isSmallTurn;

		private float timeToTurn
		{
			get
			{
				if (configureBlendspace)
				{
					return configuration.turnTime;
				}

				return k_DefaultTurnTime;
			}
		}

		private float turnSpeed
		{
			get
			{
				if (configureBlendspace)
				{
					return configuration.normalizedTurnSpeed;
				}

				return k_DefaultTurnSpeed;
			}
		}

		private AnimationCurve forwardSpeed
		{
			get
			{
				if (configureBlendspace)
				{
					return configuration.forwardSpeedOverTime;
				}

				return defaultForwardCurve;
			}
		}

		private Calculation forwardSpeedCalculation
		{
			get
			{
				if (configureBlendspace)
				{
					return configuration.forwardSpeedCalc;
				}

				return Calculation.Additive;
			}
		}

		private float classificationAngle
		{
			get
			{
				if (configureBlendspace)
				{
					return configuration.classificationAngle;
				}

				return defaultTurnClassificationAngle;
			}
		}

		private AnimationCurve turnMovementOverTime
		{
			get
			{
				if (isSmallTurn)
				{
					if (configureBlendspace)
					{
						return configuration.turn90MovementOverTime;
					}

					return defaultTurn90MovementCurve;
				}
				else
				{
					if (configureBlendspace)
					{
						return configuration.turn180MovementOverTime;
					}

					return defaultTurn180MovementCurve;
				}	
			}
		}

		private AnimationCurve rotationOverTime
		{
			get
			{
				if (configureBlendspace)
				{
					return configuration.rotationOverTime;
				}

				return defaultRotationCurve;
			}
		}

		public override float headTurnScale
		{
			get
			{
				if (configureBlendspace)
				{
					//return headTurnMultiplier;
				}

				return k_DefaultHeadTurnScale;
			}
		}

		public override void Init(ThirdPersonBrain brain)
		{
			animationController = brain.animationControl;
			transform = brain.transform;
			thirdPersonBrain = brain;
		}

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

		public override Vector3 GetMovement()
		{
			float normalizedTime = turningTime / timeToTurn;
			return movementVector * turnMovementOverTime.Evaluate(normalizedTime);
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
				startRotation + new Vector3(0, rotationOverTime.Evaluate(normalizedTime) * targetAngle, 0);
			transform.rotation = Quaternion.Euler(newRotation);
		}

		protected override void FinishedTurning()
		{
			turningTime = timeToTurn;
			EvaluateTurn();
			animationController.UpdateForwardSpeed(currentForwardSpeed, Time.deltaTime);
			animationController.UpdateTurningSpeed(currentTurningSpeed, Time.deltaTime);
		}

		protected override void StartTurningAround(float angle)
		{
			isSmallTurn = Mathf.Abs(angle) < classificationAngle;
			targetAngle = MathUtilities.Wrap180(angle);
			turningTime = 0f;
			currentForwardSpeed = animationController.animatorForwardSpeed;
			currentTurningSpeed = animationController.animatorTurningSpeed;

			startRotation = transform.eulerAngles;

			RootMotionThirdPersonMotor motor = thirdPersonBrain.CurrentMotor as RootMotionThirdPersonMotor;
			if (motor != null)
			{
				Vector3 rotatedVector = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
				movementVector = rotatedVector * motor.cachedForwardMovement;
				movementVector.y = 0;
			}
		}
	}

	public enum Calculation
	{
		Additive,
		Multiplicative
	}
}