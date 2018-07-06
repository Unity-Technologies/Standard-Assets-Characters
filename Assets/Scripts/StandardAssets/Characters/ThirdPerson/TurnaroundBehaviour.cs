using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	public class TurnaroundBehaviour : MonoBehaviour
	{
		[SerializeField]
		protected float rotation = 180f;

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
		private bool isTurningAround = false;
		private float turningTime = 0f;
		float currentForwardSpeed;
		float currentTurningSpeed;

		public void TurnAround()
		{
			if (isTurningAround)
			{
				return;
			}

			isTurningAround = true;
			turningTime = 0f;
			currentForwardSpeed = animator.GetFloat("ForwardSpeed");
			currentTurningSpeed = animator.GetFloat("TurningSpeed");
			targetRotationEuler = transform.eulerAngles;
			targetRotationEuler.y += rotation;
			targetRotation = Quaternion.Euler(targetRotationEuler);
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				TurnAround();
			}

			if (isTurningAround)
			{
				EvaluateTurn();
				turningTime += Time.deltaTime;
				if (turningTime >= timeToTurn)
				{
					turningTime = timeToTurn;
					isTurningAround = false;
					EvaluateTurn();
					animator.SetFloat("TurningSpeed", currentTurningSpeed);
					animator.SetFloat("ForwardSpeed", currentForwardSpeed);
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
	}
}