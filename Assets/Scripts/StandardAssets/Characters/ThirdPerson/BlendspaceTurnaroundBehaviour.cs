using System.Timers;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	public class BlendspaceTurnaroundBehaviour : TurnaroundBehaviour
	{
		[SerializeField]
		protected float timeToTurn = 0.3f;

		[SerializeField]
		protected float turnSpeed = 1f;

		[SerializeField]
		protected AnimationCurve forwardSpeed = AnimationCurve.Linear(0, 0, 1, 1);


		Vector3 targetRotationEuler;
		Quaternion targetRotation;
		private float turningTime = 0f;
		private float currentForwardSpeed;
		private float currentTurningSpeed;
		private float rotation;
		private ThirdPersonAnimationController animationController;

		private void Awake()
		{
			animationController = GetComponent<ThirdPersonAnimationController>();
		}

		private void Update()
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

			animationController.UpdateForwardSpeed(Mathf.Clamp(forwardSpeedValue + currentForwardSpeed, -1, 1), Time.deltaTime);
			
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
}