using System;
using UnityEngine;
using Util;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace StandardAssets.Characters.ThirdPerson
{
	/// <inheritdoc />
	/// <summary>
	/// Animation extension of TurnaroundBehaviour. Rotates the character to the target angle while playing an animation.
	/// </summary>
	/// <remarks>This turnaround type should be used to improve fidelity at the cost of responsiveness.</remarks>
	[Serializable]
	public class AnimationTurnaroundBehaviour : TurnaroundBehaviour
	{
		/// <summary>
		/// Model to store data per animation turnaround
		/// </summary>
		[Serializable]
		protected class AnimationInfo
		{
			// State name
			public string name;
			// Animation play speed
			public float speed = 1;
			// Head look at angle scale during animation
			public float headTurnScale = 1;
			[HideInInspector]
			// clip duration. HideInInspector as it should only be edited by the editor code below
			public float duration;

			public AnimationInfo(string name)
			{
				this.name = name;
			}
		}

		// the data for each animation turnaround
		[SerializeField, Tooltip("Data for run 180 left turn animation")]
		protected AnimationInfo runLeftTurn = new AnimationInfo("RunForwardTurnLeft180");
		[SerializeField, Tooltip("Data for run 180 right turn animation")]
		protected AnimationInfo runRightTurn = new AnimationInfo("RunForwardTurnRight180_Mirror");
		[SerializeField, Tooltip("Data for sprint 180 left turn animation")]
		protected AnimationInfo sprintLeftTurn = new AnimationInfo("RunForwardTurnLeft180");
		[SerializeField, Tooltip("Data for sprint 180 right turn animation")]
		protected AnimationInfo sprintRightTurn = new AnimationInfo("RunForwardTurnRight180_Mirror");
		[SerializeField, Tooltip("Data for idle 180 left turn animation")]
		protected AnimationInfo idleLeftTurn = new AnimationInfo("IdleTurnLeft180");
		[SerializeField, Tooltip("Data for idle 180 right turn animation")]
		protected AnimationInfo	idleRightTurn = new AnimationInfo("IdleTurnRight180_Mirror");

		[SerializeField, Tooltip("Curve used to determine rotation during animation")] 
		protected AnimationCurve rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);

		[SerializeField, Tooltip("Value used to determine if a run turn should be used")]
		protected float normalizedRunSpeedThreshold = 0.1f;
		
		[SerializeField, Tooltip("Duration of the cross fade into turn animation")] 
		protected float crossfadeDuration = 0.125f;

		private float animationTime, // current animation time, incremented each frame
			targetAngle, // target y rotation angle in degrees
			cachedAnimatorSpeed, // speed of the animator prior to starting an animation turnaround
			cacheForwardSpeed; // forwards speed of the motor prior to starting an animation turnaround
		private Quaternion startRotation; // rotation of the character as turnaround is started
		private AnimationInfo currentAnimationInfo; // currently selected animation info
		private ThirdPersonAnimationController animationController;
		private Transform transform; // character's transform

		/// <inheritdoc />
		public override float headTurnScale
		{
			get
			{
				return currentAnimationInfo == null ? 1 : currentAnimationInfo.headTurnScale;
			}
		}

		private Animator animator
		{
			get { return animationController.unityAnimator; }
		}

		public override void Init(ThirdPersonBrain brain)
		{
			animationController = brain.animationControl;
			transform = brain.transform;
		}

		/// <summary>
		/// Rotates the character toward <see cref="targetAngle"/> using the animation's normalized progress/>
		/// </summary>
		public override void Update()
		{
			if (!isTurningAround)
			{
				return;
			}
			animationController.UpdateForwardSpeed(cacheForwardSpeed, float.MaxValue);
			animationTime += Time.deltaTime * currentAnimationInfo.speed;
			var rotationProgress = rotationCurve.Evaluate(animationTime / currentAnimationInfo.duration);
			transform.rotation = Quaternion.AngleAxis(rotationProgress * targetAngle, Vector3.up) * startRotation;
			// animation complete, blending to locomotion
			if(animationTime >= currentAnimationInfo.duration)
			{
				animator.speed = cachedAnimatorSpeed;
				EndTurnAround();
			}
		}

		/// <inheritdoc />
		public override Vector3 GetMovement()
		{
			if (currentAnimationInfo == idleLeftTurn || currentAnimationInfo == idleRightTurn)
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
			currentAnimationInfo = GetCurrent(animationController.animatorForwardSpeed, angle > 0,
				!animationController.isRightFootPlanted);

			startRotation = transform.rotation;
			animator.CrossFade(currentAnimationInfo.name, crossfadeDuration, 0, 0);
			animationTime = 0;

			cachedAnimatorSpeed = animator.speed;
			animator.speed = currentAnimationInfo.speed;

			cacheForwardSpeed = animationController.animatorForwardSpeed;
		}

		/// <summary>
		/// Determines which animation should be played
		/// </summary>
		/// <param name="forwardSpeed">Character's normalized forward speed</param>
		/// <param name="turningClockwise">Is the character turning clockwise</param>
		/// <param name="leftFootPlanted">Is the character's left foot currently planted</param>
		/// <returns>The determined AnimationInfo</returns>
		private AnimationInfo GetCurrent(float forwardSpeed, bool turningClockwise, bool leftFootPlanted)
		{
			// idle turn
			if (forwardSpeed < normalizedRunSpeedThreshold)
			{
				return turningClockwise ? idleRightTurn : idleLeftTurn;
			}
			
			// < 180 turn
			if (targetAngle < 170 || targetAngle > 190)
			{
				return CurrentRun(forwardSpeed, turningClockwise);
			}
			
			// 180 turns should be based on footedness
			targetAngle = Mathf.Abs(targetAngle); 
			if (!leftFootPlanted) 
			{ 
				targetAngle *= -1; 
			} 
			return CurrentRun(forwardSpeed, leftFootPlanted);
		}

		/// <summary>
		/// Determines if the run or sprint AnimationInfo should be selected
		/// </summary>
		/// <param name="forwardSpeed">Character's normalized forward speed</param>
		/// <param name="turningRight">Is the character turning clockwise</param>
		/// <returns>The determined AnimationInfo</returns>
		private AnimationInfo CurrentRun(float forwardSpeed, bool turningRight)
		{
			if (turningRight)
			{
				return forwardSpeed <= 1 ? runRightTurn : sprintRightTurn;
			}
			return forwardSpeed <= 1 ? runLeftTurn : sprintLeftTurn;
		}
		
#if UNITY_EDITOR
		
		/*
		TODO
		Below is editor logic to retrieve the duration of the clips used for the turnaround. This is required as
		normalized animator time cannot be used due to transitions. This solution is not ideal as edits to the 
		animator would require this to run but this only runs in OnValidate().
		*/
		
		private const int k_AnimationCount = 6;
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