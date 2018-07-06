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

		[SerializeField]
		protected Animator animator;

		Vector3 targetRotationEuler;
		Quaternion targetRotation;
		private float turningTime = 0f;
		private float currentForwardSpeed;
		private float currentTurningSpeed;
		private float rotation;

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				TurnAround(180f);
			}

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

		void EvaluateTurn()
		{
			float normalizedTime = turningTime / timeToTurn;

			float forwardSpeedValue = forwardSpeed.Evaluate(normalizedTime);

			animator.SetFloat("ForwardSpeed", Mathf.Clamp(forwardSpeedValue + currentForwardSpeed, -1, 1));

			float oldYRotation = transform.eulerAngles.y;
			transform.rotation =
				Quaternion.RotateTowards(transform.rotation, targetRotation, rotation / timeToTurn * Time.deltaTime);
			float newYRotation = transform.eulerAngles.y;

			float actualTurnSpeed =
				turnSpeed * Mathf.Sign(MathUtilities.Wrap180(newYRotation) - MathUtilities.Wrap180(oldYRotation));

			animator.SetFloat("TurningSpeed", actualTurnSpeed);
		}

		protected override void FinishedTurning()
		{
			turningTime = timeToTurn;
			EvaluateTurn();
			animator.SetFloat("TurningSpeed", currentTurningSpeed);
			animator.SetFloat("ForwardSpeed", currentForwardSpeed);
		}

		protected override void StartTurningAround(float angle)
		{
			rotation = angle;
			turningTime = 0f;
			currentForwardSpeed = animator.GetFloat("ForwardSpeed");
			currentTurningSpeed = animator.GetFloat("TurningSpeed");
			targetRotationEuler = transform.eulerAngles;
			targetRotationEuler.y += rotation;
			targetRotation = Quaternion.Euler(targetRotationEuler);
		}
	}
}