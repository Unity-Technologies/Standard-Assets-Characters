using System;
using System.Linq;
using Boo.Lang;
using UnityEditor.Animations;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class AnimationTurnaroundBehaviour : TurnaroundBehaviour
	{
		[Serializable]
		protected class AnimationInfo
		{
			public string name;
			public float duration { get; set; }

			public AnimationInfo(string name)
			{
				this.name = name;
			}
		}

		[SerializeField] 
		protected AnimationInfo runLeftTurn = new AnimationInfo("RunForwardTurnLeft180"),
		                 runRightTurn = new AnimationInfo("RunForwardTurnRight180_Mirror"),
		                 walkLeftTurn = new AnimationInfo("WalkForwardTurnLeft180"),
		                 walkRightTurn = new AnimationInfo("WalkForwardTurnRight180_Mirror");


		[SerializeField] protected float normalizedRunSpeedThreshold = 0.5f,
			crossfadeDuration = 0.125f,
			maxNormalizedTime = 0.125f,
			normalizedCompletionTime = 0.9f;

		private float rotation;
		private float animationTime;
		private Vector3 startingRotationEuler;
		private AnimationInfo current;
		
		private ThirdPersonAnimationController animationController;
		private Transform transform;

		public override void Init(ThirdPersonBrain brain)
		{
			animationController = brain.animationControl;
			transform = brain.transform;
		}

		public override void Start()
		{
			base.Start();
			//TODO make this happen at edit time
			var runtimeAnimationController = animationController.unityAnimator.runtimeAnimatorController;
			runLeftTurn.duration = GetClipDuration(runtimeAnimationController, runLeftTurn.name);
			runRightTurn.duration = GetClipDuration(runtimeAnimationController, runRightTurn.name);
			walkLeftTurn.duration = GetClipDuration(runtimeAnimationController, walkLeftTurn.name);
			walkRightTurn.duration = GetClipDuration(runtimeAnimationController, walkRightTurn.name);
		}

		public override void Update()
		{
			if (!isTurningAround)
			{
				return;
			}

			animationTime += Time.deltaTime / normalizedCompletionTime;
			Vector3 newRotation = startingRotationEuler + new Vector3(0, animationTime * rotation, 0);
			newRotation.y = MathUtilities.Wrap180(newRotation.y);
			
			transform.rotation = Quaternion.Euler(newRotation);

			if(animationTime >= current.duration)
			{
				EndTurnAround();
			}
		}

		protected override void FinishedTurning()
		{
		}

		protected override void StartTurningAround(float angle)
		{
			current = GetCurrent(animationController.animatorForwardSpeed > normalizedRunSpeedThreshold,
				!animationController.isRightFootPlanted);
			
			rotation = GetAngleFromFootedness(Mathf.Abs(angle));
			startingRotationEuler = transform.eulerAngles;
			
			var time = Mathf.Clamp(animationController.footednessNormalizedProgress, 0, maxNormalizedTime);
			animationController.unityAnimator.CrossFade(current.name, crossfadeDuration, 0, time);
			animationTime = time;
		}

		private float GetAngleFromFootedness(float angle)
		{
			return !animationController.isRightFootPlanted ? angle : -angle;
		}

		private AnimationInfo GetCurrent(bool run, bool leftPlanted)
		{
			if (run)
			{
				return leftPlanted ? runRightTurn : runLeftTurn;
			}
			return leftPlanted ? walkRightTurn : walkLeftTurn;
		}

		private static float GetClipDuration(RuntimeAnimatorController runtimeAnimationController, string clipName)
		{
			var clip = runtimeAnimationController.animationClips.FirstOrDefault(c => 
			c.name == clipName);
			if (clip != null)
			{
				return clip.length;
			}
			Debug.LogErrorFormat("Clip not found: {0}", clipName);
			return 0;
		}
	}
}