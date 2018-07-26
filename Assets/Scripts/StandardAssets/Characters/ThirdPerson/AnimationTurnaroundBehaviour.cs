using System;
using UnityEngine;
using Util;

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
		private float rotation;
		private Transform transform;

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

			Vector3 newRotation = startingRotationEuler + new Vector3(0, animationController.animationNormalizedProgress * rotation, 0);
			
			transform.rotation = Quaternion.Euler(newRotation);
			
			if(animationController.animationNormalizedProgress > 0.9f)
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
				rapidTurnState = GetFootednessString(runLeftTurn, runRightTurn);
			}
			else
			{
				rapidTurnState = GetFootednessString(walkLeftTurn, walkRightTurn);
			}
			
			rotation = GetAngleFromFootedness(Mathf.Abs(angle));
			startingRotationEuler = transform.eulerAngles;
			
			animationController.unityAnimator.CrossFade(rapidTurnState, 0.1f, 0, MathUtilities.WrapX(animationController.footednessNormalizedProgress, 0.5f));
		}

		protected string GetFootednessString(string left, string right)
		{
			if (animationController.isRightFootPlanted)
			{
				return right;
			}

			return left;
		}

		protected float GetAngleFromFootedness(float angle)
		{
			if (animationController.isRightFootPlanted)
			{
				return angle;
			}

			return -angle;
		}
	}
}