using UnityEngine;
using Util;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Stuck info and logic used by the OpenCharacterController.
	/// </summary>
	public class OpenCharacterControllerStuckInfo
	{
		/// <summary>
		/// If character's position does not change by more than this amount then we assume the character is stuck.
		/// </summary>
		private const float k_StuckDistance = 0.001f;

		/// <summary>
		/// If character's position does not change by more than this amount then we assume the character is stuck.
		/// </summary>
		private const float k_StuckSqrDistance = k_StuckDistance * k_StuckDistance;
		
		/// <summary>
		/// If character collided this number of times during the movement loop then test if character is stuck by
		/// examining the position
		/// </summary>
		private const int k_HitCountForStuck = 6;
		
		/// <summary>
		/// Assume character is stuck if the position is the same for longer than this number of loop itterations
		/// </summary>
		private const int k_MaxStuckPositionCount = 1;
		
		/// <summary>
		/// Is the character stuck in the current move loop itteration?
		/// </summary>
		public bool isStuck;
		
		/// <summary>
		/// Count the number of collisions during movement, to determine when the character gets stuck.
		/// </summary>
		public int hitCount;

		/// <summary>
		/// For keeping track of the character's position, to determine when the character gets stuck.
		/// </summary>
		private Vector3? stuckPosition;

		/// <summary>
		/// Count how long the character is in the same position.
		/// </summary>
		private int stuckPositionCount;

		/// <summary>
		/// Called when the move loop starts.
		/// </summary>
		public void OnMoveLoop()
		{
			hitCount = 0;
			stuckPositionCount = 0;
			stuckPosition = null;
			isStuck = false;
		}
		
		/// <summary>
		/// Is the character stuck during the movement loop (e.g. bouncing between 2 or more colliders)?
		/// </summary>
		/// <param name="characterPosition">The character's position.</param>
		/// <param name="currentMoveVector">Current move vector.</param>
		/// <param name="originalMoveVector">Original move vector.</param>
		/// <returns></returns>
		public bool UpdateStuck(Vector3 characterPosition, Vector3 currentMoveVector, 
		                        Vector3 originalMoveVector)
		{
			// First test
			if (isStuck == false)
			{
				// From Quake2: "if velocity is against the original velocity, stop dead to avoid tiny occilations in sloping corners"
				if (currentMoveVector.sqrMagnitude.NotEqualToZero() &&
				    Vector3.Dot(currentMoveVector, originalMoveVector) <= 0.0f)
				{
					isStuck = true;
				}
			}
			
			// Second test
			if (isStuck == false)
			{
				// Test if collided and while position remains the same
				if (hitCount < k_HitCountForStuck)
				{
					return false;
				}

				if (stuckPosition == null)
				{
					stuckPosition = characterPosition;
				}
				else if (stuckPosition.Value.SqrMagnitudeFrom(characterPosition) <= k_StuckSqrDistance)
				{
					stuckPositionCount++;
					if (stuckPositionCount > k_MaxStuckPositionCount)
					{
						isStuck = true;
					}
				}
				else
				{
					stuckPositionCount = 0;
					stuckPosition = null;
				}
			}

			if (isStuck)
			{
				isStuck = false;
				hitCount = 0;
				stuckPositionCount = 0;
				stuckPosition = null;

				return true;
			}

			return false;
		}
	}
}