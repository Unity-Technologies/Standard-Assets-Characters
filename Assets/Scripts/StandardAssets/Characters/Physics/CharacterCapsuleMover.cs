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
		/// Max move vector length when splitting it up into horizontal and vertial components.
		/// </summary>
		private const float k_MaxMoveVectorLength = 0.5f;

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
		/// Only step over obstacles if the angle between the move vector and the obstacle's inverted normal is less than this.
		/// </summary>
		private const float k_MaxStepOverHitAngle = 90.0f;

		/// <summary>
		/// Minimum distance to move. This minimizes small penetrations and inaccurate casts (e.g. into the floor)
		/// </summary>
		private const float k_MinMoveDistance = 0.0001f;
		
		/// <summary>
		/// Minimum sqr distance to move. This minimizes small penetrations (e.g. into the floor)
		/// </summary>
		private const float k_MinMoveSqrDistance = k_MinMoveDistance * k_MinMoveDistance;

		/// <summary>
		/// Minimum step offset height to move (if character has a step offset).
		/// </summary>
		private const float k_MinStepOffsetHeight = k_MinMoveDistance;

		/// <inheritdoc />
		public event Action<bool> onGroundedChanged;

		/// <inheritdoc />
		public event Action<Vector3> onVelocityChanged;

		/// <inheritdoc />
		public event Action<CollisionFlags> onCollisionFlagsChanged;

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
		private List<CharacterCapsuleMoverVector> moveVectors = new List<CharacterCapsuleMoverVector>();
		
		/// <summary>
		/// Next index in the moveVectors list.
		/// </summary>
		private int nextMoveVectorIndex;

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
		private CharacterCapsuleMoverStepInfo stepInfo = new CharacterCapsuleMoverStepInfo();

		/// <summary>
		/// The collision info when hitting colliders.
		/// </summary>
		private Dictionary<Collider, List<CharacterCapsuleMoverCollisionInfo>> collisionInfoDictionary = new Dictionary<Collider, List<CharacterCapsuleMoverCollisionInfo>>();
		
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
		public bool isGrounded { get; private set; }

		/// <inheritdoc />
		public CollisionFlags collisionFlags { get; private set; }

		/// <inheritdoc />
		public Vector3 velocity { get; private set; }

		/// <inheritdoc />
		public bool checkSmallObstaclesWhenStepOver { get; set; }

		/// <inheritdoc />
		public CollisionFlags Move(Vector3 moveVector)
		{
			return MoveInternal(moveVector, true);
		}

		/// <inheritdoc />
		public bool SimpleMove(Vector3 speed)
		{
			Vector3 moveVector = new Vector3(speed.x, speed.y + UnityEngine.Physics.gravity.y, speed.z);

			// Reminder: Time.deltaTime returns the fixed delta time when called from inside FixedUpdate.
			MoveInternal(moveVector * Time.deltaTime, false);

			return isGrounded;
		}

		/// <inheritdoc />
		public void SetPosition(Vector3 position, bool updateGrounded)
		{
			capsuleTransform.position = position;

			#if UNITY_EDITOR
			debugCurrentPosition = capsuleTransform.position;
			#endif

			if (updateGrounded)
			{
				UpdateGrounded(CollisionFlags.None);
			}
		}

		/// <inheritdoc />
		private void Awake()
		{
			characterCapsule = (CharacterCapsule)GetComponent(typeof(CharacterCapsule));
			capsuleTransform = characterCapsule.transform;
		}

		/// <summary>
		/// Moves the characters.
		/// </summary>
		/// <param name="moveVector">Move vector.</param>
		/// <param name="slideWhenMovingDown">Slide against obstacles when moving down? (e.g. we don't want to slide when applying gravity while the charcter is grounded)</param>
		/// <returns>CollisionFlags is the summary of collisions that occurred during the move.</returns>
		private CollisionFlags MoveInternal(Vector3 moveVector, bool slideWhenMovingDown)
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
			Vector3 moveVectorNoY = new Vector3(moveVector.x, 0.0f, moveVector.z);
			bool tryToStickToGround = (wasGrounded &&
				moveVector.y <= 0.0f &&
				Math.Abs(moveVectorNoY.sqrMagnitude) > Mathf.Epsilon);

			collisionFlags = CollisionFlags.None;
			hitCount = 0;

			// Do the move loop
			MoveLoop(moveVector, tryToStickToGround, slideWhenMovingDown);

			UpdateGrounded(collisionFlags);
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

			return collisionFlags;
		}

		/// <summary>
		/// Determine if the character is grounded.
		/// </summary>
		/// <param name="movedCollisionFlags">Moved collision flags of the current move. Set to None if not moving.</param>
		private void UpdateGrounded(CollisionFlags movedCollisionFlags)
		{
			bool wasGrounded = isGrounded;
			
			if ((movedCollisionFlags & CollisionFlags.CollidedBelow) != 0)
			{
				isGrounded = true;
			}
			else
			{
				RaycastHit hitinfo;
				isGrounded = SmallSphereCast(Vector3.down, k_GroundedTestDistance + characterCapsule.GetSkinWidth(), out hitinfo,
				                             Vector3.zero, true);
			}
			
			if (onGroundedChanged != null &&
			    wasGrounded != isGrounded)
			{
				onGroundedChanged(isGrounded);
			}
		}

		/// <summary>
		/// Movement loop. Keep moving until completely blocked by obstacles, or we reached the desired position/distance.
		/// </summary>
		/// <param name="moveVector">The move vector.</param>
		/// <param name="tryToStickToGround">Try to stick to the ground?</param>
		/// <param name="slideWhenMovingDown">Slide against obstacles when moving down? (e.g. we don't want to slide when applying gravity while the charcter is grounded)</param>
		private void MoveLoop(Vector3 moveVector, bool tryToStickToGround, bool slideWhenMovingDown)
		{
			moveVectors.Clear();
			nextMoveVectorIndex = 0;

			CharacterCapsuleMoverVector remainingMoveVector;
			int tryStepOverIndex = -1;
			
			if (stepInfo.isStepping &&
			    stepInfo.OnNewMoveVector(moveVector) == false)
			{
				StopStepOver(true, "MoveLoop start: move vector changed, or too much time elapsed.");
			}

			// Split the move vector into horizontal and vertical components.
			InitMoveVectors(moveVector, out tryStepOverIndex, slideWhenMovingDown);
			remainingMoveVector = moveVectors[nextMoveVectorIndex];
			nextMoveVectorIndex++;

			bool didTryToStickToGround = false;
			stuckCount = 0;
			stuckPosition = null;
			
			#if DISABLE_STEP_OFFSET
			tryStepOverIndex = -1;
			#endif

			for (int i = 0; i < k_MaxMoveItterations; i++)
			{	
				bool collided = MoveMajorStep(ref remainingMoveVector.moveVector, 
				                              remainingMoveVector.canSlide,
				                              didTryToStickToGround, 
				                              i == tryStepOverIndex || stepInfo.isStepping);

				if (IsStuck())
				{
					remainingMoveVector = new CharacterCapsuleMoverVector(Vector3.zero);
				}
				
				// Collided or vector used up (i.e. vector is zero)?
				if (!collided || 
				    remainingMoveVector.moveVector.sqrMagnitude <= Mathf.Epsilon)
				{
					// Are there remaining movement vectors?
					if (nextMoveVectorIndex < moveVectors.Count)
					{
						remainingMoveVector = moveVectors[nextMoveVectorIndex];
						nextMoveVectorIndex++;
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
												 remainingMoveVector.moveVector.x, remainingMoveVector.moveVector.y, remainingMoveVector.moveVector.z,
												 moveVector.x, moveVector.y, moveVector.z,
												 hitCount));
				}
				#endif
			}
		}
		
		/// <summary>
		/// A single movement major step. Returns true when there is collision.
		/// </summary>
		/// <param name="moveVector">The move vector.</param>
		/// <param name="canSlide">Can slide against obsyacles?</param>
		/// <param name="tryGrounding">Try grounding the player?</param>
		/// <param name="tryStepOver">Try to step over obstacles?</param>
		/// <returns>True when there is collision.</returns>
		private bool MoveMajorStep(ref Vector3 moveVector, bool canSlide, bool tryGrounding, bool tryStepOver)
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
				
				// Stepping and the final movement vector?
				if (stepInfo.isStepping &&
				    IsFinalMoveVector())
				{
					// Do a last update, in case we need to move up.
					UpdateStepOver(moveVector, 0.0f, canSlide);
					
					StopStepOver(true, string.Format("No collision.     heightRem: {0}",
						                                 stepInfo.GetRemainingHeight(capsuleTransform.position)));
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
				                     canSlide,
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
				                     canSlide,
									 tryGrounding,
				                     tryStepOver,
				                     true,
				                     "UseSmallCollision");
				return true;
			}

			MoveAwayFromObstacle(ref moveVector, ref bigRadiusHitInfo,
								 direction, distance,
			                     canSlide,
								 tryGrounding,
			                     tryStepOver,
			                     false,
								 "UseBigCollision");
			
			return true;
		}

		/// <summary>
		/// Initialize the moveVectors list.
		/// </summary>
		/// <param name="moveVector">The move vector.</param>
		/// <param name="getTryStepOverIndex">Get the index in the list when the character must try to step over obstacles.</param>
		/// <param name="slideWhenMovingDown">Slide against obstacles when moving down? (e.g. we don't want to slide when applying gravity while the charcter is grounded)</param>
		private void InitMoveVectors(Vector3 moveVector, out int getTryStepOverIndex, bool slideWhenMovingDown)
		{
			getTryStepOverIndex = -1;

			// Split the move vector into horizontal and vertical components.
			float length = moveVector.magnitude;
			if (length <= k_MaxMoveVectorLength ||
				moveVector.y == 0.0f ||
				(moveVector.x == 0.0f && moveVector.z == 0.0f))
			{
				SplitMoveVector(moveVector, out getTryStepOverIndex, slideWhenMovingDown);
				return;
			}

			Vector3 direction = moveVector.normalized;
			int len = (int)((float)length / k_MaxMoveVectorLength) + 1;
			for (int i = 0; i < len; i++)
			{
				float distance = Mathf.Min(length, k_MaxMoveVectorLength);
				if (distance <= 0.0f)
				{
					break;
				}
				int tempTryStepOverIndex;
				SplitMoveVector(direction * distance, out tempTryStepOverIndex, slideWhenMovingDown);
				if (getTryStepOverIndex == -1)
				{
					getTryStepOverIndex = tempTryStepOverIndex;
				}
				length -= k_MaxMoveVectorLength;
			}

			#if UNITY_EDITOR
			if (len >= k_MaxMoveItterations)
			{
				Debug.LogWarning(string.Format("The moveVector is large ({0}). Try using a smaller moveVector.",
											   moveVector.magnitude));
			}
			#endif
		}

		/// <summary>
		/// Split the move vector into horizontal and vertical components. The results are added to the moveVectors list.
		/// </summary>
		/// <param name="moveVector">The move vector.</param>
		/// <param name="getTryStepOverIndex">Get the index in the list when the character must try to step over obstacles.</param>
		/// <param name="slideWhenMovingDown">Slide against obstacles when moving down? (e.g. we don't want to slide when applying gravity while the charcter is grounded)</param>
		private void SplitMoveVector(Vector3 moveVector, out int getTryStepOverIndex, bool slideWhenMovingDown)
		{
			Vector3 horizontal = new Vector3(moveVector.x, 0.0f, moveVector.z);
			Vector3 vertical = new Vector3(0.0f, moveVector.y, 0.0f);

			getTryStepOverIndex = -1;

			if (vertical.y > 0.0f)
			{
				if (Math.Abs(horizontal.x) > Mathf.Epsilon ||
					Math.Abs(horizontal.z) > Mathf.Epsilon)
				{
					// Move up then horizontal
					AddMoveVector(vertical);
					getTryStepOverIndex = AddMoveVector(horizontal);
				}
				else
				{
					// Move up
					AddMoveVector(vertical);
				}
			}
			else if (vertical.y < 0.0f)
			{
				if (Math.Abs(horizontal.x) > Mathf.Epsilon ||
					Math.Abs(horizontal.z) > Mathf.Epsilon)
				{
					// Move horizontal then down
					getTryStepOverIndex = AddMoveVector(horizontal);
					AddMoveVector(vertical, slideWhenMovingDown);
				}
				else
				{
					// Move down
					AddMoveVector(vertical, slideWhenMovingDown);
				}
			}
			else
			{
				// Move horizontal
				getTryStepOverIndex = AddMoveVector(horizontal);
			}
		}

		/// <summary>
		/// Add the movement vector to the moveVectors list.
		/// </summary>
		/// <param name="vector">Move vector to add.</param>
		/// <param name="canSlide">Can the movement slide along obstacles?</param>
		private int AddMoveVector(Vector3 moveVector, bool canSlide = true)
		{
			moveVectors.Add(new CharacterCapsuleMoverVector(moveVector, canSlide));
			return moveVectors.Count - 1;
		}

		/// <summary>
		/// Insert the movement vector into the moveVectors list.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="vector">Move vector to add.</param>
		/// <param name="canSlide">Can the movement slide along obstacles?</param>
		/// <returns>The index where it was inserted.</returns>
		private int InsertMoveVector(int index, Vector3 moveVector, bool canSlide = true)
		{
			if (index < 0)
			{
				index = 0;
			}
			if (index >= moveVectors.Count)
			{
				moveVectors.Add(new CharacterCapsuleMoverVector(moveVector, canSlide));
				return moveVectors.Count - 1;
			}

			moveVectors.Insert(index, new CharacterCapsuleMoverVector(moveVector, canSlide));
			return index;
		}

		/// <summary>
		/// Is the move loop on the final move vector?
		/// </summary>
		/// <returns></returns>
		private bool IsFinalMoveVector()
		{
			return moveVectors.Count == 0 ||
			       nextMoveVectorIndex >= moveVectors.Count;
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
			else if (VectorSqrMagnitude(stuckPosition.Value, capsuleTransform.position) <= k_StuckSqrDistance)
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
		/// <param name="getCanSlide">Get if can slide against obstacles.</param>
		/// <returns></returns>
		private bool CanStickToGround(Vector3 moveVector, out CharacterCapsuleMoverVector getDownVector)
		{
			Vector3 moveVectorNoY = new Vector3(moveVector.x, 0.0f, moveVector.z);
			float downDistance = moveVectorNoY.magnitude;

			if (downDistance <= k_MaxStickToGroundDownDistance)
			{
				getDownVector = new CharacterCapsuleMoverVector(Vector3.down * downDistance, false);
				return true;
			}
			
			getDownVector = new CharacterCapsuleMoverVector(Vector3.zero);
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
		private bool StartStepOver(Vector3 moveVector, float remainingDistance, float angleToObstacle)
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
			
			if (checkSmallObstaclesWhenStepOver)
			{
				// TODO: Handle cases where small obstacles hit our side edges, but we don't want to try to step over them.
			}

			float moveVectorMagnitude = moveVector.magnitude;
			Vector3 moveVectorNoY = new Vector3(moveVector.x, 0.0f, moveVector.z).normalized;
			RaycastHit hitInfo;
			 
			// Only step up if there's an obstacle at the character's feet (e.g. do not step when only character's head collides)
			if (!SmallSphereCast(moveVector, moveVectorMagnitude, out hitInfo, Vector3.zero, true) && 
			    !BigSphereCast(moveVector, moveVectorMagnitude, out hitInfo, Vector3.zero, true))
			{	
				return false;
			}
			
			// We only step over obstacles if we can fully fit on it (i.e. the capsule's diameter)
			float diameter = characterCapsule.scaledRadius * 2.0f;
			Vector3 horizontal = moveVectorNoY * diameter;
			float horizontalSize = horizontal.magnitude;
			horizontal.Normalize();
			// Any obstacles above?
			float upDistance = Mathf.Max(stepOffset, k_MinStepOffsetHeight);
			Vector3 up = Vector3.up * upDistance;
			Ray ray = new Ray(characterCapsule.GetTopSphereWorldPosition(), Vector3.up);
			if (UnityEngine.Physics.SphereCast(ray,
			                                   characterCapsule.scaledRadius,
			                                   characterCapsule.GetSkinWidth() + upDistance,
			                                   characterCapsule.GetCollisionLayerMask()))
			{
				return false;
			}
			
			// Any obstacles ahead (after we moved up)?
			// First test big capsule then small capsule, in case big capsule already inside an obstacle.
			if (BigCapsuleCast(horizontal, horizontalSize, out hitInfo, up) ||
			    SmallCapsuleCast(horizontal, horizontalSize + characterCapsule.GetSkinWidth(), out hitInfo, up))
			{
				return false;
			}
			
			// Move Up
			int index = InsertMoveVector(nextMoveVectorIndex, up, false);
			
			// Move Horizontal
			horizontal = moveVectorNoY * remainingDistance;
			InsertMoveVector(index + 1, horizontal, false);
			
			// Start stepping over the obstacles
			stepInfo.OnStartStepOver(upDistance, moveVector, capsuleTransform.position);
			
			return true;
		}

		/// <summary>
		/// Update stepping over obstacles.
		/// </summary>
		/// <param name="moveVector">Movement vector.</param>
		/// <param name="remainingDistance">Remaining distance to move after moving up.</param>
		/// <param name="canSlide">Can the remaining distance slide along obstacles?</param>
		/// <returns></returns>
		private bool UpdateStepOver(Vector3 moveVector, float remainingDistance, bool canSlide)
		{
			float stepOffset = characterCapsule.GetStepOffset();
			if (stepOffset <= 0.0f)
			{
				return false;
			}

			if (Math.Abs(moveVector.x) < Mathf.Epsilon &&
			    Math.Abs(moveVector.z) < Mathf.Epsilon)
			{
				// Wait until we get a horizontal vector
				return true;
			}
			
			// Climb height remaining
			float heightRemaining = stepInfo.GetRemainingHeight(capsuleTransform.position);
			if (heightRemaining <= 0.0f)
			{
				return false;
			}
			
			// Any obstacles above?
			Vector3 up = Vector3.up * Mathf.Max(heightRemaining, k_MinStepOffsetHeight);
			Ray ray = new Ray(characterCapsule.GetTopSphereWorldPosition(), Vector3.up);
			if (UnityEngine.Physics.SphereCast(ray,
			                                   characterCapsule.scaledRadius,
			                                   characterCapsule.GetSkinWidth() + heightRemaining,
			                                   characterCapsule.GetCollisionLayerMask()))
			{
				// Stop stepping over
				return false;
			}
				
			// Move Up
			int index = InsertMoveVector(nextMoveVectorIndex, up, false);
			
			// Continue other movement after moving up
			if (remainingDistance > 0.0f)
			{
				InsertMoveVector(index + 1, moveVector.normalized * remainingDistance, false);
			}

			stepInfo.OnUpdate();
			
			return true;
		}

		/// <summary>
		/// Stop stepping over obstacles.
		/// </summary>
		/// <param name="fallDown">Should fall down to tough the ground?</param>
		/// <param name="debugInfo">Debug info.</param>
		private void StopStepOver(bool fallDown, string debugInfo)
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
				InsertMoveVector(nextMoveVectorIndex, down, false);
			}
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
			smallRadiusHit = SmallCapsuleCast(direction, distance, out smallRadiusHitInfo, Vector3.zero);
			
			// Include the skin width in the test
			bigRadiusHit = BigCapsuleCast(direction, distance, out bigRadiusHitInfo, Vector3.zero);

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
		private bool SmallCapsuleCast(Vector3 direction, float distance,
									  out RaycastHit smallRadiusHitInfo,
		                              Vector3 offsetPosition)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
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
		private bool BigCapsuleCast(Vector3 direction, float distance,
									out RaycastHit bigRadiusHitInfo,
		                            Vector3 offsetPosition)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
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
		/// Do a sphere cast, exlucliding the skin width. Sphere position is at the top or bottom of the capsule.
		/// </summary>
		/// <param name="direction">Direction to cast.</param>
		/// <param name="distance">Distance to cast.</param>
		/// <param name="smallRadiusHitInfo">Hit info.</param>
		/// <param name="offsetPosition">Position offset. If we want to do a cast not at the capsule's current position.</param>
		/// <param name="useBottomSphere">Use the sphere at the bottom of the capsule? If false then use the top sphere.</param>
		/// <returns></returns>
		private bool SmallSphereCast(Vector3 direction, float distance,
		                             out RaycastHit smallRadiusHitInfo,
		                             Vector3 offsetPosition,
		                             bool useBottomSphere)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
			// when moving almost parallel to an obstacle for small distances).
			float extraDistance = characterCapsule.scaledRadius;

			Vector3 spherePosition = useBottomSphere
				? characterCapsule.GetBottomSphereWorldPosition() + offsetPosition
				: characterCapsule.GetTopSphereWorldPosition() + offsetPosition;
			if (UnityEngine.Physics.SphereCast(spherePosition,
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
		/// Do a sphere cast, including the skin width. Sphere position is at the top or bottom of the capsule.
		/// </summary>
		/// <param name="direction">Direction to cast.</param>
		/// <param name="distance">Distance to cast.</param>
		/// <param name="bigRadiusHitInfo">Hit info.</param>
		/// <param name="offsetPosition">Position offset. If we want to do a cast not at the capsule's current position.</param>
		/// <param name="useBottomSphere">Use the sphere at the bottom of the capsule? If false then use the top sphere.</param>
		/// <returns></returns>
		private bool BigSphereCast(Vector3 direction, float distance,
		                           out RaycastHit bigRadiusHitInfo,
		                           Vector3 offsetPosition,
		                           bool useBottomSphere)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
			// when moving almost parallel to an obstacle for small distances).
			float extraDistance = characterCapsule.scaledRadius + characterCapsule.GetSkinWidth();

			Vector3 spherePosition = useBottomSphere
				? characterCapsule.GetBottomSphereWorldPosition() + offsetPosition
				: characterCapsule.GetTopSphereWorldPosition() + offsetPosition;
			if (UnityEngine.Physics.SphereCast(spherePosition,
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
		/// <param name="canSlide">Can slide against obstacles?</param>
		/// <param name="tryGrounding">Try grounding the player?</param>
		/// <param name="tryStepOver">Try to step over obstacles?</param>
		/// <param name="hitInfoIsSmallRadius">If the hitInfo for the capsule's small radius?</param>
		/// <param name="debugInfo">Debug info. Remove when testing is done.</param>
		private void MoveAwayFromObstacle(ref Vector3 moveVector, ref RaycastHit hitInfo,
		                                  Vector3 direction, float distance,
		                                  bool canSlide,
										  bool tryGrounding, 
		                                  bool tryStepOver,
		                                  bool hitInfoIsSmallRadius,
		                                  string debugInfo)
		{
			// IMPORTANT: This method must set moveVector.
			
			float hitDistance = Mathf.Max(hitInfo.distance - k_CollisionOffset, 0.0f);
			float remainingDistance = Mathf.Max(distance - hitDistance, 0.0f);

			// Move to the collision point
			MovePosition(direction * hitDistance, direction, hitInfo, string.Format("move to collision point (rd: {0}     mv: {1}, {2}, {3}     cSld: {4})", 
																					remainingDistance, 
																					moveVector.x, moveVector.y, moveVector.z,
																					canSlide));

			float skinPenetrationDistance;
			Vector3 skinPenetrationVector;

			GetPenetrationInfo(out skinPenetrationDistance, out skinPenetrationVector);

			// Push away from the obstacle
			MovePosition(skinPenetrationVector * skinPenetrationDistance, null, null,
			             string.Format("push away from obstacle (d: {0}     rd: {1})",
			                           distance,
			                           remainingDistance));
			
			bool slopeIsSteep = false;
			if (tryGrounding)
			{
				// No further movement when grounding the character
				canSlide = false;
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
					StartStepOver(moveVector, remainingDistance, Vector3.Angle(moveVector, -hitInfo.normal));
				}
				else if (UpdateStepOver(moveVector, remainingDistance, canSlide) == false)
				{
					StopStepOver(true, string.Format("UpdateStepOver returned false.     heightRem: {0}",
					                                 stepInfo.GetRemainingHeight(capsuleTransform.position)));
				}

				if (stepInfo.isStepping)
				{
					// Do not slide while stepping (e.g. prevent sliding off an obstacle we are trying to step over)
					canSlide = false;
				}
			}

			// Set moveVector
			if (canSlide)
			{
				// Vector to slide along the obstacle
				Vector3 project = Vector3.ProjectOnPlane(direction, hitInfo.normal);

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
			else
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

			int debugMaxCurrentPositionErrorCount = 5;
			if (debugCurrentPosition != null &&
			    debugCurrentPositionErrorCount < debugMaxCurrentPositionErrorCount &&	// Only show error 5 times
			    (Math.Abs(debugCurrentPosition.Value.x - capsuleTransform.position.x) > Mathf.Epsilon ||
			     Math.Abs(debugCurrentPosition.Value.y - capsuleTransform.position.y) > Mathf.Epsilon ||
			     Math.Abs(debugCurrentPosition.Value.z - capsuleTransform.position.z) > Mathf.Epsilon))
			{
				Debug.LogError(string.Format(
					               "{0}: The character capsule's position was changed by something other than Move, SimpleMove or SetPosition. " +
					               "[position: ({1}, {2}, {3})     should be: ({4}, {5}, {6})] (Only showing this error {7} times.)",
					               capsuleTransform.name,
					               capsuleTransform.position.x, capsuleTransform.position.y, capsuleTransform.position.z,
					               debugCurrentPosition.Value.x, debugCurrentPosition.Value.y, debugCurrentPosition.Value.z,
					               debugMaxCurrentPositionErrorCount));
				debugCurrentPositionErrorCount++;
				debugCurrentPosition = capsuleTransform.position;
			}
			#endif

			if (collideDirection != null &&
			    hitInfo != null)
			{
				UpdateCollisionInfo(collideDirection.Value, hitInfo.Value);
			}
		}

		/// <summary>
		/// Update the collision flags and info.
		/// </summary>
		/// <param name="direction">The direction moved.</param>
		/// <param name="hitInfo">The hit info of the collision.</param>
		private void UpdateCollisionInfo(Vector3 direction, RaycastHit hitInfo)
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

			hitCount++;

			CharacterCapsuleMoverCollisionInfo newCollisionInfo = new CharacterCapsuleMoverCollisionInfo(hitInfo);
			List<CharacterCapsuleMoverCollisionInfo> list = null;
			Collider collider = hitInfo.collider;
			if (collisionInfoDictionary.ContainsKey(collider))
			{
				list = collisionInfoDictionary[collider];
			}
			else
			{
				list = new List<CharacterCapsuleMoverCollisionInfo>();
				collisionInfoDictionary.Add(collider, list);
			}
			if (list != null)
			{
				list.Add(newCollisionInfo);
			}
		}
	}
}