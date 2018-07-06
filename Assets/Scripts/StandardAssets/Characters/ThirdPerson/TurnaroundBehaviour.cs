using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class TurnaroundBehaviour : MonoBehaviour
	{
		[SerializeField]
		protected float rotation = 180f;
		
		[SerializeField]
		protected float timeToTurn = 0.3f;

		[SerializeField]
		protected float forwardDampTimeScale = 0.1f;
		
		[SerializeField]
		protected AnimationCurve forwardSpeed = AnimationCurve.Linear(0,0,1,1);
		
		[SerializeField]
		protected AnimationCurve turningSpeed = AnimationCurve.Linear(0,0,1,1);

		[SerializeField]
		protected Animator animator;

		Vector3 targetRotationEuler;
		Quaternion targetRotation;
		private bool isTurningAround = false;
		private float turningTime = 0f;
		float currentForwardSpeed;

		public void TurnAround()
		{
			if (isTurningAround)
			{
				return;
			}

			isTurningAround = true;
			turningTime = 0f;
			currentForwardSpeed = animator.GetFloat("ForwardSpeed");
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
				}
			}
		}

		void EvaluateTurn()
		{
			float normalizedTime = turningTime / timeToTurn;

			float turningSpeedValue = turningSpeed.Evaluate(normalizedTime);
			float forwardSpeedValue = forwardSpeed.Evaluate(normalizedTime);
			
			animator.SetFloat("TurningSpeed", turningSpeedValue);
			animator.SetFloat("ForwardSpeed", forwardSpeedValue + currentForwardSpeed);
			
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotation/timeToTurn * Time.deltaTime);
		}
	}
}