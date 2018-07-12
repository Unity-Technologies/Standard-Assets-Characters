using System;
using System.Collections.Generic;
using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Handles the movement of a CharacterCapsule.
	/// </summary>
	public class CharacterCapsuleMover : MonoBehaviour, ICharacterCapsuleMover
	{
		/// <summary>
		/// The maximum move itterations. Mainly used as a fail safe to prevent an infinite loop.
		/// </summary>
		private const int k_MaxMoveItterations = 100;

		/// <summary>
		/// Stick to the ground if it is less than this distance from the character.
		/// </summary>
		private const float k_MaxStickToGroundDownDistance = 1.0f;

		/// <summary>
		/// Max colliders to use in the overlap methods.
		/// </summary>
		private const int k_MaxOverlapColliders = 10;

		/// <summary>
		/// Offset to use when moving to a collision point, to prevent overlapping the colliders
		/// </summary>
		private const float k_CollisionOffset = 0.001f;

		/// <summary>
		/// Distance to test beneath the character when doing the grounded test
		/// </summary>
		private const float k_GroundedTestDistance = 0.001f;

		/// <summary>
		/// If character's position does not change by more than this amount then we assume the character is stuck.
		/// </summary>
		private const float k_StuckDistance = 0.001f;
		
		/// <summary>
		/// If character collided this number of times during the movement loop then test if character is stuck
		/// </summary>
		private const int k_HitCountForStuck = 6;
		
		/// <summary>
		/// Assume character is stuck if the position is the same for longer than this number of loop itterations
		/// </summary>
		private const int k_MaxStuckCount = 1;

		/// <summary>
		/// Only step over obstacles if the angle between the move vector and the obstacle's inverted normal is less than this.
		/// </summary>
		private const float k_MaxStepOverHitAngle = 90.0f;

		/// <summary>
		/// Minimum distance to move. This minimizes small penetrations (e.g. into the floor)
		/// </summary>
		private const float k_MinMoveDistance = 0.0001f;
		
		/// <summary>
		/// Minimum sqr distance to move. This minimizes small penetrations (e.g. into the floor)
		/// </summary>
		private const float k_MinMoveSqrDistance = k_MinMoveDistance * k_MinMoveDistance;

		/// <inheritdoc />
		public event Action<bool> onGroundedChanged;

		/// <inheritdoc />
		public event Action<Vector3> onVelocityChanged;

		/// <inheritdoc />
		public event Action<CollisionFlags> onCollisionFlagsChanged;

		/// <inheritdoc />
		public event Action<bool> onStepOverChanged;

		/// <summary>
		/// Was the touching the ground during the last move?
		/// </summary>
		private bool isGrounded;

		/// <summary>
		/// Collision flags from the last move.
		/// </summary>
		private CollisionFlags collisionFlags;

		/// <summary>
		/// The character capsule to move.
		/// </summary>
		private CharacterCapsule characterCapsule;
		
		/// <summary>
		/// Cached reference to the capsule's transform.
		/// </summary>
		private Transform capsuleTransform;
		
		/// <summary>
		/// Remaining movement vectors.
		/// </summary>
		private List<Vector3> moveVectors = new List<Vector3>();
		
		/// <summary>
		/// Index into the moveVectors array.
		/// </summary>
		private int moveVectorIndex;

		/// <summary>
		/// Velocity of the last movement. It's the new position minus the old position.
		/// </summary>
		private Vector3 velocity;

		/// <summary>
		/// Count the number of collisions during movement, to determine when the character gets stuck.
		/// </summary>
		private int hitCount;

		/// <summary>
		/// For keeping track of the character's position, to determine when the character gets stuck.
		/// </summary>
		private Vector3? stuckPosition;

		/// <summary>
		/// Count how long the character is in the same position.
		/// </summary>
		private int stuckCount;
		
		/// <summary>
		/// Stepping over obstacles info.
		/// </summary>
		private CharacterCapsuleStepInfo stepInfo = new CharacterCapsuleStepInfo();
		
		#if UNITY_EDITOR
		/// <summary>
		/// Keeps track of the character's position. Used to show an error when character is moved by means other than the Move method.
		/// </summary>
		private Vector3? debugCurrentPosition;
		/// <summary>
		/// Limits how many times we show the error message, when debugCurrentPosition not same as character's position.
		/// </summary>
		private int debugCurrentPositionErrorCount;
		#endif

		/// <inheritdoc />
		public CollisionFlags Move(Vector3 moveVector)
		{
			float sqrDistance = moveVector.sqrMagnitude;
			if (sqrDistance <= Mathf.Epsilon ||
			    sqrDistance < characterCapsule.GetMinMoveSqrDistance() ||
			    sqrDistance < k_MinMoveSqrDistance)
			{
				return CollisionFlags.None;
			}

			bool wasGrounded = isGrounded;
			Vector3 oldPosition = capsuleTransform.position;
			Vector3 oldVelocity = velocity;
			CollisionFlags oldCollisionFlags = collisionFlags;
			bool oldIsStepping = stepInfo.isStepping;
			Vector3 moveVectorNoY = new Vector3(moveVector.x, 0.0f, moveVector.z);
			bool tryToStickToGround = (wasGrounded &&
			                           moveVector.y <= 0.0f &&
			                           Math.Abs(moveVectorNoY.sqrMagnitude) > Mathf.Epsilon);
			
			collisionFlags = CollisionFlags.None;
			hitCount = 0;

			// Do the move loop
			MoveLoop(moveVector, tryToStickToGround);

			isGrounded = GetGrounded(wasGrounded, oldPosition);
			velocity = capsuleTransform.position - oldPosition;

			// Check is the changed events should be fired
			if (onVelocityChanged != null &&
			    oldVelocity != velocity)
			{
				onVelocityChanged(velocity);
			}
			
			if (onCollisionFlagsChanged != null &&
			    oldCollisionFlags != collisionFlags)
			{
				onCollisionFlagsChanged(collisionFlags);
			}
			
			if (onGroundedChanged != null &&
			    wasGrounded != isGrounded)
			{
				onGroundedChanged(isGrounded);
			}

			if (onStepOverChanged != null &&
			    oldIsStepping != stepInfo.isStepping)
			{
				onStepOverChanged(stepInfo.isStepping);
			}

			return collisionFlags;
		}

		/// <inheritdoc />
		private void Awake()
		{
			characterCapsule = (CharacterCapsule)GetComponent(typeof(CharacterCapsule));
			capsuleTransform = characterCapsule.transform;
		}

		/// <summary>
		/// Determine if the character is grounded.
		/// </summary>
		/// <returns></returns>
		private bool GetGrounded(bool wasGrounded, Vector3 oldPosition)
		{
			if ((collisionFlags & CollisionFlags.CollidedBelow) != 0)
			{
				return true;
			}

			Ray ray = new Ray(characterCapsule.GetBottomSphereWorldPosition(), Vector3.down);
			return UnityEngine.Physics.SphereCast(ray, 
												  characterCapsule.scaledRadius, 
												  characterCapsule.GetSkinWidth() + k_GroundedTestDistance,
												  characterCapsule.GetCollisionLayerMask());
		}

		/// <summary>
		/// Movement loop. Keep moving until completely blocked by obstacles, or we reached the desired position/distance.
		/// </summary>
		/// <param name="moveVector">The move vector.</param>
		/// <param name="tryToStickToGround">Try to stick to the ground?</param>
		private void MoveLoop(Vector3 moveVector, bool tryToStickToGround)
		{
			moveVectors.Clear();

			Vector3 remainingMoveVector;
			int tryStepOverIndex = -1;

			// Break the movement up into horizontal and vertical
			Vector3 horizontal = new Vector3(moveVector.x, 0.0f, moveVector.z);
			Vector3 vertical = new Vector3(0.0f, moveVector.y, 0.0f);
			
			if (vertical.y > 0.0f)
			{
				// TODO: Handle cases where the vector is large
				if (Math.Abs(horizontal.x) > Mathf.Epsilon ||
				    Math.Abs(horizontal.z) > Mathf.Epsilon)
				{
					// Move up then horizontal
					remainingMoveVector = vertical;
					moveVectors.Add(horizontal);
				}
				else
				{
					// Move up
					remainingMoveVector = vertical;
				}
			}
			else if (vertical.y < 0.0f)
			{
				// TODO: Handle cases where the vector is large
				if (Math.Abs(horizontal.x) > Mathf.Epsilon ||
				    Math.Abs(horizontal.z) > Mathf.Epsilon)
				{
					// Move horizontal then down
					remainingMoveVector = horizontal;
					moveVectors.Add(vertical);
				}
				else
				{
					// Move down
					remainingMoveVector = vertical;
				}
			}
			else
			{
				// Move horizontal
				remainingMoveVector = horizontal;
				tryStepOverIndex = 0;
			}

			moveVectorIndex = 0;
			bool didTryToStickToGround = false;
			stuckCount = 0;
			stuckPosition = null;
			
			// TEMP: Disable stepOffset. Some bugs to fix first before commiting it.
			tryStepOverIndex = -1;

			if (stepInfo.isStepping &&
			    stepInfo.OnNewMoveVector(remainingMoveVector) == false)
			{
				StopStepOver(true);
			}

			for (int i = 0; i < k_MaxMoveItterations; i++)
			{				
				bool collided = MoveMajorStep(ref remainingMoveVector, 
				                              didTryToStickToGround, 
				                              i == tryStepOverIndex || stepInfo.isStepping);

				if (IsStuck())
				{
					remainingMoveVector = Vector3.zero;
				}
				
				// Collided or vector used up (i.e. vector is zero)?
				if (!collided || 
				    remainingMoveVector.sqrMagnitude <= Mathf.Epsilon)
				{
					// Are there remaining movement vectors?
					if (moveVectorIndex < moveVectors.Count)
					{
						remainingMoveVector = moveVectors[moveVectorIndex];
						moveVectorIndex++;
					}
					else
					{
						if (!tryToStickToGround || 
						    didTryToStickToGround)
						{
							break;
						}
						
						// Try to stick to the ground
						didTryToStickToGround = true;
						if (!CanStickToGround(moveVector, out remainingMoveVector))
						{
							break;
						}
					}
				}
				
				#if UNITY_EDITOR
				if (i == k_MaxMoveItterations - 1)
				{					
					Debug.LogError(string.Format("reached k_MaxMoveItterations!     (remainingMoveVector: {0}, {1}, {2})     " +
												 "(moveVector: {3}, {4}, {5})     hitCount: {6}",
												 remainingMoveVector.x, remainingMoveVector.y, remainingMoveVector.z,
												 moveVector.x, moveVector.y, moveVector.z,
												 hitCount));
				}
				#endif
			}
		}

		/// <summary>
		/// Is the move loop on the final move vector?
		/// </summary>
		/// <returns></returns>
		private bool IsFinalMoveVector()
		{
			return moveVectors.Count == 0 ||
			       moveVectorIndex >= moveVectors.Count;
		}

		/// <summary>
		/// Is the character stuck during the movement loop (e.g. bouncing between 2 or more colliders)?
		/// </summary>
		/// <returns></returns>
		private bool IsStuck()
		{
			if (hitCount < k_HitCountForStuck)
			{
				return false;
			}

			if (stuckPosition == null)
			{
				stuckPosition = capsuleTransform.position;
			}
			else if (VectorSqrMagnitude(stuckPosition.Value, capsuleTransform.position) <= k_StuckDistance * k_StuckDistance)
			{
				stuckCount ++;
				if (stuckCount > k_MaxStuckCount)
				{
					hitCount = 0;
					stuckCount = 0;
					stuckPosition = null;
					return true;
				}
			}
			else
			{
				stuckCount = 0;
				stuckPosition = null;
			}

			return false;
		}

		/// <summary>
		/// Get the square magnitude of the difference of the two vectors.
		/// </summary>
		/// <returns>The sqr magnitude.</returns>
		/// <param name="vectorA">First vector.</param>
		/// <param name="vectorB">Second vector.</param>
		private float VectorSqrMagnitude(Vector3 vectorA, Vector3 vectorB)
		{
			Vector3 diff = vectorA - vectorB;
			return diff.sqrMagnitude;
		}

		/// <summary>
		/// Test if character can stick to the ground, and set the down vector if so.
		/// </summary>
		/// <param name="moveVector">The original movement vector.</param>
		/// <param name="getDownVector">Get the down vector.</param>
		/// <returns></returns>
		private bool CanStickToGround(Vector3 moveVector, out Vector3 getDownVector)
		{
			Vector3 moveVectorNoY = new Vector3(moveVector.x, 0.0f, moveVector.z);
			float downDistance = moveVectorNoY.magnitude;
			
			if (downDistance <= k_MaxStickToGroundDownDistance)
			{
				getDownVector = Vector3.down * downDistance;
				return true;
			}
			
			getDownVector = Vector3.zero;
			return false;
		}

		/// <summary>
		/// Try to start stepping over obstacles.
		/// Sets up the movement vectors to step over an obstacle.
		/// </summary>
		/// <param name="moveVector">The movement vector.</param>
		/// <param name="remainingDistance">The remaining distance to move</param>
		/// <param name="angleToObstacle">Angle between the move vector and the obstacle's inverted normal.</param>
		/// <returns></returns>
		private bool TryToStartStepOver(Vector3 moveVector, float remainingDistance, float angleToObstacle)
		{
			float stepOffset = characterCapsule.GetStepOffset();
			if (stepOffset <= 0.0f ||
			    (collisionFlags & CollisionFlags.CollidedSides) == 0 ||
			    isGrounded == false ||
			    angleToObstacle > k_MaxStepOverHitAngle ||
			    Math.Abs(moveVector.y) > Mathf.Epsilon ||
			    (Math.Abs(moveVector.x) < Mathf.Epsilon &&
			     Math.Abs(moveVector.z) < Mathf.Epsilon))
			{
				return false;
			}
			
			// TODO: Need to handle cases where small obstacles hit our side edges, but we don't want to try to step over
			// them. Instead slide along them.

			// Any obstacles above?
			float upDistance = stepOffset;
			Vector3 up = Vector3.up * upDistance;
			Ray ray = new Ray(characterCapsule.GetTopSphereWorldPosition(), Vector3.up);
			if (UnityEngine.Physics.SphereCast(ray,
			                                   characterCapsule.scaledRadius,
			                                   characterCapsule.GetSkinWidth() + upDistance,
			                                   characterCapsule.GetCollisionLayerMask()))
			{				
				return false;
			}
			
			// We only step over obstacles if we can fully fit on it (i.e. the capsule's diameter)
			Vector3 moveVectorNoY = new Vector3(moveVector.x, 0.0f, moveVector.z).normalized;
			float diameter = characterCapsule.scaledRadius * 2.0f;
			Vector3 horizontal = moveVectorNoY * diameter;
			RaycastHit hitInfo;
			// Any obstacles ahead (after we moved up)?
			if (CapsuleBigCast(horizontal.normalized, horizontal.magnitude, out hitInfo, up))
			{
				return false;
			}
			
			// Determine how high we actually need to step (e.g. if obstacle height is less than step height)
			// Test how far character will fall (after moving up and horizontal)
			ray = new Ray(characterCapsule.GetBottomSphereWorldPosition() + up + horizontal, Vector3.down);
			if (UnityEngine.Physics.SphereCast(ray,
			                                   characterCapsule.scaledRadius,
			                                   out hitInfo,
			                                   characterCapsule.GetSkinWidth() + stepOffset,
			                                   characterCapsule.GetCollisionLayerMask()))
			{
				float downDistance = Mathf.Max(hitInfo.distance - k_CollisionOffset, 0.0f);
				if (downDistance > 0.0f)
				{
					float finalY = capsuleTransform.position.y + up.y - downDistance;
					upDistance = Mathf.Max(finalY - capsuleTransform.position.y, 0.0f);
					if (upDistance <= 0.0f)
					{
						return false;
					}

					if (upDistance < stepOffset)
					{
						up = Vector3.up * upDistance;
					}
				}
			}

			// Move Up
			moveVectors.Add(up);
			
			// Move Horizontal
			horizontal = moveVectorNoY * remainingDistance;
			moveVectors.Add(horizontal);
			
			// Start stepping over the obstacles
			stepInfo.OnStartStepOver(upDistance, moveVector, capsuleTransform.position);
			
			return true;
		}

		/// <summary>
		/// Update stepping over obstacles.
		/// </summary>
		/// <param name="moveVector">Movement vector.</param>
		/// <returns></returns>
		private bool UpdateStepOver(Vector3 moveVector)
		{
			float stepOffset = characterCapsule.GetStepOffset();
			if (stepOffset <= 0.0f)
			{
				return false;
			}
			
			float upDistance = stepInfo.GetRemainingHeight(capsuleTransform.position);
			if (upDistance <= 0.0f)
			{
				return false;
			}

			if (Math.Abs(moveVector.x) < Mathf.Epsilon &&
			    Math.Abs(moveVector.z) < Mathf.Epsilon)
			{
				// Wait until we get a horizontal vector
				return true;
			}
			
			// Any obstacles above?
			Vector3 up = Vector3.up * upDistance;
			Ray ray = new Ray(characterCapsule.GetTopSphereWorldPosition(), Vector3.up);
			if (UnityEngine.Physics.SphereCast(ray,
			                                   characterCapsule.scaledRadius,
			                                   characterCapsule.GetSkinWidth() + upDistance,
			                                   characterCapsule.GetCollisionLayerMask()))
			{
				// Stop stepping over
				return false;
			}
				
			// Move Up
			moveVectors.Add(up);
			
			return true;
		}

		/// <summary>
		/// Stop stepping over obstacles.
		/// </summary>
		/// <param name="fallDown"></param>
		private void StopStepOver(bool fallDown)
		{
			stepInfo.OnStopStepOver();

			if (fallDown == false)
			{
				return;
			}
			
			// Determine how far down we should fall
			float maxDistance = characterCapsule.GetStepOffset();
			Vector3 down;
			RaycastHit hitInfo;
			Ray ray = new Ray(characterCapsule.GetBottomSphereWorldPosition(), Vector3.down);
			if (UnityEngine.Physics.SphereCast(ray,
			                                   characterCapsule.scaledRadius,
			                                   out hitInfo,
			                                   characterCapsule.GetSkinWidth() + maxDistance,
			                                   characterCapsule.GetCollisionLayerMask()))
			{
				float downDistance = Mathf.Max(hitInfo.distance - k_CollisionOffset, 0.0f);
				down = Vector3.down * downDistance;
			}
			else
			{
				down = Vector3.down * maxDistance;
			}
			
			if (Math.Abs(down.y) > Mathf.Epsilon)
			{
				// Move Down
				moveVectors.Add(down);
			}
		}
		
		/// <summary>
		/// A single movement major step. Returns true when there is collision.
		/// </summary>
		/// <param name="moveVector">The move vector.</param>
		/// <param name="tryGrounding">Try grounding the player?</param>
		/// <param name="tryStepOver">Try to step over obstacles?</param>
		/// <returns>True when there is collision.</returns>
		private bool MoveMajorStep(ref Vector3 moveVector, bool tryGrounding, bool tryStepOver)
		{
			Vector3 direction = moveVector.normalized;
			float distance = moveVector.magnitude;
			RaycastHit bigRadiusHitInfo;
			RaycastHit smallRadiusHitInfo;
			bool smallRadiusHit;
			bool bigRadiusHit;

			if (!CapsuleCast(direction, distance,
			                 out smallRadiusHit, out bigRadiusHit,
			                 out smallRadiusHitInfo, out bigRadiusHitInfo))
			{
				// No collision, so move to the position
				MovePosition(moveVector, null, null, "no collision");

				// Stepping and the final movement?
				if (stepInfo.isStepping && 
				    IsFinalMoveVector())
				{
					StopStepOver(false);
				}
				
				moveVector = Vector3.zero;

				return false;
			}

			// Did the big radius not hit an obstacle?
			if (bigRadiusHit == false)
			{
				// The small radius hit an obstacle, so character is inside an obstacle
				MoveAwayFromObstacle(ref moveVector, ref smallRadiusHitInfo,
				                     direction, distance, 
									 tryGrounding,
				                     tryStepOver,
				                     true,
				                     "NoBigCollision");
				
				return true;
			}

			// Use the nearest collision point (e.g. to handle cases where 2 or more colliders' edges meet)
			if (smallRadiusHit && 
			    smallRadiusHitInfo.distance < bigRadiusHitInfo.distance)
			{
				MoveAwayFromObstacle(ref moveVector, ref smallRadiusHitInfo,
				                     direction, distance,
									 tryGrounding,
				                     tryStepOver,
				                     true,
				                     "UseSmallCollision");
				return true;
			}

			MoveAwayFromObstacle(ref moveVector, ref bigRadiusHitInfo,
								 direction, distance,
								 tryGrounding,
			                     tryStepOver,
			                     false,
								 "UseBigCollision");
			
			return true;
		}

		/// <summary>
		/// Do two capsule casts. One excluding the capsule's skin width and one including the skin width.
		/// </summary>
		/// <param name="direction">Direction to cast.</param>
		/// <param name="distance">Distance to cast.</param>
		/// <param name="smallRadiusHit">Did hit, excluding the skin width?</param>
		/// <param name="bigRadiusHit">Did hit, including the skin width?</param>
		/// <param name="smallRadiusHitInfo">Hit info for cast exlucing the skin width.</param>
		/// <param name="bigRadiusHitInfo">Hit info for cast including the skin width.</param>
		/// <returns></returns>
		private bool CapsuleCast(Vector3 direction, float distance, 
		                         out bool smallRadiusHit, out bool bigRadiusHit,
		                         out RaycastHit smallRadiusHitInfo, out RaycastHit bigRadiusHitInfo)
		{			
			// Exclude the skin width in the test
			smallRadiusHit = CapsuleSmallCast(direction, distance, out smallRadiusHitInfo, Vector3.zero);
			
			// Include the skin width in the test
			bigRadiusHit = CapsuleBigCast(direction, distance, out bigRadiusHitInfo, Vector3.zero);

			return smallRadiusHit ||
			       bigRadiusHit;
		}

		/// <summary>
		/// Do a capsule cast, exlucliding the skin width.
		/// </summary>
		/// <param name="direction">Direction to cast.</param>
		/// <param name="distance">Distance to cast.</param>
		/// <param name="smallRadiusHitInfo">Hit info.</param>
		/// <param name="offsetPosition">Position offset. If we want to do a cast not at the capsule's current position.</param>
		/// <returns></returns>
		private bool CapsuleSmallCast(Vector3 direction, float distance,
									  out RaycastHit smallRadiusHitInfo,
		                              Vector3 offsetPosition)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. CapsuleCast fails 
			// when moving almost parallel to an obstacle for small distances).
			float extraDistance = characterCapsule.scaledRadius;

			if (UnityEngine.Physics.CapsuleCast(characterCapsule.GetTopSphereWorldPosition() + offsetPosition,
			                                    characterCapsule.GetBottomSphereWorldPosition() + offsetPosition,
			                                    characterCapsule.scaledRadius,
			                                    direction,
			                                    out smallRadiusHitInfo,
			                                    distance + extraDistance,
			                                    characterCapsule.GetCollisionLayerMask()))
			{
				return smallRadiusHitInfo.distance <= distance;
			}

			return false;
		}

		/// <summary>
		/// Do a capsule cast, including the skin width.
		/// </summary>
		/// <param name="direction">Direction to cast.</param>
		/// <param name="distance">Distance to cast.</param>
		/// <param name="bigRadiusHitInfo">Hit info.</param>
		/// <param name="offsetPosition">Position offset. If we want to do a cast not at the capsule's current position.</param>
		/// <returns></returns>
		private bool CapsuleBigCast(Vector3 direction, float distance,
									out RaycastHit bigRadiusHitInfo,
		                            Vector3 offsetPosition)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. CapsuleCast fails 
			// when moving almost parallel to an obstacle for small distances).
			float extraDistance = characterCapsule.scaledRadius + characterCapsule.GetSkinWidth();

			if (UnityEngine.Physics.CapsuleCast(characterCapsule.GetTopSphereWorldPosition() + offsetPosition,
			                                    characterCapsule.GetBottomSphereWorldPosition() + offsetPosition,
			                                    characterCapsule.scaledRadius + characterCapsule.GetSkinWidth(),
			                                    direction,
			                                    out bigRadiusHitInfo,
			                                    distance + extraDistance,
			                                    characterCapsule.GetCollisionLayerMask()))
			{
				return bigRadiusHitInfo.distance <= distance;
			}

			return false;
		}

		/// <summary>
		/// Move away from an obstacle.
		/// </summary>
		/// <param name="moveVector">The movement vector.</param>
		/// <param name="hitInfo">Hit info of the collision.</param>
		/// <param name="direction">Direction of the cast.</param>
		/// <param name="distance">Distance of the cast.</param>
		/// <param name="tryGrounding">Try grounding the player?</param>
		/// <param name="tryStepOver">Try to step over obstacles?</param>
		/// <param name="hitInfoIsSmallRadius">If the hitInfo for the capsule's small radius?</param>
		/// <param name="debugInfo">Debug info. Remove when testing is done.</param>
		private void MoveAwayFromObstacle(ref Vector3 moveVector, ref RaycastHit hitInfo,
		                                  Vector3 direction, float distance,
										  bool tryGrounding, 
		                                  bool tryStepOver,
		                                  bool hitInfoIsSmallRadius,
		                                  string debugInfo)
		{
			// IMPORTANT: This method must set moveVector, unless we are certain the character needs to continue moving
			// forward into/over the obstacle.
			
			float hitDistance = Mathf.Max(hitInfo.distance - k_CollisionOffset, 0.0f);

			// Move to the collision point
			MovePosition(direction * hitDistance, direction, hitInfo, "move to collision point");

			float skinPenetrationDistance;
			Vector3 skinPenetrationVector;
			float remainingDistance = Mathf.Max(distance - hitDistance, 0.0f);

			GetPenetrationInfo(out skinPenetrationDistance, out skinPenetrationVector);

			// Push away from the obstacle
			MovePosition(skinPenetrationVector * skinPenetrationDistance, null, null,
			             string.Format("push away from obstacle (d: {0}     rd: {1})",
			                           distance,
			                           remainingDistance));
			
			bool slide = true;
			bool preserveMoveVector = false;
			bool slopeIsSteep = false;
			if (tryGrounding)
			{
				// No further movement when grounding the character
				slide = false;
			}
			else if (Math.Abs(moveVector.x) > Mathf.Epsilon ||
			         Math.Abs(moveVector.z) > Mathf.Epsilon)
			{
				// Test if character is trying to walk up a steep slope
				float angle = Vector3.Angle(Vector3.up, hitInfo.normal);
				slopeIsSteep = angle > characterCapsule.GetSlopeLimit();
			}
			
			if (tryStepOver &&
			    tryGrounding == false &&
			    (stepInfo.isStepping || slopeIsSteep))
			{
				if (stepInfo.isStepping == false)
				{
					if (TryToStartStepOver(moveVector, remainingDistance, Vector3.Angle(moveVector, -hitInfo.normal)))
					{
						slide = false;
					}
				}
				else if (UpdateStepOver(moveVector) == false)
				{
					StopStepOver(true);
				}
				else
				{
					// Bussy stepping over
					slide = false;
					preserveMoveVector = true;
				}
			}

			// Set moveVector
			if (slide)
			{
				// Vector to slide along the obstacle
				Vector3 project = Vector3.ProjectOnPlane(direction, hitInfo.normal);
				//project.Normalize();

				if (slopeIsSteep &&
				    project.y > 0.0f)
				{
					// Do not move up the slope
					project.y = 0.0f;
				}
				
				project.Normalize();
			
				// Slide along the obstacle
				moveVector = project * remainingDistance;
			}
			else if (preserveMoveVector == false)
			{
				// Stop current move loop itteration
				moveVector = Vector3.zero;
			}
		}

		/// <summary>
		/// Get direction and distance to move out of the obstacle.
		/// </summary>
		/// <param name="getDistance">Get distance to move out of the obstacle.</param>
		/// <param name="getDirection">Get direction to move out of the obstacle.</param>
		/// <param name="includSkinWidth">Include the skin width in the test?</param>
		/// <param name="offsetPosition">Offset position, if we want to test somewhere other than the capsule's position.</param>
		private bool GetPenetrationInfo(out float getDistance, out Vector3 getDirection,
		                                bool includSkinWidth = true,
		                                Vector3? offsetPosition = null)
		{
			getDistance = 0.0f;
			getDirection = Vector3.zero;

			Collider[] colliders = new Collider[k_MaxOverlapColliders];
			Vector3 offset = offsetPosition != null
				? offsetPosition.Value
				: Vector3.zero;
			float skinWidth = includSkinWidth
				? characterCapsule.GetSkinWidth()
				: 0.0f;
			int overlapCount = UnityEngine.Physics.OverlapCapsuleNonAlloc(characterCapsule.GetTopSphereWorldPosition() + offset,
			                                                              characterCapsule.GetBottomSphereWorldPosition() + offset,
			                                                              characterCapsule.scaledRadius + skinWidth,
			                                                              colliders,
			                                                              characterCapsule.GetCollisionLayerMask());
			if (overlapCount <= 0 ||
			    colliders.Length <= 0)
			{
				return false;
			}

			bool result = false;
			for (int i = 0; i < overlapCount; i++)
			{
				Collider collider = colliders[i];
				if (collider == null)
				{
					break;
				}
				
				Vector3 direction;
				float distance;
				Transform colliderTransform = collider.transform;
				if (characterCapsule.ComputePenetration(offset, 
														collider, colliderTransform.position, colliderTransform.rotation, 
														out direction, out distance, includSkinWidth))
				{
					getDistance += distance + k_CollisionOffset;
					getDirection += direction;
					result = true;
				}
			}

			return result;
		}

		/// <summary>
		/// Move the capsule position.
		/// </summary>
		/// <param name="moveVector">Move vector.</param>
		/// <param name="collideDirection">Direction we encountered collision. Null if no collision.</param>
		/// <param name="hitInfo">Hit info of the collision. Null if no collision.</param>
		private void MovePosition(Vector3 moveVector, Vector3? collideDirection, RaycastHit? hitInfo,
		                          string debugInfo = null)
		{
			if (Math.Abs(moveVector.sqrMagnitude) > Mathf.Epsilon)
			{
				capsuleTransform.position += moveVector;
			}

			#if UNITY_EDITOR
			if (debugCurrentPosition == null)
			{
				debugCurrentPosition = capsuleTransform.position;
			}
			else if (Math.Abs(moveVector.sqrMagnitude) > Mathf.Epsilon)
			{
				debugCurrentPosition += moveVector;
			}
			if (debugCurrentPosition != null &&
			    debugCurrentPositionErrorCount < 5 &&	// Only show error 5 times
			    (Math.Abs(debugCurrentPosition.Value.x - capsuleTransform.position.x) > Mathf.Epsilon ||
			     Math.Abs(debugCurrentPosition.Value.y - capsuleTransform.position.y) > Mathf.Epsilon ||
			     Math.Abs(debugCurrentPosition.Value.z - capsuleTransform.position.z) > Mathf.Epsilon))
			{
				Debug.LogError(string.Format(
					               "The character capsule's position was changed by something other than Move. " +
					               "[position: ({0}, {1}, {2})     should be: ({3}, {4}, {5})]",
					               capsuleTransform.position.x, capsuleTransform.position.y, capsuleTransform.position.z,
					               debugCurrentPosition.Value.x, debugCurrentPosition.Value.y, debugCurrentPosition.Value.z));
				debugCurrentPositionErrorCount++;
				debugCurrentPosition = capsuleTransform.position;
			}
			#endif

			if (collideDirection != null &&
			    hitInfo != null)
			{
				hitCount++;
				UpdateCollisionFlags(collideDirection.Value, hitInfo.Value);
			}
		}

		/// <summary>
		/// Update the collision flags.
		/// </summary>
		/// <param name="direction">The direction moved.</param>
		/// <param name="hitInfo">The hit info of the collision.</param>
		private void UpdateCollisionFlags(Vector3 direction, RaycastHit hitInfo)
		{
			if (Math.Abs(direction.x) > Mathf.Epsilon ||
			    Math.Abs(direction.z) > Mathf.Epsilon)
			{
				collisionFlags |= CollisionFlags.Sides;
			}

			if (direction.y > 0.0)
			{
				collisionFlags |= CollisionFlags.CollidedAbove;
			}
			else if (direction.y < 0.0)
			{
				collisionFlags |= CollisionFlags.CollidedBelow;
			}
		}
	}
}