using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Stuck info used by the CharacterCapsuleMover.
	/// </summary>
	public class CharacterCapsuleMoverStuckInfo
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
		/// If character collided this number of times during the movement loop then test if character is stuck
		/// </summary>
		private const int k_HitCountForStuck = 6;
		
		/// <summary>
		/// Assume character is stuck if the position is the same for longer than this number of loop itterations
		/// </summary>
		private const int k_MaxStuckCount = 1;

		/// <summary>
		/// If the remaining move distance has not changed by more than this value then assume it is the same.
		/// </summary>
		private const float k_RemainingMoveDistanceEpsilon = 0.0000001f;

		/// <summary>
		/// Max hit directions to keep track of, to determine if character is stuck between obstacles
		/// </summary>
		private const int k_MaxHitDirections = 6;
		
		/// <summary>
		/// Count the number of collisions during movement, to determine when the character gets stuck.
		/// </summary>
		public int hitCount;

		/// <summary>
		/// For keeping track of the character's position, to determine when the character gets stuck.
		/// </summary>
		public Vector3? stuckPosition;

		/// <summary>
		/// Count how long the character is in the same position.
		/// </summary>
		public int stuckCount;

		/// <summary>
		/// Is the character stuck in the current move loop itteration?
		/// </summary>
		public bool isStuck;

		/// <summary>
		/// Keep track of the max remaining distance during collision, to determine if character gets stuck
		/// </summary>
		public float? stuckMaxRemainingDistance;

		/// <summary>
		/// List of hit directions to determine if character is bouncing between obstacles.
		/// </summary>
		private List<Vector3> hitDirections = new List<Vector3>(k_MaxHitDirections);

		/// <summary>
		/// Called when the move loop starts.
		/// </summary>
		public void OnMoveLoop()
		{
			hitCount = 0;
			stuckCount = 0;
			stuckPosition = null;
			isStuck = false;
			stuckMaxRemainingDistance = null;
		}
		
		/// <summary>
		/// Is the character stuck during the movement loop (e.g. bouncing between 2 or more colliders)?
		/// </summary>
		/// <param name="characterPosition">The character's position.</param>
		/// <returns></returns>
		public bool UpdateStuck(Vector3 characterPosition)
		{
			if (isStuck == false)
			{
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
					stuckCount++;
					if (stuckCount > k_MaxStuckCount)
					{
						isStuck = true;
					}
				}
				else
				{
					stuckCount = 0;
					stuckPosition = null;
				}
			}

			if (isStuck)
			{
				isStuck = false;
				hitCount = 0;
				stuckCount = 0;
				stuckPosition = null;
				stuckMaxRemainingDistance = null;

				return true;
			}

			return false;
		}

		/// <summary>
		/// Called when the character collided with an obstacles. It determine's if the character is stuck.
		/// </summary>
		/// <param name="hitDirection">Direction character travelled when hitting an obstacle.</param>
		public void OnCollided(Vector3 hitDirection)
		{
			// We only care if the direction has horizontal movement
			if (hitDirection.x.IsEqualToZero() &&
			    hitDirection.z.IsEqualToZero())
			{
				return;
			}
			
			hitDirections.Add(hitDirection);
			
			// TEMP: log directions and angles between them
			if (hitDirections.Count >= k_MaxHitDirections)
			{
				int anglesCount = 0;
				StringBuilder sb = new StringBuilder();
				Vector3 prevDirection = hitDirections[0];
				for (int i = 1, len = hitDirections.Count; i < len; i++)
				{
					Vector3 direction = hitDirections[i];
					float angle = Vector3.Angle(prevDirection, direction);
					if (angle >= 80.0f)
					{
						anglesCount++;
					}
					sb.AppendLine(string.Format("angle: {0}     dir: ({1}, {2}, {3})",
					                        angle,
					                        direction.x, direction.y, direction.z));
				}
				hitDirections.Clear();
				
				/*if (anglesCount > 2)
				{
					isStuck = true;
					sb.AppendLine(string.Format("STUCK: {0}",
					                            anglesCount));
				}*/
				
				Debug.Log(sb.ToString());
			}
		}
	}
}