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
			public float speed = 1;
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
								sprintLeftTurn = new AnimationInfo("RunForwardTurnLeft180"),
								sprintRightTurn = new AnimationInfo("RunForwardTurnRight180_Mirror"),
								idleLeftTurn = new AnimationInfo("IdleTurnLeft180"),
								idleRightTurn = new AnimationInfo("IdleTurnRight180_Mirror");

		private const int k_AnimationCount = 6;

		[SerializeField] 
		protected AnimationCurve rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);

		[SerializeField] 
		protected float normalizedRunSpeedThreshold = 0.1f,
						crossfadeDuration = 0.125f;

		private bool isTransitioning;

		private float animationTime,
			targetAngle,
			cachedAnimatorSpeed;
		private Quaternion startRotation;
		
		private AnimationInfo current;
		private ThirdPersonAnimationController animationController;
		private Transform transform;

		private Animator animator
		{
			get { return animationController.unityAnimator; }
		}

		private AnimationInfo CurrentRun(bool rightTurn, float forwardSpeed)
		{
			if (rightTurn)
			{
				return forwardSpeed <= 1 ? runRightTurn : sprintRightTurn;
			}
			return forwardSpeed <= 1 ? runLeftTurn : sprintLeftTurn;
		}

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

			if (current == idleLeftTurn || current == idleRightTurn)
			{
				animationController.UpdateForwardSpeed(0, Time.deltaTime);
			}

			if (isTransitioning)
			{
				var transitionTime = animator.GetAnimatorTransitionInfo(0).duration;
				if (transitionTime <= 0)
				{
					EndTurnAround();
				}
				return;
			}
			animationTime += Time.deltaTime * current.speed;
			var rotationProgress = rotationCurve.Evaluate(animationTime / current.duration);
			transform.rotation = Quaternion.AngleAxis(rotationProgress * targetAngle, Vector3.up) * startRotation;
			// animation complete, blending to locomotion
			if(animationTime >= current.duration)
			{
				animator.speed = cachedAnimatorSpeed;
				isTransitioning = true;
			}
		}

		public override Vector3 GetMovement()
		{
			if (current == idleLeftTurn || current == idleRightTurn)
			{
				return Vector3.zero;
			}
			return animator.deltaPosition;
		}

		protected override void FinishedTurning()
		{

		}

		protected override void StartTurningAround(float angle)
		{
			targetAngle = MathUtilities.Wrap180(angle);
			current = GetCurrent(animationController.animatorForwardSpeed, angle > 0,
				!animationController.isRightFootPlanted);

			startRotation = transform.rotation;
			animator.CrossFade(current.name, crossfadeDuration, 0, 0);
			animationTime = 0;

			cachedAnimatorSpeed = animator.speed;
			animator.speed = current.speed;

			isTransitioning = false;
		}

		private AnimationInfo GetCurrent(float forwardSpeed, bool turningRight, bool leftPlanted)
		{
			// idle turn
			if (forwardSpeed < normalizedRunSpeedThreshold)
			{
				return turningRight ? idleRightTurn : idleLeftTurn;
			}

			// < 180 turn
			if (targetAngle < 170 || targetAngle > 190)
			{
				return CurrentRun(turningRight, forwardSpeed);
			}
			
			// 180 turns should be based on footedness
			targetAngle = Mathf.Abs(targetAngle); 
			if (!leftPlanted) 
			{ 
				targetAngle *= -1; 
			} 
			return CurrentRun(leftPlanted, forwardSpeed);
		}
		
#if UNITY_EDITOR
		private int turnsFound;
		// Validate the durations of the turn animations
		public void OnValidate(Animator animator)
		{
			turnsFound = 0;
			// we get states from state machine, no need to look in blend trees for this.
			var animation = (AnimatorController)animator.runtimeAnimatorController;
			TraverseStatemachineToCheckStates(animation.layers[0].stateMachine);
			
			if (turnsFound < k_AnimationCount)
			{
				Debug.LogError("Did not find all turn states in state machine");
			}
		}

		private void TraverseStatemachineToCheckStates(AnimatorStateMachine stateMachine)
		{
			if (turnsFound == k_AnimationCount)
			{
				return;
			}
			foreach (var childState in stateMachine.states)
			{
				var clip = childState.state.motion as AnimationClip;
				if (clip != null)
				{
					CheckStateForTurn(childState.state);
					if (turnsFound == k_AnimationCount)
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
			if (state.name == sprintLeftTurn.name)
			{
				sprintLeftTurn.duration = state.motion.averageDuration;
				turnsFound++;
			}
			if (state.name == sprintRightTurn.name)
			{
				sprintRightTurn.duration = state.motion.averageDuration;
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