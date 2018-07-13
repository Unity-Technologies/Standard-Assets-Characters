using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Info for stepping over small obstacles (i.e. if the character's heightToStep > 0).
	/// </summary>
	public class CharacterCapsuleStepInfo
	{
		/// <summary>
		/// Continue stepping if the angle between the original move vector (which started the stepping) and the new vector
		/// is smaller than this (degrees).
		/// </summary>
		private const float k_MinAngleFromStartVector = 0.5f;

		/// <summary>
		/// Remaining height to step over.
		/// </summary>
		private float remainingHeight;

		/// <summary>
		/// The move vector that started the step over process.
		/// </summary>
		private Vector3 startMoveVector;

		/// <summary>
		/// Position when the step over process started.
		/// </summary>
		private Vector3 startPosition;
		
		/// <summary>
		/// Busy stepping over obstacles?
		/// </summary>
		public bool isStepping { get; private set; }

		/// <summary>
		/// Get the remaining height to climb up.
		/// </summary>
		/// <param name="position">The position of the capsule.</param>
		/// <returns></returns>
		public float GetRemainingHeight(Vector3 position)
		{
			float distance = position.y - startPosition.y;
			
			// Moved up?
			if (distance > 0.0f)
			{
				// Reduce the remaining distance as the character moves up
				remainingHeight -= distance;
			}
			else if (distance < 0.0f)
			{
				// Moved down
				// When moving down then assume the climbing stopped
				remainingHeight = 0.0f;
			}

			return Mathf.Max(remainingHeight, 0.0f);
		}
		
		/// <summary>
		/// Called when starting to step over obstacles.
		/// </summary>
		/// <param name="heightToStep">Height to step up.</param>
		/// <param name="moveVector">Move vector.</param>
		/// <param name="position">The position of the capsule.</param>
		public void OnStartStepOver(float heightToStep, Vector3 moveVector, Vector3 position)
		{
			isStepping = true;
			remainingHeight = heightToStep;
			startMoveVector = moveVector;
			startPosition = position;
		}

		/// <summary>
		/// Called when stop to step over obstcales.
		/// </summary>
		public void OnStopStepOver()
		{
			isStepping = false;
		}

		/// <summary>
		/// Called when a new move vector is used to move the character. It checks if the step over should continue.
		/// </summary>
		/// <param name="moveVector">The new move vector.</param>
		public bool OnNewMoveVector(Vector3 moveVector)
		{
			if (isStepping == false)
			{
				// Not busy stepping over obstacles
				return false;
			}

			// Continue stepping if the angle between the original vector and the new vector is
			// small enough (i.e. character has not changed direction).
			return Vector3.Angle(startMoveVector, moveVector) < k_MinAngleFromStartVector;
		}
	}
}