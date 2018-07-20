using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Info for stepping over small obstacles (i.e. if the character's stepOffset > 0).
	/// This is used by the OpenCharacterController.
	/// </summary>
	public class CharacterCapsuleMoverStepInfo
	{
		/// <summary>
		/// Continue stepping if the angle between the original move vector (which started the stepping) and the new vector
		/// is smaller than this (degrees).
		/// </summary>
		private const float k_MinAngleFromStartVector = 0.5f;

		/// <summary>
		/// If updates between Move methods take longer than this then assume the stepping stopped.
		/// </summary>
		private const float k_MaxTimeSinceLastUpdate = 1.0f;

		/// <summary>
		/// Remaining height to step over.
		/// </summary>
		private float remainingHeight;

		/// <summary>
		/// Target height to step over.
		/// </summary>
		private float targetHeight;

		/// <summary>
		/// The move vector that started the step over process.
		/// </summary>
		private Vector3 startMoveVector;

		/// <summary>
		/// Position when the step over process started.
		/// </summary>
		private Vector3 startPosition;

		/// <summary>
		/// Time.realtimeSinceStartup when the step was last updated. Used to determine if a step should coninue between Move methods.
		/// </summary>
		private float lastUpdateTime;
		
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
			float heightClimbed = position.y - startPosition.y;
			
			if (heightClimbed < 0.0f)
			{
				// Moved down past the original position (assume the climbing stopped)
				remainingHeight = 0.0f;
			}
			else
			{
				remainingHeight = targetHeight - heightClimbed;
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
			targetHeight = heightToStep;
			remainingHeight = heightToStep;
			startMoveVector = moveVector;
			startPosition = position;
			lastUpdateTime = Time.realtimeSinceStartup;
		}

		/// <summary>
		/// Called when the step was updated.
		/// </summary>
		public void OnUpdate()
		{
			lastUpdateTime = Time.realtimeSinceStartup;
		}
		
		/// <summary>
		/// Called when stop to step over obstcales.
		/// </summary>
		public void OnStopStepOver()
		{
			isStepping = false;
		}

		/// <summary>
		/// Called when the move loop starts. It checks if the step over should continue.
		/// </summary>
		/// <param name="moveVector">The new move vector.</param>
		public bool OnMoveLoop(Vector3 moveVector)
		{
			// Not busy stepping over obstacles OR too much time elapsed sine the last Move?
			if (isStepping == false ||
			    Time.realtimeSinceStartup - lastUpdateTime > k_MaxTimeSinceLastUpdate)
			{
				return false;
			}

			// Continue stepping if the angle between the original vector and the new vector is
			// small enough (i.e. character has not changed direction).
			// Ignore Y component of vector.
			return Vector3.Angle(new Vector3(startMoveVector.x, 0.0f, startMoveVector.z), 
			                     new Vector3(moveVector.x, 0.0f, moveVector.z)) < k_MinAngleFromStartVector;
		}
	}
}