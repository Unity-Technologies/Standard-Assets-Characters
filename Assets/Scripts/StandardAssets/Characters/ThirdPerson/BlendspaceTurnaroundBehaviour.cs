using System;
using Attributes;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class BlendspaceTurnaroundBehaviour : TurnaroundBehaviour
	{
		private const float k_DefaultTurnTime = 0.2f, k_DefaultTurnSpeed = 0f;
		
		[SerializeField]
		protected bool configureBlendspace;
		
		//[ConditionalInclude("configureBlendspace")]
		[SerializeField]
		protected BlendspaceTurnaroundConfiguration configuration;

		Vector3 targetRotationEuler;
		Quaternion targetRotation;
		private float turningTime = 0f;
		private float currentForwardSpeed;
		private float currentTurningSpeed;
		private float rotation;
		private ThirdPersonAnimationController animationController;
		private Transform transform;
		
		private AnimationCurve defaultForwardCurve = AnimationCurve.Linear(0, 1, 1, 1);

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

		public override void Init(ThirdPersonBrain brain)
		{
			animationController = brain.animationControl;
			transform = brain.transform;
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
		}
	}

	public enum Calculation
	{
		Additive,
		Multiplicative
	}
}