using System;
using Attributes;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class BlendspaceTurnaroundBehaviour : TurnaroundBehaviour
	{
		private const float k_DefaultTurnTime = 0.15f, k_DefaultTurnSpeed = 0f;
		
		[SerializeField]
		protected bool configureBlendspace;
		
		//[ConditionalInclude("configureBlendspace")]
		[SerializeField]
		protected BlendspaceTurnaroundConfiguration configuration;

		private Vector3 targetRotationEuler, movementVector;
		private Quaternion targetRotation;
		private float turningTime = 0f;
		private float currentForwardSpeed;
		private float currentTurningSpeed;
		private float rotation;
		private ThirdPersonBrain thirdPersonBrain;
		private ThirdPersonAnimationController animationController;
		private Transform transform;
		
		private AnimationCurve defaultForwardCurve = AnimationCurve.Linear(0, 1, 1, 1);
		
		private AnimationCurve defaultTurnMovementCurve = AnimationCurve.Linear(0,-1,1,-1);

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

				return Calculation.Multiplicative;
			}
		}

		private AnimationCurve turnMovementOverTime
		{
			get
			{
				if (configureBlendspace)
				{
					return configuration.turnMovementOverTime;
				}

				return defaultTurnMovementCurve;
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

			animationController.UpdateForwardSpeed(Mathf.Clamp(forwardSpeedValue, -1, 1), Time.deltaTime);
			
			float oldYRotation = transform.eulerAngles.y;
			transform.rotation =
				Quaternion.RotateTowards(transform.rotation, targetRotation, rotation / timeToTurn * Time.deltaTime);
			float newYRotation = transform.eulerAngles.y;

			float actualTurnSpeed =
				turnSpeed * Mathf.Sign(MathUtilities.Wrap180(newYRotation) - MathUtilities.Wrap180(oldYRotation));

			animationController.UpdateLateralSpeed(actualTurnSpeed, Time.deltaTime);
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
			rotation = Mathf.Abs(angle);
			turningTime = 0f;
			currentForwardSpeed = animationController.animatorForwardSpeed;
			currentTurningSpeed = animationController.animatorTurningSpeed;
			
			targetRotationEuler = transform.eulerAngles;
			targetRotationEuler.y += rotation;
			targetRotation = Quaternion.Euler(targetRotationEuler);

			RootMotionThirdPersonMotor motor = thirdPersonBrain.CurrentMotor as RootMotionThirdPersonMotor;
			if (motor != null)
			{
				movementVector = transform.forward * motor.cachedForwardMovement;
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