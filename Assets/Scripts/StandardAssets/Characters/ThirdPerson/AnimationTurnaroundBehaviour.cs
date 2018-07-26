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
		protected float normalizedRunSpeedThreshold = 0.5f,
						crossfadeDuration = 0.125f,
						maxNormalizedTime = 0.125f,
						animationProgressLerpTime = 0.4f;

		private float rotation;
		private float normalizedTime;
		private Vector3 startingRotationEuler;
		
		private ThirdPersonAnimationController animationController;
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

			normalizedTime = Mathf.Lerp(normalizedTime, animationController.animationNormalizedProgress,
				animationProgressLerpTime);
			Vector3 newRotation = startingRotationEuler + new Vector3(0, normalizedTime * rotation, 0);
			newRotation.y = MathUtilities.Wrap180(newRotation.y);
			
			transform.rotation = Quaternion.Euler(newRotation);

			if(animationController.animationNormalizedProgress > 0.9f)
			{
				EndTurnAround();
			}
		}

		protected override void FinishedTurning()
		{
		}

		protected override void StartTurningAround(float angle)
		{
			var rapidTurnState = animationController.animatorForwardSpeed > normalizedRunSpeedThreshold ? 
				GetFootednessString(runLeftTurn, runRightTurn) : GetFootednessString(walkLeftTurn, walkRightTurn);
			
			rotation = GetAngleFromFootedness(Mathf.Abs(angle));
			startingRotationEuler = transform.eulerAngles;
			
			var time = Mathf.Clamp(animationController.footednessNormalizedProgress, 0, maxNormalizedTime);
			animationController.unityAnimator.CrossFade(rapidTurnState, crossfadeDuration, 0, time);

			normalizedTime = 0;
		}

		private string GetFootednessString(string left, string right)
		{
			return !animationController.isRightFootPlanted ? right : left;
		}

		private float GetAngleFromFootedness(float angle)
		{
			return !animationController.isRightFootPlanted ? angle : -angle;
		}
	}
}