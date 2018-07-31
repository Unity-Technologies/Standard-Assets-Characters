using System;
using System.Linq;
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
		
#if UNITY_EDITOR
		private int turnsFound;
		public void OnValidate(Animator animator)
		{
			turnsFound = 0;
			// we get states from state machine, no need to look in blend trees for this.
			var animation = animator.runtimeAnimatorController as AnimatorController;
			TraverseStatemachineToCheckStates(animation.layers[0].stateMachine);
			
			if (turnsFound < 4)
			{
				Debug.LogError("Did not find all turn states in state machine");
			}
		}

		private void TraverseStatemachineToCheckStates(AnimatorStateMachine stateMachine)
		{
			if (turnsFound == 4)
			{
				return;
			}
			foreach (var childState in stateMachine.states)
			{
				var clip = childState.state.motion as AnimationClip;
				if (clip != null)
				{
					CheckStateForTurn(childState.state);
					if (turnsFound == 4)
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
		}
#endif
	}
}