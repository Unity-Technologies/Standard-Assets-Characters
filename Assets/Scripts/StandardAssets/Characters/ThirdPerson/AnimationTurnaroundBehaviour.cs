using System;
using UnityEngine;
using Util;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class AnimationTurnaroundBehaviour : TurnaroundBehaviour
	{
		[Serializable]
		protected class AnimationInfo
		{
			public string name;
			[HideInInspector]
			public float duration;

			public AnimationInfo(string name)
			{
				this.name = name;
			}
		}

		[SerializeField] 
		protected AnimationInfo runLeftTurn = new AnimationInfo("RunForwardTurnLeft180"),
		                 runRightTurn = new AnimationInfo("RunForwardTurnRight180_Mirror"),
		                 walkLeftTurn = new AnimationInfo("WalkForwardTurnLeft180"),
		                 walkRightTurn = new AnimationInfo("WalkForwardTurnRight180_Mirror"),
						idleLeftTurn = new AnimationInfo("IdleTurnLeft180"),
						idleRightTurn = new AnimationInfo("IdleTurnRight180_Mirror");


		[SerializeField] protected float normalizedRunSpeedThreshold = 0.5f,
			crossfadeDuration = 0.125f,
			maxNormalizedTime = 0.125f,
			normalizedCompletionTime = 0.9f;

		private float animationTime,
			targetAngle;
		private AnimationInfo current;
		private Vector3 startRotation;
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

			Vector3 newRotation = startRotation + new Vector3(0, (animationTime / current.duration) * targetAngle, 0);
			transform.rotation = Quaternion.Euler(newRotation);

			animationTime += Time.deltaTime / normalizedCompletionTime;
			if(animationTime >= current.duration)
			{
				EndTurnAround();
			}
		}

		public override Vector3 GetMovement()
		{
			return animationController.unityAnimator.deltaPosition;
		}

		protected override void FinishedTurning()
		{
		}

		protected override void StartTurningAround(float angle)
		{
			targetAngle = MathUtilities.Wrap180(angle);
			current = GetCurrent(animationController.animatorForwardSpeed,
				!animationController.isRightFootPlanted);

			startRotation = transform.eulerAngles;
			var time = Mathf.Clamp(animationController.footednessNormalizedProgress, 0, maxNormalizedTime);
			animationController.unityAnimator.CrossFade(current.name, crossfadeDuration, 0, 0);
			animationTime = time;
		}

		private AnimationInfo GetCurrent(float forwardSpeed, bool leftPlanted)
		{
			targetAngle = Mathf.Abs(targetAngle);
			if (!leftPlanted)
			{
				targetAngle *= -1;
			}
			if (forwardSpeed < 0.1f)
			{
				return leftPlanted ? idleRightTurn : idleLeftTurn;
			}
			if (forwardSpeed < normalizedRunSpeedThreshold)
			{
				return leftPlanted ? walkRightTurn : walkLeftTurn;
			}
			return leftPlanted ? runRightTurn : runLeftTurn;
		}
		
#if UNITY_EDITOR
		private int turnsFound;
		public void OnValidate(Animator animator)
		{
			turnsFound = 0;
			// we get states from state machine, no need to look in blend trees for this.
			var animation = animator.runtimeAnimatorController as AnimatorController;
			TraverseStatemachineToCheckStates(animation.layers[0].stateMachine);
			
			if (turnsFound < 6)
			{
				Debug.LogError("Did not find all turn states in state machine");
			}
		}

		private void TraverseStatemachineToCheckStates(AnimatorStateMachine stateMachine)
		{
			if (turnsFound == 6)
			{
				return;
			}
			foreach (var childState in stateMachine.states)
			{
				var clip = childState.state.motion as AnimationClip;
				if (clip != null)
				{
					CheckStateForTurn(childState.state);
					if (turnsFound == 6)
					{
						return;
					}
				}
			}
			foreach (var childStateMachine in stateMachine.stateMachines)
			{
				TraverseStatemachineToCheckStates(childStateMachine.stateMachine);
			}
		}

		private void CheckStateForTurn(AnimatorState state)
		{
			if (state.name == runLeftTurn.name)
			{
				runLeftTurn.duration = state.motion.averageDuration;
				turnsFound++;
			}
			else if (state.name == runRightTurn.name)
			{
				runRightTurn.duration = state.motion.averageDuration;
				turnsFound++;
			}
			if (state.name == walkLeftTurn.name)
			{
				walkLeftTurn.duration = state.motion.averageDuration;
				turnsFound++;
			}
			if (state.name == walkRightTurn.name)
			{
				walkRightTurn.duration = state.motion.averageDuration;
				turnsFound++;
			}
			if (state.name == idleLeftTurn.name)
			{
				idleLeftTurn.duration = state.motion.averageDuration;
				turnsFound++;
			}
			if (state.name == idleRightTurn.name)
			{
				idleRightTurn.duration = state.motion.averageDuration;
				turnsFound++;
			}
		}
#endif
	}
}