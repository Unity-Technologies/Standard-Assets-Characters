//#define DEBUG_CHARACTER_CAPSULE_MOVER
using System;
using System.Collections.Generic;
using UnityEngine;

#if DEBUG_CHARACTER_CAPSULE_MOVER
using StandardAssets.GizmosHelpers;
#endif

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
		
		// Start testing if character is stuck if character collided this number of times during the movement loop
		private const int k_HitCountForStuck = 6;
		
		// Assume character is stuck if the position is the same for longer than this number of loop itterations
		private const int k_MaxStuckCount = 1;

		/// <inheritdoc />
		public event Action<bool> onGroundedChanged;

		/// <inheritdoc />
		public event Action<Vector3> onVelocityChanged;

		/// <inheritdoc />
		public event Action<CollisionFlags> onCollisionFlagsChanged;

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

		#region DEBUG FIELDS REMOVE WHEN DONE TESTING
		#if DEBUG_CHARACTER_CAPSULE_MOVER
		private Vector3? debugTopSphereWorldPosition = null;
		private Vector3? debugBottomSphereWorldPosition = null;
		private static int debugItteration;
		private static int debugTestCollisionCount;
		private static int debugMaxMoveItterationsCount;
		private Vector3? debugCurrentPosition;
		private bool debugToggle;	// Toggle special debug output via the F1 key
		#endif
		#endregion
		
		/// <inheritdoc />
		public CollisionFlags Move(Vector3 moveVector)
		{
			float distance = moveVector.sqrMagnitude;
			if (distance <= Mathf.Epsilon ||
			    distance < characterCapsule.GetMinMoveDistance())
			{
				return CollisionFlags.None;
			}

			bool wasGrounded = isGrounded;
			Vector3 oldPosition = capsuleTransform.position;
			Vector3 oldVelocity = velocity;
			CollisionFlags oldCollisionFlags = collisionFlags;
			Vector3 moveVectorNoY = new Vector3(moveVector.x, 0.0f, moveVector.z);
			bool tryToStickToGround = (wasGrounded &&
			                           moveVector.y <= 0.0f &&
			                           Math.Abs(moveVectorNoY.sqrMagnitude) > Mathf.Epsilon);
			
			collisionFlags = CollisionFlags.None;
			hitCount = 0;

			MoveLoop(moveVector, tryToStickToGround);

			isGrounded = GetGrounded(wasGrounded, oldPosition);
			velocity = capsuleTransform.position - oldPosition;

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

			#if DEBUG_CHARACTER_CAPSULE_MOVER
			DebugTestCollision(null, "END OF MOVE");
			if (debugToggle)
			{
				Debug.Log(string.Format("moveVector: {0}     y: {1}",
				                        DebugVectorToString(moveVector),
				                        moveVector.y));
			}
			#endif

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

			// Break the movement up into horizontal and vertical
			Vector3 horizontal = new Vector3(moveVector.x, 0.0f, moveVector.z);
			Vector3 vertical = new Vector3(0.0f, moveVector.y, 0.0f);
			
			if (vertical.y > 0.0f)
			{
				// Move up then horizontal
				remainingMoveVector = vertical;
				moveVectors.Add(horizontal);
				// TODO: Handle cases where the vector is large
			}
			else if (vertical.y < 0.0f)
			{
				// Move horizontal then down
				remainingMoveVector = horizontal;
				moveVectors.Add(vertical);
				// TODO: Handle cases where the vector is large
			}
			else
			{
				// Move horizontal only
				remainingMoveVector = horizontal;
			}

			int moveVectorIndex = 0;
			bool didTryToStickToGround = false;
			stuckCount = 0;
			stuckPosition = null;

			for (int i = 0; i < k_MaxMoveItterations; i++)
			{
				#if DEBUG_CHARACTER_CAPSULE_MOVER
				debugItteration = i;
				#endif
				
				bool collided = MoveMajorStep(ref remainingMoveVector, i == 0, didTryToStickToGround);

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
					#if DEBUG_CHARACTER_CAPSULE_MOVER
					if (debugMaxMoveItterationsCount > 5)
					{
						break;
					}
					debugMaxMoveItterationsCount++;
					#endif
					
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
		/// A single movement major step. Returns true when there is collision.
		/// </summary>
		/// <param name="moveVector">The move vector.</param>
		/// <param name="tryStepOver">Try to step over obstacles?</param>
		/// <param name="tryGrounding">Try grounding the player?</param>
		/// <returns>True when there is collision.</returns>
		private bool MoveMajorStep(ref Vector3 moveVector, bool tryStepOver, bool tryGrounding)
		{
			/*if (tryStepOver)
			{
				if (TryStepOver(moveVector))
				{
					moveVector = Vector3.zero;
					return false;
				}
			}*/
			
			return MoveMinorStep(ref moveVector, tryGrounding);
		}

		/// <summary>
		/// Try to step over obstacles.
		/// Sets up the movement vectors to step over an obstacle.
		/// </summary>
		/// <param name="moveVector"></param>
		/// <returns></returns>
		private bool TryStepOver(Vector3 moveVector)
		{
			float stepOffset = characterCapsule.GetStepOffset();
			if (stepOffset > 0.0f &&
			    Math.Abs(moveVector.y) < Mathf.Epsilon &&
			    (Math.Abs(moveVector.x) > Mathf.Epsilon ||
			     Math.Abs(moveVector.z) > Mathf.Epsilon))
			{
				// Up
				moveVectors.Add(Vector3.up * stepOffset);
				
				// Horizontal
				moveVectors.Add(new Vector3(moveVector.x, 0.0f, moveVector.z));
				
				// Down
				moveVectors.Add(Vector3.down * stepOffset);
				
				return true;
			}

			return false;
		}
		
		/// <summary>
		/// A single movement minor step. Returns true when there is collision.
		/// </summary>
		/// <param name="moveVector">The move vector.</param>
		/// <param name="tryGrounding">Try grounding the player?</param>
		/// <returns>True when there is collision.</returns>
		private bool MoveMinorStep(ref Vector3 moveVector, bool tryGrounding)
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
				MovePosition(moveVector, null, null);
				moveVector = Vector3.zero;

				#if DEBUG_CHARACTER_CAPSULE_MOVER
				DebugTestCollision(null, "move minor 1 (should never happen!)");
				#endif

				return false;
			}

			// Did the big radius not hit an obstacle?
			if (bigRadiusHit == false)
			{
				// The small radius hit an obstacle, so character is inside an obstacle
				MoveAwayFromObstacle(ref moveVector, ref smallRadiusHitInfo,
				                     direction, distance, 
									 tryGrounding,
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
				                     "UseSmallCollision");
				return true;
			}

			MoveAwayFromObstacle(ref moveVector, ref bigRadiusHitInfo,
								 direction, distance,
								 tryGrounding,
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
			// Exlclude the skin width in the test
			smallRadiusHit = UnityEngine.Physics.CapsuleCast(characterCapsule.GetTopSphereWorldPosition(),
															 characterCapsule.GetBottomSphereWorldPosition(),
			                                                 characterCapsule.scaledRadius,
			                                                 direction,
			                                                 out smallRadiusHitInfo,
			                                                 distance,
			                                                 characterCapsule.GetCollisionLayerMask());
			
			// Include the skin width in the test
			bigRadiusHit = UnityEngine.Physics.CapsuleCast(characterCapsule.GetTopSphereWorldPosition(),
														   characterCapsule.GetBottomSphereWorldPosition(),
			                                               characterCapsule.scaledRadius + characterCapsule.GetSkinWidth(),
			                                               direction,
			                                               out bigRadiusHitInfo,
			                                               distance,
			                                               characterCapsule.GetCollisionLayerMask());

			return smallRadiusHit ||
			       bigRadiusHit;
		}

		/// <summary>
		/// Move away from an obstacle.
		/// </summary>
		/// <param name="moveVector">The movement vector.</param>
		/// <param name="hitInfo">Hit info of the collision.</param>
		/// <param name="direction">Direction of the cast.</param>
		/// <param name="distance">Distance of the cast.</param>
		/// <param name="tryGrounding">Try grounding the player?</param>
		private void MoveAwayFromObstacle(ref Vector3 moveVector, ref RaycastHit hitInfo,
		                                  Vector3 direction, float distance,
										  bool tryGrounding,
		                                  string debugInfo)
		{
			float hitDistance = Mathf.Max(hitInfo.distance - k_CollisionOffset, 0.0f);

			// Move to the collision point
			MovePosition(direction * hitDistance, direction, hitInfo);

			float skinPenetrationDistance;
			Vector3 skinPenetrationVector;
			float remainingDistance = Mathf.Max(distance - hitDistance, 0.0f);

			GetPenetrationInfo(out skinPenetrationDistance, out skinPenetrationVector, ref hitInfo);

			#if DEBUG_CHARACTER_CAPSULE_MOVER
			DebugTestCollision(hitInfo.collider, 
							   string.Format("move away 1  (hd: {0}     rd: {1}     pd: {2}     pv: {3}     dbg: {4})",
											 hitDistance,
											 remainingDistance,
											 skinPenetrationDistance,
											 DebugVectorToString(skinPenetrationVector),
											 debugInfo));
			
			debugTopSphereWorldPosition = characterCapsule.GetTopSphereWorldPosition();
			debugBottomSphereWorldPosition = characterCapsule.GetBottomSphereWorldPosition();
			#endif

			// Push away from the obstacle
			MovePosition(skinPenetrationVector * skinPenetrationDistance, null, null);

			#if DEBUG_CHARACTER_CAPSULE_MOVER
			bool debugShow = DebugTestCollision(hitInfo.collider, "move away 2");
			#endif

			bool slide = true;
			bool slopeIsSteep = false;
			if (tryGrounding)
			{
				// No further movement when grounding the character
				moveVector = Vector3.zero;
				slide = false;
			}
			else if (Math.Abs(moveVector.x) > Mathf.Epsilon ||
			         Math.Abs(moveVector.z) > Mathf.Epsilon)
			{
				// Test if character is trying to walk up a steep slope
				float angle = Vector3.Angle(Vector3.up, hitInfo.normal);
				slopeIsSteep = angle > characterCapsule.GetSlopeLimit();
			}

			if (slide)
			{
				// Vector to slide along the obstacle
				Vector3 project = Vector3.ProjectOnPlane(direction, hitInfo.normal);
				project.Normalize();

				if (slopeIsSteep &&
				    project.y > 0.0f)
				{
					// Do not move up the slope
					project.y = 0.0f;
				}
			
				// Slide along the obstacle
				moveVector = project * remainingDistance;
			}

			#if DEBUG_CHARACTER_CAPSULE_MOVER
			if (debugShow)
			{
				// Ray backwards from hit point
				Debug.DrawRay(hitInfo.point - direction, direction, Color.red, 10.0f);

				// Ray forwards from capsule centre position
				Debug.DrawRay(capsuleTransform.position + characterCapsule.scaledCenter, direction, Color.green, 10.0f);

				// Original movement vector
				Debug.DrawRay(capsuleTransform.position + characterCapsule.scaledCenter, direction * distance, Color.yellow, 10.0f);

				// Penetration info
				Debug.DrawRay(hitInfo.point, skinPenetrationVector * skinPenetrationDistance, Color.yellow, 10.0f);

				//Debug.Break();
			}
			#endif
		}

		/// <summary>
		/// Get direction and distance to move out of the obstacle.
		/// </summary>
		/// <param name="getDistance">Get distance to move out of the obstacle.</param>
		/// <param name="getDirection">Get direction to move out of the obstacle.</param>
		/// <param name="hitInfo">Hit info of the obstacle that was hit.</param>
		private bool GetPenetrationInfo(out float getDistance, out Vector3 getDirection, ref RaycastHit hitInfo)
		{
			getDistance = 0.0f;
			getDirection = Vector3.zero;

			Collider[] colliders = new Collider[k_MaxOverlapColliders];
			UnityEngine.Physics.OverlapCapsuleNonAlloc(characterCapsule.GetTopSphereWorldPosition(),
			                                           characterCapsule.GetBottomSphereWorldPosition(),
			                                           characterCapsule.scaledRadius + characterCapsule.GetSkinWidth(),
			                                           colliders,
			                                           characterCapsule.GetCollisionLayerMask());
			if (colliders.Length <= 0)
			{
				return false;
			}

			bool result = false;
			foreach(var collider in colliders)
			{
				if (collider == null)
				{
					break;
				}
				Vector3 direction;
				float distance;
				Transform colliderTransform = collider.transform;
				if (characterCapsule.ComputePenetration(Vector3.zero, 
														collider, colliderTransform.position, colliderTransform.rotation, 
														out direction, out distance, true))
				{
					getDistance += distance;
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
		private void MovePosition(Vector3 moveVector, Vector3? collideDirection, RaycastHit? hitInfo)
		{
			#if DEBUG_CHARACTER_CAPSULE_MOVER
			if (debugCurrentPosition == null)
			{
				debugCurrentPosition = capsuleTransform.position;
			}
			#endif

			if (Math.Abs(moveVector.sqrMagnitude) > Mathf.Epsilon)
			{
				capsuleTransform.position += moveVector;
			}

			#if DEBUG_CHARACTER_CAPSULE_MOVER
			if (Math.Abs(moveVector.sqrMagnitude) > Mathf.Epsilon)
			{
				debugCurrentPosition += moveVector;
			}
			if (debugCurrentPosition != null &&
			    (Math.Abs(debugCurrentPosition.Value.x - capsuleTransform.position.x) > Mathf.Epsilon ||
			     Math.Abs(debugCurrentPosition.Value.y - capsuleTransform.position.y) > Mathf.Epsilon ||
			     Math.Abs(debugCurrentPosition.Value.z - capsuleTransform.position.z) > Mathf.Epsilon))
			{
				Debug.LogError(string.Format(
					               "Positions are different! Something else changed the capsule's transform position! {0}     {1}",
					               DebugVectorToString(capsuleTransform.position),
					               DebugVectorToString(debugCurrentPosition.Value)));
			}
			DebugTestCollision(hitInfo != null ? hitInfo.Value.collider : null, 
							   string.Format("MOVE POSITION: {0}",
											 DebugVectorToString(moveVector)));
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

		#region DEBUG METHODS REMOVE WHEN DONE TESTING
		#if DEBUG_CHARACTER_CAPSULE_MOVER
		/// <inheritdoc />
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F1))
			{
				debugToggle = !debugToggle;
			}
		}

		/// <summary>
		/// Debugs the test collision.
		/// </summary>
		/// <param name="testCollider">Test specifically for this collider. If null then will test for all colliders.</param>
		private bool DebugTestCollision(Collider testCollider = null, string debugInfo = null)
		{
			Collider[] colliders = UnityEngine.Physics.OverlapCapsule(characterCapsule.GetTopSphereWorldPosition(),
																	  characterCapsule.GetBottomSphereWorldPosition(),
																	  characterCapsule.scaledRadius + characterCapsule.GetSkinWidth(),
																	  characterCapsule.GetCollisionLayerMask());
			if (colliders != null &&
				colliders.Length > 0)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				List<Collider> foundColliders = new List<Collider>(colliders.Length);
				foreach(var collider in colliders)
				{
					/*if (testCollider != null &&
						collider != testCollider)
					{
						continue;
					}*/

					foundColliders.Add(collider);
				}

				if (foundColliders.Count > 0)
				{
					debugTestCollisionCount ++;
					if (debugTestCollisionCount <= 10 ||
					    debugToggle)
					{
						sb.Append(string.Format("inside colliders: {0}     itt: {1}     hCount: {2}     (x{3})     ", 
							debugInfo != null ? debugInfo : "",
							debugItteration,
							hitCount,
							foundColliders.Count));
						bool foundTestCollider = false;
						foreach(var collider in foundColliders)
						{
							if (collider == testCollider)
							{
								foundTestCollider = true;
								sb.Append(string.Format("**{0}**", collider.name));
							}
							else
							{
								sb.Append(collider.name);
							}
							sb.Append("     ");
						}

						if (testCollider != null &&
							foundTestCollider == false)
						{
							sb.Append(string.Format("(did not find test collider: {0})", testCollider.name));
						}

						Debug.LogWarning(sb.ToString());
					}

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// DEBUG: Some test calculations.
		/// </summary>
		private void DebugTestCalcs()
		{
			float distance = 2.5f;
			float hitDistance = 1.0f;
			float remainingDistance = distance - hitDistance;
			float penetrationDistance = 0.3f;
			Vector3 hitNormal = Vector3.back;
			Vector3 direction = Vector3.RotateTowards(Vector3.forward, Vector3.right, Mathf.Deg2Rad * 45.0f, 1.0f);

			if (remainingDistance < penetrationDistance)
			{
				// Increase distance to move away from obstacle
				remainingDistance = penetrationDistance;
			}

			// Vector to slide along the obstacle
			Vector3 project = Vector3.ProjectOnPlane(direction, hitNormal);
			project.Normalize();
			// Normal to rotate the vector
			Vector3 normal = Vector3.Cross(project, -direction);
			float angle = Mathf.Asin(penetrationDistance / remainingDistance);
			// Rotate the projected vector away from the obstacle
			Vector3 rotatedVector = Quaternion.AngleAxis(Mathf.Rad2Deg * angle, normal) * 
				(project * remainingDistance);

			Debug.Log(string.Format("dir: {0}     proj: {1}     nor: {2}     rd: {3}     pd: {4}     rvec: {5}     ang: {6}",
				DebugVectorToString(direction),
				DebugVectorToString(project),
				DebugVectorToString(normal),
				remainingDistance,
				penetrationDistance,
				DebugVectorToString(rotatedVector),
				Mathf.Rad2Deg * angle));

			//distance = 2.5, hitDistance = 1.0, penetrationDistance = 0.3
			//dir: (0.7071068, 0, 0.7071068) [0.9999999]     proj: (1, 0, 0) [1]     nor: (0, 0.7071068, 0) [0.7071068]     rd: 1.5     pd: 0.3     rvec: (1.469694, 0, -0.3) [1.5]     ang: 11.53696

			//distance = 2.5, hitDistance = 1.0, penetrationDistance = 3.3
			//dir: (0.7071068, 0, 0.7071068) [0.9999999]     proj: (1, 0, 0) [1]     nor: (0, 0.7071068, 0) [0.7071068]     rd: 1.5     pd: 3.3     rvec: (NaN, NaN, NaN) [NaN]     ang: NaN

			//distance = 2.5, hitDistance = 1.0, penetrationDistance = 3.3
			//dir: (0.7071068, 0, 0.7071068) [0.9999999]     proj: (1, 0, 0) [1]     nor: (0, 0.7071068, 0) [0.7071068]     rd: 3.3     pd: 3.3     rvec: (1.966953E-07, 0, -3.3) [3.3]     ang: 90

		}

		/// <summary>
		/// DEBUG: Vector3 ToString
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		private string DebugVectorToString(Vector3 vector)
		{
			return string.Format("({0}, {1}, {2}) [{3}]",
				vector.x,
				vector.y,
				vector.z,
				vector.magnitude);
		}

		/// <inheritdoc />
		private void OnDrawGizmosSelected()
		{
			if (characterCapsule == null ||
				debugTopSphereWorldPosition == null ||
				debugBottomSphereWorldPosition == null)
			{
				return;
			}

			GizmosHelper.DrawCapsule(debugTopSphereWorldPosition.Value, debugBottomSphereWorldPosition.Value, 
				characterCapsule.scaledRadius, Color.cyan);

			GizmosHelper.DrawCapsule(debugTopSphereWorldPosition.Value, debugBottomSphereWorldPosition.Value, 
				characterCapsule.scaledRadius + characterCapsule.GetSkinWidth(), Color.magenta);
		}
		#endif
		#endregion
	}
}