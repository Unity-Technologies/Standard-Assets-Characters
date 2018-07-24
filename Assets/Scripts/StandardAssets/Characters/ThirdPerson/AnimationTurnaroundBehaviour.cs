using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class AnimationTurnaroundBehaviour : TurnaroundBehaviour
	{
		[SerializeField]
		protected string runLeftTurn = "RunForwardTurnLeft",
		                 runRightTurn = "RunForwardTurnRight",
		                 walkLeftTurn = "WalkForwardTurnLeft",
		                 walkRightTurn = "WalkForwardTurnRight";

		[SerializeField]
		protected float normalizedRunSpeedThreshold = 0.5f;

		private ThirdPersonAnimationController animationController;
		
		Vector3 startingRotationEuler;
//		Quaternion targetRotation;
		private float rotation;
		private Transform transform;

		//TODO actually implement
		public override void Init(ThirdPersonBrain brain)
		{
			animationController = brain.animationControl;
			transform = brain.transform;
		}

		public override void Update()
		{
			if (!isTurningAround)
			{
				return;
			}

			Vector3 newRotation = startingRotationEuler - new Vector3(0, animationController.animationNormalizedProgress * rotation, 0);
			
			transform.rotation = Quaternion.Euler(newRotation);
			
			if(animationController.animationNormalizedProgress > 0.9f)
			//if (Mathf.Approximately(animationController.animationNormalizedProgress, 1))
			{
				EndTurnAround();
			}
		}

		protected override void FinishedTurning()
		{
			Debug.Log("FINISHED TURNING");
		}

		protected override void StartTurningAround(float angle)
		{
			string rapidTurnState = runLeftTurn;
			if (animationController.animatorForwardSpeed > normalizedRunSpeedThreshold)
			{
				
			}
			else
			{
				
			}
			
			rotation = Mathf.Abs(angle);
			Debug.Log(rotation);
			startingRotationEuler = transform.eulerAngles;
//			targetRotationEuler.y += rotation;
//			targetRotation = Quaternion.Euler(targetRotationEuler);
			
			animationController.unityAnimator.CrossFade(rapidTurnState, 0.1f, 0, animationController.animationNormalizedProgress);
		}
	}
}