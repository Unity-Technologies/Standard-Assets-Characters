using System;
using System.Collections.Generic;
using StandardAssets.Characters.Helpers;
using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Open character controller. Handles the movement of a character, by using a capsule for movement and collision detection.
	/// Note: The capsule is always upright. It ignores rotation.
	/// </summary>
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Rigidbody))]
	public class OpenCharacterController : MonoBehaviour
	{
		/// <summary>
		/// Fired on collision with colliders in the world
		/// </summary>
		public event Action<CollisionInfo> collision;
		
		/// <summary>
		/// Collision info used by the OpenCharacterController and sent to the OnOpenCharacterControllerHit message.
		/// </summary>
		public struct CollisionInfo
		{
			// The collider that was hit by the controller.
			readonly Collider m_Collider;

			// The controller that hit the collider.
			readonly OpenCharacterController m_Controller;

			// The game object that was hit by the controller.
			readonly GameObject m_GameObject;

			// The direction the character Controller was moving in when the collision occured.
			readonly Vector3 m_MoveDirection;

			// How far the character has travelled until it hit the collider.
			readonly float m_MoveLength;

			// The normal of the surface we collided with in world space.
			readonly Vector3 m_Normal;

			// The impact point in world space.
			readonly Vector3 m_Point;

			// The rigidbody that was hit by the controller.
			readonly Rigidbody m_Rigidbody;

			// The transform that was hit by the controller.
			readonly Transform m_Transform;

			/// <summary>
			/// Gets the <see cref="Collider"/> associated with the collision
			/// </summary>
			public Collider collider { get { return m_Collider; } }

			/// <summary>
			/// Gets the <see cref="OpenCharacterController"/> associated with the collision
			/// </summary>
			public OpenCharacterController controller { get { return m_Controller; } }

			/// <summary>
			/// Gets the <see cref="GameObject"/> associated with the collision
			/// </summary>
			public GameObject gameObject { get { return m_GameObject; } }

			/// <summary>
			/// Gets the move direction associated with the collision
			/// </summary>
			public Vector3 moveDirection { get { return m_MoveDirection; } }

			/// <summary>
			/// Gets the length of the move associated with the collision
			/// </summary>
			public float moveLength { get { return m_MoveLength; } }

			/// <summary>
			/// Gets the normal of the collision
			/// </summary>
			public Vector3 normal { get { return m_Normal; } }

			/// <summary>
			/// Gets the point of the collision
			/// </summary>
			public Vector3 point { get { return m_Point; } }

			/// <summary>
			/// Gets the <see cref="Rigidbody"/> associated with the collision
			/// </summary>
			public Rigidbody rigidbody { get { return m_Rigidbody; } }

			/// <summary>
			/// Gets the <see cref="Transform"/> associated with the collision
			/// </summary>
			public Transform transform { get { return m_Transform; } }

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="openCharacterController">The character controller that hit.</param>
			/// <param name="hitInfo">The hit info.</param>
			/// <param name="directionMoved">Direction moved when collision occured.</param>
			/// <param name="distanceMoved">How far the character has travelled until it hit the collider.</param>
			public CollisionInfo(OpenCharacterController openCharacterController,
			                     RaycastHit hitInfo,
			                     Vector3 directionMoved,
			                     float distanceMoved)
			{
				m_Collider = hitInfo.collider;
				m_Controller = openCharacterController;
				m_GameObject = hitInfo.collider.gameObject;
				m_MoveDirection = directionMoved;
				m_MoveLength = distanceMoved;
				m_Normal = hitInfo.normal;
				m_Point = hitInfo.point;
				m_Rigidbody = hitInfo.rigidbody;
				m_Transform = hitInfo.transform;
			}
		}

		// A vector used by the OpenCharacterController.
		struct MoveVector
		{
			/// <summary>
			/// The move vector.
			/// Note: This gets used up during the move loop, so will be zero by the end of the loop.
			/// </summary>
			public Vector3 moveVector { get; set; }

			/// <summary>
			/// Can the movement slide along obstacles?
			/// </summary>
			public bool canSlide { get; set; }
			
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="newMoveVector">The move vector.</param>
			/// <param name="newCanSlide">Can the movement slide along obstacles?</param>
			public MoveVector(Vector3 newMoveVector, bool newCanSlide = true)
				: this()
			{
				moveVector = newMoveVector;
				canSlide = newCanSlide;
			}
		}

		// Resize info for OpenCharacterController (e.g. delayed resizing until it is safe to resize).
		class ResizeInfo
		{
			// Intervals (seconds) in which to check if the capsule's height/center must be changed.
			const float k_PendingUpdateIntervals = 1.0f;

			/// <summary>
			/// Height to set.
			/// </summary>
			public float? height { get; private set; }

			/// <summary>
			/// Center to set.
			/// </summary>
			public Vector3? center { get; private set; }

			/// <summary>
			/// Time.time when the height must be set.
			/// </summary>
			public float? heightTime { get; private set; }

			/// <summary>
			/// Time.time when the center must be set.
			/// </summary>
			public float? centerTime { get; private set; }


			/// <summary>
			/// Set the pending height.
			/// </summary>
			public void SetHeight(float newHeight)
			{
				height = newHeight;
				if (heightTime == null)
				{
					heightTime = Time.time + k_PendingUpdateIntervals;
				}
			}

			/// <summary>
			/// Set the pending center.
			/// </summary>
			public void SetCenter(Vector3 newCenter)
			{
				center = newCenter;
				if (centerTime == null)
				{
					centerTime = Time.time + k_PendingUpdateIntervals;
				}
			}

			/// <summary>
			/// Set the pending height and center.
			/// </summary>
			public void SetHeightAndCenter(float newHeight, Vector3 newCenter)
			{
				SetHeight(newHeight);
				SetCenter(newCenter);
			}

			/// <summary>
			/// Cancel the pending height.
			/// </summary>
			public void CancelHeight()
			{
				height = null;
				heightTime = null;
			}

			/// <summary>
			/// Cancel the pending center.
			/// </summary>
			public void CancelCenter()
			{
				center = null;
				centerTime = null;
			}

			/// <summary>
			/// Cancel the pending height and center.
			/// </summary>
			public void CancelHeightAndCenter()
			{
				CancelHeight();
				CancelCenter();
			}

			/// <summary>
			/// Clear the timers.
			/// </summary>
			public void ClearTimers()
			{
				heightTime = null;
				centerTime = null;
			}
		}

		// Stuck info and logic used by the OpenCharacterController.
		class StuckInfo
		{
			// For keeping track of the character's position, to determine when the character gets stuck.
			Vector3? m_StuckPosition;

			// Count how long the character is in the same position.
			int m_StuckPositionCount;

			// If character's position does not change by more than this amount then we assume the character is stuck.
			const float k_StuckDistance = 0.001f;

			// If character's position does not change by more than this amount then we assume the character is stuck.
			const float k_StuckSqrDistance = k_StuckDistance * k_StuckDistance;

			// If character collided this number of times during the movement loop then test if character is stuck by examining the position
			const int k_HitCountForStuck = 6;

			// Assume character is stuck if the position is the same for longer than this number of loop iterations
			const int k_MaxStuckPositionCount = 1;

			/// <summary>
			/// Is the character stuck in the current move loop iteration?
			/// </summary>
			public bool isStuck { get; set; }

			/// <summary>
			/// Count the number of collisions during movement, to determine when the character gets stuck.
			/// </summary>
			public int hitCount { get; set; }


			/// <summary>
			/// Called when the move loop starts.
			/// </summary>
			public void OnMoveLoop()
			{
				hitCount = 0;
				m_StuckPositionCount = 0;
				m_StuckPosition = null;
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
				if (!isStuck)
				{
					// From Quake2: "if velocity is against the original velocity, stop dead to avoid tiny occilations in sloping corners"
					if (currentMoveVector.sqrMagnitude.NotEqualToZero() &&
					    Vector3.Dot(currentMoveVector, originalMoveVector) <= 0.0f)
					{
						isStuck = true;
					}
				}

				// Second test
				if (!isStuck)
				{
					// Test if collided and while position remains the same
					if (hitCount < k_HitCountForStuck)
					{
						return false;
					}

					if (m_StuckPosition == null)
					{
						m_StuckPosition = characterPosition;
					}
					else if (m_StuckPosition.Value.SqrMagnitudeFrom(characterPosition) <= k_StuckSqrDistance)
					{
						m_StuckPositionCount++;
						if (m_StuckPositionCount > k_MaxStuckPositionCount)
						{
							isStuck = true;
						}
					}
					else
					{
						m_StuckPositionCount = 0;
						m_StuckPosition = null;
					}
				}

				if (isStuck)
				{
					isStuck = false;
					hitCount = 0;
					m_StuckPositionCount = 0;
					m_StuckPosition = null;

					return true;
				}

				return false;
			}
		}


		[Header("Player Root")]
		[SerializeField, Tooltip("The root bone in the avatar.")]
		Transform m_PlayerRootTransform;

		[SerializeField, Tooltip("The root transform will be positioned at this offset.")]
		Vector3 m_RootTransformOffset = new Vector3(0, 0, 0);

		[Header("Collision")]
		[SerializeField, Tooltip("Limits the collider to only climb slopes that are less steep (in degrees) than the indicated value.")]
		float m_SlopeLimit = 45.0f;

		[SerializeField, Tooltip("The character will step up a stair only if it is closer to the ground than the indicated value. " +
		         "This should not be greater than the Character Controller’s height or it will generate an error. " +
		         "Generally this should be kept as small as possible.")]
		float m_StepOffset = 0.3f;

		[SerializeField, Tooltip(
			"Two colliders can penetrate each other as deep as their Skin Width. Larger Skin Widths reduce jitter. " +
			"Low Skin Width can cause the character to get stuck. A good setting is to make this value 10% of the Radius.")]
		float m_SkinWidth = 0.08f;

		[SerializeField, Tooltip(
			"If the character tries to move below the indicated value, it will not move at all. This can be used to reduce jitter. " +
			"In most situations this value should be left at 0.")]
		float m_MinMoveDistance;

		[SerializeField, Tooltip("This will offset the Capsule Collider in world space, and won’t affect how the Character pivots. " +
		         "Ideally, x and z should be zero to avoid rotating into another collider.")]
		Vector3 m_Center;

		[SerializeField, Tooltip("Length of the Capsule Collider’s radius. This is essentially the width of the collider.")]
		float m_Radius = 0.5f;

		[SerializeField, Tooltip("The Character’s Capsule Collider height. It should be at least double the radius.")]
		float m_Height = 2.0f;

		[SerializeField, Tooltip("Layers to test against for collisions.")]
		LayerMask m_CollisionLayerMask = ~0; // ~0 sets it to Everything

		[SerializeField, Tooltip("Is the character controlled by a local human? If true then more calculations are done for more " +
		         "accurate movement.")]
		bool m_IsLocalHuman = true;

		[SerializeField, Tooltip("Can character slide vertically when touching the ceiling? (For example, if ceiling is sloped.)")]
		bool m_SlideAlongCeiling = true;

		[SerializeField, Tooltip("Should the character slow down against walls?")]
		bool m_SlowAgainstWalls = false;

		[SerializeField, Range(0.0f, 90.0f), Tooltip("The minimal angle from which the character will start slowing down on walls.")]
		float m_MinSlowAgainstWallsAngle = 10.0f;

		[SerializeField, Tooltip("The desired interaction that cast calls should make against triggers")]
		QueryTriggerInteraction m_TriggerQuery = QueryTriggerInteraction.Ignore;

		[Header("Sliding")]
		[SerializeField, Tooltip("Should the character slide down slopes when their angle is more than the slope limit?")]
		bool m_SlideDownSlopes = true;

		[SerializeField, Tooltip("The maximum speed that the character can slide downwards")]
		float m_SlideMaxSpeed = 10.0f;

		[SerializeField, Tooltip("Gravity scale to apply when sliding down slopes.")]
		float m_SlideGravityScale = 1.0f;

		[SerializeField, Tooltip("The time (in seconds) after initiating a slide classified as a slide start. Used to disable jumping.")]
		float m_SlideStartTime = 0.25f;

		// Max slope limit.
		const float k_MaxSlopeLimit = 90.0f;

		// Max slope angle on which character can slide down automatically.
		const float k_MaxSlopeSlideAngle = 90.0f;

		// Distance to test for ground when sliding down slopes.
		const float k_SlideDownSlopeTestDistance = 1.0f;

		// Slight delay before we stop sliding down slopes. To handle cases where sliding test fails for a few frames.
		const float k_StopSlideDownSlopeDelay = 0.5f;

		// Distance to push away from slopes when sliding down them.
		const float k_PushAwayFromSlopeDistance = 0.001f;

		// Minimum distance to use when checking ahead for steep slopes, when checking if it's safe to do the step offset.
		const float k_MinCheckSteepSlopeAheadDistance = 0.2f;

		// Min skin width.
		const float k_MinSkinWidth = 0.0001f;

		// The maximum move iterations. Mainly used as a fail safe to prevent an infinite loop.
		const int k_MaxMoveIterations = 20;

		// Stick to the ground if it is less than this distance from the character.
		const float k_MaxStickToGroundDownDistance = 1.0f;

		// Min distance to test for the ground when sticking to the ground.
		const float k_MinStickToGroundDownDistance = 0.01f;

		// Max colliders to use in the overlap methods.
		const int k_MaxOverlapColliders = 10;

		// Offset to use when moving to a collision point, to try to prevent overlapping the colliders
		const float k_CollisionOffset = 0.001f;

		// Distance to test beneath the character when doing the grounded test
		const float k_GroundedTestDistance = 0.001f;

		// Minimum distance to move. This minimizes small penetrations and inaccurate casts (e.g. into the floor)
		const float k_MinMoveDistance = 0.0001f;

		// Minimum sqr distance to move. This minimizes small penetrations and inaccurate casts (e.g. into the floor)
		const float k_MinMoveSqrDistance = k_MinMoveDistance * k_MinMoveDistance;

		// Minimum step offset height to move (if character has a step offset).
		const float k_MinStepOffsetHeight = k_MinMoveDistance;

		// Small value to test if the movement vector is small.
		const float k_SmallMoveVector = 1e-6f;

		// If angle between raycast and capsule/sphere cast normal is less than this then use the raycast normal, which is more accurate.
		const float k_MaxAngleToUseRaycastNormal = 5.0f;

		// Scale the capsule/sphere hit distance when doing the additional raycast to get a more accurate normal
		const float k_RaycastScaleDistance = 2.0f;
		
		// Slope check ahead is clamped by the distance moved multiplied by this scale.
		const float k_SlopeCheckDistanceMultiplier = 5.0f;

		// The capsule collider.
		CapsuleCollider m_CapsuleCollider;

		// The position at the start of the movement.
		Vector3 m_StartPosition;

		// Movement vectors used in the move loop.
		List<MoveVector> m_MoveVectors = new List<MoveVector>();

		// Next index in the moveVectors list.
		int m_NextMoveVectorIndex;

		// Surface normal of the last collision while moving down.
		Vector3? m_DownCollisionNormal;

		// Stuck info.
		StuckInfo m_StuckInfo = new StuckInfo();

		// The collision info when hitting colliders.
		Dictionary<Collider, CollisionInfo> m_CollisionInfoDictionary = new Dictionary<Collider, CollisionInfo>();

		// Slight delay before stopping the sliding down slopes. 
		float m_DelayStopSlidingDownSlopeTime;

		// Pending resize info to set when it is safe to do so.
		readonly ResizeInfo m_PendingResize = new ResizeInfo();

		// Collider array used for UnityEngine.Physics.OverlapCapsuleNonAlloc in GetPenetrationInfo
		readonly Collider[] m_PenetrationInfoColliders = new Collider[k_MaxOverlapColliders];

		// Velocity of the last movement. It's the new position minus the old position.
		Vector3 m_Velocity;

		// Factor used to perform a slow down against the walls.
		float m_InvRescaleFactor;

		// How long has character been sliding down a steep slope? (Zero means not busy sliding.)
		float m_SlidingDownSlopeTime;

		// Default center of the capsule (e.g. for resetting it).
		Vector3 m_DefaultCenter;

		// Used to offset movement raycast when determining if a slope is travesable.
		float m_SlopeMovementOffset;

		// Is character busy sliding down a steep slope?
		bool isSlidingDownSlope { get { return m_SlidingDownSlopeTime > 0.0f; } }

		// The capsule center with scaling and rotation applied.
		Vector3 transformedCenter { get { return transform.TransformVector(m_Center); } }

		// The capsule height with the relevant scaling applied (e.g. if object scale is not 1,1,1)
		float scaledHeight { get { return m_Height * transform.lossyScale.y; } }
		
		/// <summary>
		/// Is the character on the ground? This is updated during Move or SetPosition.
		/// </summary>
		public bool isGrounded { get; private set; }

		/// <summary>
		/// Collision flags from the last move.
		/// </summary>
		public CollisionFlags collisionFlags { get; private set; }

		/// <summary>
		/// Default height of the capsule (e.g. for resetting it).
		/// </summary>
		public float defaultHeight { get; private set; }
		
		/// <summary>
		/// Is the character able to be slowed down by walls?
		/// </summary>
		public bool slowAgainstWalls { get { return m_SlowAgainstWalls; } }

		/// <summary>
		/// Is the character sliding and has been sliding less than slideDownTimeUntilJumAllowed
		/// </summary>
		public bool startedSlide { get { return isSlidingDownSlope && m_SlidingDownSlopeTime <= m_SlideStartTime; } }

		/// <summary>
		/// The capsule radius with the relevant scaling applied (e.g. if object scale is not 1,1,1)
		/// </summary>
		public float scaledRadius
		{
			get
			{
				var scale = transform.lossyScale;
				var maxScale = Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);
				return m_Radius * maxScale;
			}
		}

		/// <summary>
		/// Is the character controlled by a local human? If true then more calculations are done for more accurate movement.
		/// </summary>
		public bool IsLocalHuman { get { return m_IsLocalHuman; } }

		// Initialise the capsule and rigidbody, and set the root position.
		void Awake()
		{
			InitCapsuleColliderAndRigidbody();

			SetRootToOffset();
			
			m_InvRescaleFactor = 1 / Mathf.Cos(m_MinSlowAgainstWallsAngle * Mathf.Deg2Rad);
			m_SlopeMovementOffset =  m_StepOffset / Mathf.Tan(m_SlopeLimit * Mathf.Deg2Rad);
		}

		// Set the root position.
		void LateUpdate()
		{
			SetRootToOffset();
		}

		// Update sliding down slopes, and changes to the capsule's height and center.
		void Update()
		{
			UpdateSlideDownSlopes();
			UpdatePendingHeightAndCenter();
		}

#if UNITY_EDITOR
		// Validate the capsule.
		void OnValidate()
		{
			var position = transform.position;
			ValidateCapsule(false, ref position);
			transform.position = position;
			SetRootToOffset();
			
			m_InvRescaleFactor = 1 / Mathf.Cos(m_MinSlowAgainstWallsAngle * Mathf.Deg2Rad);
		}

		// Draws the debug Gizmos
		void OnDrawGizmosSelected()
		{
			// Foot position
			Gizmos.color = Color.cyan;
			var footPosition = GetFootWorldPosition(transform.position);
			Gizmos.DrawLine(footPosition + Vector3.left * scaledRadius,
			                footPosition + Vector3.right * scaledRadius);
			Gizmos.DrawLine(footPosition + Vector3.back * scaledRadius,
			                footPosition + Vector3.forward * scaledRadius);

			// Top of head
			var headPosition = transform.position + transformedCenter + Vector3.up * (scaledHeight / 2.0f + m_SkinWidth);
			Gizmos.DrawLine(headPosition + Vector3.left * scaledRadius,
			                headPosition + Vector3.right * scaledRadius);
			Gizmos.DrawLine(headPosition + Vector3.back * scaledRadius,
			                headPosition + Vector3.forward * scaledRadius);

			// Center position
			var centerPosition = transform.position + transformedCenter;
			Gizmos.DrawLine(centerPosition + Vector3.left * scaledRadius,
			                centerPosition + Vector3.right * scaledRadius);
			Gizmos.DrawLine(centerPosition + Vector3.back * scaledRadius,
			                centerPosition + Vector3.forward * scaledRadius);
		}
#endif

		/// <summary>
		/// Move the character. This function does not apply any gravity.
		/// </summary>
		/// <param name="moveVector">Move along this vector.</param>
		/// <returns>CollisionFlags is the summary of collisions that occurred during the Move.</returns>
		public CollisionFlags Move(Vector3 moveVector)
		{
			MoveInternal(moveVector, true);
			return collisionFlags;
		}

		/// <summary>
		/// Set the position of the character.
		/// </summary>
		/// <param name="position">Position to set.</param>
		/// <param name="updateGrounded">Update the grounded state? This uses a cast, so only set it to true if you need it.</param>
		public void SetPosition(Vector3 position, bool updateGrounded)
		{
			transform.position = position;

			if (updateGrounded)
			{
				UpdateGrounded(CollisionFlags.None);
			}
		}

		/// <summary>
		/// Compute the minimal translation required to separate the character from the collider.
		/// </summary>
		/// <param name="positionOffset">Position offset to add to the capsule collider's position.</param>
		/// <param name="collider">The collider to test.</param>
		/// <param name="colliderPosition">Position of the collider.</param>
		/// <param name="colliderRotation">Rotation of the collider.</param>
		/// <param name="direction">Direction along which the translation required to separate the colliders apart is minimal.</param>
		/// <param name="distance">The distance along direction that is required to separate the colliders apart.</param>
		/// <param name="includeSkinWidth">Include the skin width in the test?</param>
		/// <param name="currentPosition">Position of the character</param>
		/// <returns>True if found penetration.</returns>
		bool ComputePenetration(Vector3 positionOffset,
		                               Collider collider, Vector3 colliderPosition, Quaternion colliderRotation,
		                               out Vector3 direction, out float distance,
		                               bool includeSkinWidth, Vector3 currentPosition)
		{
			if (collider == m_CapsuleCollider)
			{
				// Ignore self
				direction = Vector3.one;
				distance = 0.0f;
				return false;
			}

			if (includeSkinWidth)
			{
				m_CapsuleCollider.radius = m_Radius + m_SkinWidth;
				m_CapsuleCollider.height = m_Height + (m_SkinWidth * 2.0f);
			}

			// Note: Physics.ComputePenetration does not always return values when the colliders overlap.
			var result = UnityEngine.Physics.ComputePenetration(m_CapsuleCollider,
			                                                     currentPosition + positionOffset,
			                                                     Quaternion.identity,
			                                                     collider, colliderPosition, colliderRotation,
			                                                     out direction, out distance);
			if (includeSkinWidth)
			{
				m_CapsuleCollider.radius = m_Radius;
				m_CapsuleCollider.height = m_Height;
			}

			return result;
		}

		/// <summary>
		/// Check for collision below the character, using a ray or sphere cast.
		/// </summary>
		/// <param name="distance">Distance to check.</param>
		/// <param name="hitInfo">Get the hit info.</param>
		/// <param name="offsetPosition">Position offset. If we want to do a cast relative to the character's current position.</param>
		/// <param name="useSphereCast">Use a sphere cast? If false then use a ray cast.</param>
		/// <param name="useSecondSphereCast">The second cast includes the skin width. Ideally only needed for human controlled player, for more accuracy.</param>
		/// <param name="adjustPositionSlightly">Adjust position slightly up, in case it's already inside an obstacle.</param>
		/// <param name="currentPosition">Position of the character</param>
		/// <returns>True if collision occurred.</returns>
		public bool CheckCollisionBelow(float distance, out RaycastHit hitInfo, Vector3 currentPosition,
		                                Vector3 offsetPosition,
		                                bool useSphereCast = false,
		                                bool useSecondSphereCast = false,
		                                bool adjustPositionSlightly = false)
		{
			var didCollide = false;
			var extraDistance = adjustPositionSlightly ? k_CollisionOffset : 0.0f;
			if (!useSphereCast)
			{
#if UNITY_EDITOR
				var start = GetFootWorldPosition(currentPosition) + offsetPosition + Vector3.up * extraDistance;
				Debug.DrawLine(start, start + Vector3.down * (distance + extraDistance), Color.red);
#endif
				if (UnityEngine.Physics.Raycast(GetFootWorldPosition(currentPosition) + offsetPosition + Vector3.up * extraDistance,
				                                Vector3.down,
				                                out hitInfo,
				                                distance + extraDistance,
				                                GetCollisionLayerMask(),
				                                m_TriggerQuery))
				{
					didCollide = true;
					hitInfo.distance = Mathf.Max(0.0f, hitInfo.distance - extraDistance);
				}
			}
			else
			{
#if UNITY_EDITOR
				Debug.DrawRay(currentPosition, Vector3.down, Color.red); // Center
			
				Debug.DrawRay(currentPosition +  new Vector3(scaledRadius, 0.0f), Vector3.down, Color.blue);
				Debug.DrawRay(currentPosition +  new Vector3(-scaledRadius, 0.0f), Vector3.down, Color.blue);
				Debug.DrawRay(currentPosition +  new Vector3(0.0f, 0.0f, scaledRadius), Vector3.down, Color.blue);
				Debug.DrawRay(currentPosition +  new Vector3(0.0f, 0.0f, -scaledRadius), Vector3.down, Color.blue);
#endif
				if (SmallSphereCast(Vector3.down,
				                    GetSkinWidth() + distance,
				                    out hitInfo,
				                    offsetPosition,
				                    true, currentPosition))
				{
					didCollide = true;
					hitInfo.distance = Mathf.Max(0.0f, hitInfo.distance - GetSkinWidth());
				}

				if (!didCollide && useSecondSphereCast)
				{
					if (BigSphereCast(Vector3.down,
					                  distance + extraDistance, currentPosition,
					                  out hitInfo,
					                  offsetPosition + Vector3.up * extraDistance,
					                  true))
					{
						didCollide = true;
						hitInfo.distance = Mathf.Max(0.0f, hitInfo.distance - extraDistance);
					}
				}
			}

			return didCollide;
		}

		/// <summary>
		/// Get the skin width.
		/// </summary>
		public float GetSkinWidth()
		{
			return m_SkinWidth;
		}

		/// <summary>
		/// Get the minimum move sqr distance.
		/// </summary>
		float GetMinMoveSqrDistance()
		{
			return m_MinMoveDistance * m_MinMoveDistance;
		}

		/// <summary>
		/// Set the capsule's height and center.
		/// </summary>
		/// <param name="newHeight">The new height.</param>
		/// <param name="newCenter">The new center.</param>
		/// <param name="checkForPenetration">Check for collision, and then de-penetrate if there's collision?</param>
		/// <param name="updateGrounded">Update the grounded state? This uses a cast, so only set it to true if you need it.</param>
		/// <returns>Returns the height that was set, which may be different to newHeight because of validation.</returns>
		public float SetHeightAndCenter(float newHeight, Vector3 newCenter, bool checkForPenetration,
		                                bool updateGrounded)
		{
			var oldHeight = m_Height;
			var oldCenter = m_Center;
			var oldPosition = transform.position;
			var cancelPending = true;
			Vector3 virtualPosition = oldPosition;

			SetCenter(newCenter, false, false);
			SetHeight(newHeight, false, false, false);

			if (checkForPenetration)
			{
				if (Depenetrate(ref virtualPosition))
				{
					// Inside colliders?
					if (CheckCapsule(virtualPosition))
					{
						// Wait until it is safe to resize
						cancelPending = false;
						m_PendingResize.SetHeightAndCenter(newHeight, newCenter);
						// Restore data
						m_Height = oldHeight;
						m_Center = oldCenter;
						transform.position = oldPosition;
						ValidateCapsule(true, ref virtualPosition);
					}
				}
			}

			if (cancelPending)
			{
				m_PendingResize.CancelHeightAndCenter();
			}

			if (updateGrounded)
			{
				UpdateGrounded(CollisionFlags.None);
			}

			transform.position = virtualPosition;
			return m_Height;
		}

		/// <summary>
		/// Reset the capsule's height and center to the default values.
		/// </summary>
		/// <param name="checkForPenetration">Check for collision, and then de-penetrate if there's collision?</param>
		/// <param name="updateGrounded">Update the grounded state? This uses a cast, so only set it to true if you need it.</param>
		/// <returns>Returns the reset height.</returns>
		public float ResetHeightAndCenter(bool checkForPenetration, bool updateGrounded)
		{
			return SetHeightAndCenter(defaultHeight, m_DefaultCenter, checkForPenetration, updateGrounded);
		}

		/// <summary>
		/// Get the capsule's center (local).
		/// </summary>
		public Vector3 GetCenter()
		{
			return m_Center;
		}

		/// <summary>
		/// Set the capsule's center (local).
		/// </summary>
		/// <param name="newCenter">The new center.</param>
		/// <param name="checkForPenetration">Check for collision, and then de-penetrate if there's collision?</param>
		/// <param name="updateGrounded">Update the grounded state? This uses a cast, so only set it to true if you need it.</param>
		public void SetCenter(Vector3 newCenter, bool checkForPenetration, bool updateGrounded)
		{
			var oldCenter = m_Center;
			var oldPosition = transform.position;
			var cancelPending = true;
			var virtualPosition = oldPosition;

			m_Center = newCenter;
			ValidateCapsule(true, ref virtualPosition);

			if (checkForPenetration)
			{
				if (Depenetrate(ref virtualPosition))
				{
					// Inside colliders?
					if (CheckCapsule(virtualPosition))
					{
						// Wait until it is safe to resize
						cancelPending = false;
						m_PendingResize.SetCenter(newCenter);
						// Restore data
						m_Center = oldCenter;
						transform.position = oldPosition;
						ValidateCapsule(true, ref virtualPosition);
					}
				}
			}

			if (cancelPending)
			{
				m_PendingResize.CancelCenter();
			}

			if (updateGrounded)
			{
				UpdateGrounded(CollisionFlags.None);
			}

			transform.position = virtualPosition;
		}

		/// <summary>
		/// Reset the capsule's center to the default value.
		/// </summary>
		/// <param name="checkForPenetration">Check for collision, and then de-penetrate if there's collision?</param>
		/// <param name="updateGrounded">Update the grounded state? This uses a cast, so only set it to true if you need it.</param>
		public void ResetCenter(bool checkForPenetration, bool updateGrounded)
		{
			SetCenter(m_DefaultCenter, checkForPenetration, updateGrounded);
		}

		/// <summary>
		/// Get the capsule's height (local).
		/// </summary>
		public float GetHeight()
		{
			return m_Height;
		}

		/// <summary>
		/// Validate the capsule's height. (It must be at least double the radius size.)
		/// </summary>
		/// <returns>The valid height.</returns>
		public float ValidateHeight(float newHeight)
		{
			return Mathf.Clamp(newHeight, m_Radius * 2.0f, float.MaxValue);
		}

		/// <summary>
		/// Set the capsule's height (local). Minimum limit is double the capsule radius size.
		/// Call CanSetHeight if you want to test if height can change, e.g. when changing from crouch to stand.
		/// </summary>
		/// <param name="newHeight">The new height.</param>
		/// <param name="preserveFootPosition">Adjust the capsule's center to preserve the foot position?</param>
		/// <param name="checkForPenetration">Check for collision, and then de-penetrate if there's collision?</param>
		/// <param name="updateGrounded">Update the grounded state? This uses a cast, so only set it to true if you need it.</param>
		/// <returns>Returns the height that was set, which may be different to newHeight because of validation.</returns>
		float SetHeight(float newHeight, bool preserveFootPosition, bool checkForPenetration,
		                       bool updateGrounded)
		{
			var virtualPosition = transform.position;
			var changeCenter = preserveFootPosition;
			var newCenter = changeCenter ? CalculateCenterWithSameFootPosition(newHeight) : m_Center;
			newHeight = ValidateHeight(newHeight);
			if (Mathf.Approximately(m_Height, newHeight))
			{
				// Height remains the same
				m_PendingResize.CancelHeight();
				if (changeCenter)
				{
					SetCenter(newCenter, checkForPenetration, updateGrounded);
				}

				return m_Height;
			}

			var oldHeight = m_Height;
			var oldCenter = m_Center;
			var oldPosition = transform.position;
			var cancelPending = true;

			if (changeCenter)
			{
				m_Center = newCenter;
			}

			m_Height = newHeight;
			ValidateCapsule(true, ref virtualPosition);

			if (checkForPenetration)
			{
				if (Depenetrate(ref virtualPosition))
				{
					// Inside colliders?
					if (CheckCapsule(virtualPosition))
					{
						// Wait until it is safe to resize
						cancelPending = false;
						if (changeCenter)
						{
							m_PendingResize.SetHeightAndCenter(newHeight, newCenter);
						}
						else
						{
							m_PendingResize.SetHeight(newHeight);
						}

						// Restore data
						m_Height = oldHeight;
						if (changeCenter)
						{
							m_Center = oldCenter;
						}

						transform.position = oldPosition;
						ValidateCapsule(true, ref virtualPosition);
					}
				}
			}

			if (cancelPending)
			{
				if (changeCenter)
				{
					m_PendingResize.CancelHeightAndCenter();
				}
				else
				{
					m_PendingResize.CancelHeight();
				}
			}

			if (updateGrounded)
			{
				UpdateGrounded(CollisionFlags.None);
			}

			transform.position = virtualPosition;
			return m_Height;
		}

		/// <summary>
		/// Reset the capsule's height to the default value.
		/// </summary>
		/// <param name="preserveFootPosition">Adjust the capsule's center to preserve the foot position?</param>
		/// <param name="checkForPenetration">Check for collision, and then de-penetrate if there's collision?</param>
		/// <param name="updateGrounded">Update the grounded state? This uses a cast, so only set it to true if you need it.</param>
		/// <returns>Returns the reset height.</returns>
		public float ResetHeight(bool preserveFootPosition, bool checkForPenetration, bool updateGrounded)
		{
			return SetHeight(defaultHeight, preserveFootPosition, checkForPenetration, updateGrounded);
		}

		/// <summary>
		/// Get the layers to test for collision.
		/// </summary>
		public LayerMask GetCollisionLayerMask()
		{
			return m_CollisionLayerMask;
		}

		/// <summary>
		/// Get the foot world position.
		/// </summary>
		public Vector3 GetFootWorldPosition()
		{
			return transform.position + transformedCenter + (Vector3.down * (scaledHeight / 2.0f + m_SkinWidth));
		}
		
		// Get the foot world position.
		Vector3 GetFootWorldPosition(Vector3 position)
		{
			return position + transformedCenter + (Vector3.down * (scaledHeight / 2.0f + m_SkinWidth));
		}

		/// <summary>
		/// Get the top sphere's world position.
		/// </summary>
		Vector3 GetTopSphereWorldPosition(Vector3 position)
		{
			var sphereOffsetY = Vector3.up * (scaledHeight / 2.0f - scaledRadius);
			return position + transformedCenter + sphereOffsetY;
		}

		/// <summary>
		/// Get the bottom sphere's world position.
		/// </summary>
		Vector3 GetBottomSphereWorldPosition(Vector3 position)
		{
			var sphereOffsetY = Vector3.up * (scaledHeight / 2.0f - scaledRadius);
			return position + transformedCenter - sphereOffsetY;
		}
		
		// Initialize the capsule collider and the rigidbody
		void InitCapsuleColliderAndRigidbody()
		{
			var go = transform.gameObject;
			m_CapsuleCollider = go.GetComponent<CapsuleCollider>();

			// Copy settings to the capsule collider
			m_CapsuleCollider.center = m_Center;
			m_CapsuleCollider.radius = m_Radius;
			m_CapsuleCollider.height = m_Height;

			// Ensure that the rigidbody is kinematic and does not use gravity 
			var rigidbody = go.GetComponent<Rigidbody>();
			rigidbody.isKinematic = true;
			rigidbody.useGravity = false;

			defaultHeight = m_Height;
			m_DefaultCenter = m_Center;
		}

		// Call this when the capsule's values change.
		// 		updateCapsuleCollider: Update the capsule collider's values (e.g. center, height, radius)?
		//		currentPosition: position of the character
		// 		checkForPenetration: Check for collision, and then de-penetrate if there's collision?
		//	 	updateGrounded: Update the grounded state? This uses a cast, so only set it to true if you need it.
		void ValidateCapsule(bool updateCapsuleCollider, ref Vector3 currentPosition, bool checkForPenetration = false, 
			bool updateGrounded = false)
		{
			m_SlopeLimit = Mathf.Clamp(m_SlopeLimit, 0.0f, k_MaxSlopeLimit);
			m_SkinWidth = Mathf.Clamp(m_SkinWidth, k_MinSkinWidth, float.MaxValue);
			var oldHeight = m_Height;
			m_Height = ValidateHeight(m_Height);

			if (m_CapsuleCollider != null)
			{
				if (updateCapsuleCollider)
				{
					// Copy settings to the capsule collider
					m_CapsuleCollider.center = m_Center;
					m_CapsuleCollider.radius = m_Radius;
					m_CapsuleCollider.height = m_Height;
				}
				else if (!Mathf.Approximately(m_Height, oldHeight))
				{
					// Height changed
					m_CapsuleCollider.height = m_Height;
				}
			}

			if (checkForPenetration)
			{
				Depenetrate(ref currentPosition);
			}

			if (updateGrounded)
			{
				UpdateGrounded(CollisionFlags.None);
			}
		}

		// Calculate a new center if the height changes and preserve the foot position.
		//		newHeight: New height
		Vector3 CalculateCenterWithSameFootPosition(float newHeight)
		{
			var localFootY = m_Center.y - (m_Height / 2.0f + m_SkinWidth);
			var newCenterY = localFootY + (newHeight / 2.0f + m_SkinWidth);
			return new Vector3(m_Center.x, newCenterY, m_Center.z);
		}

		// Moves the characters.
		// 		moveVector: Move vector
		// 		slideWhenMovingDown: Slide against obstacles when moving down? (e.g. we don't want to slide when applying gravity while the character is grounded)
		// 		forceTryStickToGround: Force try to stick to ground? Only used if character is grounded before moving.
		// 		doNotStepOffset: Do not try to perform the step offset?
		void MoveInternal(Vector3 moveVector, bool slideWhenMovingDown,
		                                    bool forceTryStickToGround = false,
		                                    bool doNotStepOffset = false)
		{
			var wasGrounded = isGrounded;
			var moveVectorNoY = new Vector3(moveVector.x, 0.0f, moveVector.z);
			var tryToStickToGround = wasGrounded && (forceTryStickToGround || (moveVector.y <= 0.0f && moveVectorNoY.sqrMagnitude.NotEqualToZero()));

			m_StartPosition = transform.position;

			collisionFlags = CollisionFlags.None;
			m_CollisionInfoDictionary.Clear();
			m_DownCollisionNormal = null;

			// Stop sliding down slopes when character jumps
			if (moveVector.y > 0.0f && isSlidingDownSlope)
			{
				StopSlideDownSlopes();
			}

			// Do the move loop
			MoveLoop(moveVector, tryToStickToGround, slideWhenMovingDown, doNotStepOffset);

			var doDownCast = tryToStickToGround ||
			                  moveVector.y <= 0.0f;
			UpdateGrounded(collisionFlags, doDownCast);
			m_Velocity = transform.position - m_StartPosition;

			BroadcastCollisionEvent();
		}

		// Send hit messages.
		void BroadcastCollisionEvent()
		{
			if (collision == null || m_CollisionInfoDictionary == null || m_CollisionInfoDictionary.Count <= 0)
			{
				return;
			}

			foreach (var keyValuePair in m_CollisionInfoDictionary)
			{
				collision(keyValuePair.Value);
			}
		}

		// Determine if the character is grounded.
		// 		movedCollisionFlags: Moved collision flags of the current move. Set to None if not moving.
		// 		doDownCast: Do a down cast? We want to avoid this when the character is jumping upwards.
		void UpdateGrounded(CollisionFlags movedCollisionFlags, bool doDownCast = true)
		{
			if ((movedCollisionFlags & CollisionFlags.CollidedBelow) != 0)
			{
				isGrounded = true;
			}
			else if (doDownCast)
			{
				RaycastHit hitInfo;
				isGrounded = CheckCollisionBelow(k_GroundedTestDistance,
				                                 out hitInfo, transform.position,
				                                 Vector3.zero,
				                                 true,
				                                 m_IsLocalHuman,
				                                 m_IsLocalHuman);
			}
			else
			{
				isGrounded = false;
			}
		}
		
		// Movement loop. Keep moving until completely blocked by obstacles, or we reached the desired position/distance.
		// 		moveVector: The move vector.
		// 		tryToStickToGround: Try to stick to the ground?
		// 		slideWhenMovingDown: Slide against obstacles when moving down? (e.g. we don't want to slide when applying gravity while the charcter is grounded)
		// 		doNotStepOffset: Do not try to perform the step offset?
		void MoveLoop(Vector3 moveVector, bool tryToStickToGround, bool slideWhenMovingDown, bool doNotStepOffset)
		{
			m_MoveVectors.Clear();
			m_NextMoveVectorIndex = 0;

			// Split the move vector into horizontal and vertical components.
			SplitMoveVector(moveVector, slideWhenMovingDown, doNotStepOffset);
			var remainingMoveVector = m_MoveVectors[m_NextMoveVectorIndex];
			m_NextMoveVectorIndex++;

			var didTryToStickToGround = false;
			m_StuckInfo.OnMoveLoop();
			var virtualPosition = transform.position;
			
			// The loop
			for (var i = 0; i < k_MaxMoveIterations; i++)
			{
				var refMoveVector = remainingMoveVector.moveVector;
				var collided = MoveMajorStep(ref refMoveVector, remainingMoveVector.canSlide, didTryToStickToGround, ref virtualPosition);

				remainingMoveVector.moveVector = refMoveVector;

				// Character stuck?
				if (m_StuckInfo.UpdateStuck(virtualPosition, remainingMoveVector.moveVector, moveVector))
				{
					// Stop current move loop vector
					remainingMoveVector = new MoveVector(Vector3.zero);
				}
				else if (!m_IsLocalHuman && collided)
				{
					// Only slide once for non-human controlled characters
					remainingMoveVector.canSlide = false;
				}

				// Not collided OR vector used up (i.e. vector is zero)?
				if (!collided || remainingMoveVector.moveVector.sqrMagnitude.IsEqualToZero())
				{
					// Are there remaining movement vectors?
					if (m_NextMoveVectorIndex < m_MoveVectors.Count)
					{
						remainingMoveVector = m_MoveVectors[m_NextMoveVectorIndex];
						m_NextMoveVectorIndex++;
					}
					else
					{
						if (!tryToStickToGround || didTryToStickToGround)
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
				if (i == k_MaxMoveIterations - 1)
				{
					Debug.LogWarning(string.Format(
						                 "reached k_MaxMoveIterations!     (remainingMoveVector: {0}, {1}, {2})     " +
						                 "(moveVector: {3}, {4}, {5})     hitCount: {6}",
						                 remainingMoveVector.moveVector.x, remainingMoveVector.moveVector.y,
						                 remainingMoveVector.moveVector.z,
						                 moveVector.x, moveVector.y, moveVector.z,
						                 m_StuckInfo.hitCount));
				}
#endif
			}

			transform.position = virtualPosition;
		}

		// A single movement major step. Returns true when there is collision.
		//		moveVector: The move vector.
		// 		canSlide: Can slide against obstacles?
		// 		tryGrounding: Try grounding the player?
		//		currentPosition: position of the character
		bool MoveMajorStep(ref Vector3 moveVector, bool canSlide, bool tryGrounding, ref Vector3 currentPosition)
		{
			var direction = moveVector.normalized;
			var distance = moveVector.magnitude;
			RaycastHit bigRadiusHitInfo;
			RaycastHit smallRadiusHitInfo;
			bool smallRadiusHit;
			bool bigRadiusHit;

			if (!CapsuleCast(direction, distance, currentPosition, 
			                 out smallRadiusHit, out bigRadiusHit,
			                 out smallRadiusHitInfo, out bigRadiusHitInfo,
			                 Vector3.zero))
			{
				// No collision, so move to the position
				MovePosition(moveVector, null, null, ref currentPosition);

				// Check for penetration
				float penetrationDistance;
				Vector3 penetrationDirection;
				if (GetPenetrationInfo(out penetrationDistance, out penetrationDirection, currentPosition))
				{
					// Push away from obstacles
					MovePosition(penetrationDirection * penetrationDistance, null, null, ref currentPosition);
				}

				// Stop current move loop vector
				moveVector = Vector3.zero;

				return false;
			}

			// Did the big radius not hit an obstacle?
			if (!bigRadiusHit)
			{
				// The small radius hit an obstacle, so character is inside an obstacle
				MoveAwayFromObstacle(ref moveVector, ref smallRadiusHitInfo,
				                     direction, distance,
				                     canSlide,
				                     tryGrounding,
				                     true, ref currentPosition);

				return true;
			}

			// Use the nearest collision point (e.g. to handle cases where 2 or more colliders' edges meet)
			if (smallRadiusHit && smallRadiusHitInfo.distance < bigRadiusHitInfo.distance)
			{
				MoveAwayFromObstacle(ref moveVector, ref smallRadiusHitInfo,
				                     direction, distance,
				                     canSlide,
				                     tryGrounding,
				                     true, ref currentPosition);
				return true;
			}

			MoveAwayFromObstacle(ref moveVector, ref bigRadiusHitInfo,
			                     direction, distance,
			                     canSlide,
			                     tryGrounding,
			                     false, ref currentPosition);

			return true;
		}

		// Can the character perform a step offset?
		// 		moveVector: Horizontal movement vector.
		bool CanStepOffset(Vector3 moveVector)
		{
			var moveVectorMagnitude = moveVector.magnitude;
			var position = transform.position;
			RaycastHit hitInfo;

			// Only step up if there's an obstacle at the character's feet (e.g. do not step when only character's head collides)
			if (!SmallSphereCast(moveVector, moveVectorMagnitude, out hitInfo, Vector3.zero, true, position) &&
			    !BigSphereCast(moveVector, moveVectorMagnitude, position, out hitInfo, Vector3.zero, true))
			{
				return false;
			}

			var upDistance = Mathf.Max(m_StepOffset, k_MinStepOffsetHeight);

			// We only step over obstacles if we can partially fit on it (i.e. fit the capsule's radius)
			var horizontal = moveVector * scaledRadius;
			var horizontalSize = horizontal.magnitude;
			horizontal.Normalize();

			// Any obstacles ahead (after we moved up)?
			var up = Vector3.up * upDistance;
			if (SmallCapsuleCast(horizontal, GetSkinWidth() + horizontalSize, out hitInfo, up, position) ||
			    BigCapsuleCast(horizontal, horizontalSize, out hitInfo, up, position))
			{
				return false;
			}

			return !CheckSteepSlopeAhead(moveVector);
		}

		// Returns true if there's a steep slope ahead.
		//		moveVector: The movement vector.
		// 		alsoCheckForStepOffset: Do a second test where the step offset will move the player to?
		bool CheckSteepSlopeAhead(Vector3 moveVector, bool alsoCheckForStepOffset = true)
		{
			var direction = moveVector.normalized;
			var distance = moveVector.magnitude;

			if (CheckSteepSlopAhead(direction, distance, Vector3.zero))
			{
				return true;
			}

			// Only need to do the second test for human controlled character
			if (!alsoCheckForStepOffset || !m_IsLocalHuman)
			{
				return false;
			}

			// Check above where the step offset will move the player to
			return CheckSteepSlopAhead(direction,
			                           Mathf.Max(distance, k_MinCheckSteepSlopeAheadDistance),
			                           Vector3.up * m_StepOffset);
		}

		// Returns true if there's a steep slope ahead.
		bool CheckSteepSlopAhead(Vector3 direction, float distance, Vector3 offsetPosition)
		{
			RaycastHit bigRadiusHitInfo;
			RaycastHit smallRadiusHitInfo;
			bool smallRadiusHit;
			bool bigRadiusHit;

			if (!CapsuleCast(direction, distance, transform.position, 
			                 out smallRadiusHit, out bigRadiusHit,
			                 out smallRadiusHitInfo, out bigRadiusHitInfo,
			                 offsetPosition))
			{
				// No collision
				return false;
			}

			RaycastHit hitInfoCapsule = (!bigRadiusHit || (smallRadiusHit && smallRadiusHitInfo.distance < bigRadiusHitInfo.distance)) ? 
										smallRadiusHitInfo : 
										bigRadiusHitInfo;

			RaycastHit hitInfoRay;
			var rayOrigin = transform.position + transformedCenter + offsetPosition;

			var offset = Mathf.Clamp(m_SlopeMovementOffset, 0.0f, distance * k_SlopeCheckDistanceMultiplier);
			var rayDirection = (hitInfoCapsule.point + direction * offset) - rayOrigin;

			// Raycast returns a more accurate normal than SphereCast/CapsuleCast
			if (UnityEngine.Physics.Raycast(rayOrigin,
			                                rayDirection,
			                                out hitInfoRay,
			                                rayDirection.magnitude * k_RaycastScaleDistance,
			                                GetCollisionLayerMask(),
			                                m_TriggerQuery) &&
			    hitInfoRay.collider == hitInfoCapsule.collider)
			{
				hitInfoCapsule = hitInfoRay;
			}
			else
			{
				return false;
			}

			var slopeAngle = Vector3.Angle(Vector3.up, hitInfoCapsule.normal);
			var slopeIsSteep = slopeAngle > m_SlopeLimit &&
			                    slopeAngle < k_MaxSlopeLimit &&
			                    Vector3.Dot(direction, hitInfoCapsule.normal) < 0.0f;

			return slopeIsSteep;
		}

		// Split the move vector into horizontal and vertical components. The results are added to the moveVectors list.
		// 		moveVector: The move vector.
		// 		slideWhenMovingDown: Slide against obstacles when moving down? (e.g. we don't want to slide when applying gravity while the character is grounded)
		// 		doNotStepOffset: Do not try to perform the step offset?
		void SplitMoveVector(Vector3 moveVector, bool slideWhenMovingDown, bool doNotStepOffset)
		{
			var horizontal = new Vector3(moveVector.x, 0.0f, moveVector.z);
			var vertical = new Vector3(0.0f, moveVector.y, 0.0f);
			var horizontalIsAlmostZero = IsMoveVectorAlmostZero(horizontal);
			var tempStepOffset = m_StepOffset;
			var doStepOffset = isGrounded &&
			                    !doNotStepOffset &&
			                    !Mathf.Approximately(tempStepOffset, 0.0f) &&
			                    !horizontalIsAlmostZero;

			// Note: Vector is split in this order: up, horizontal, down

			if (vertical.y > 0.0f)
			{
				// Up
				if (horizontal.x.NotEqualToZero() || horizontal.z.NotEqualToZero())
				{
					// Move up then horizontal
					AddMoveVector(vertical, m_SlideAlongCeiling);
					AddMoveVector(horizontal);
				}
				else
				{
					// Move up
					AddMoveVector(vertical, m_SlideAlongCeiling);
				}
			}
			else if (vertical.y < 0.0f)
			{
				// Down
				if (horizontal.x.NotEqualToZero() || horizontal.z.NotEqualToZero())
				{
					if (doStepOffset && CanStepOffset(horizontal))
					{
						// Move up, horizontal then down
						AddMoveVector(Vector3.up * tempStepOffset, false);
						AddMoveVector(horizontal);
						if (slideWhenMovingDown)
						{
							AddMoveVector(vertical);
							AddMoveVector(Vector3.down * tempStepOffset);
						}
						else
						{
							AddMoveVector(vertical + Vector3.down * tempStepOffset);
						}
					}
					else
					{
						// Move horizontal then down
						AddMoveVector(horizontal);
						AddMoveVector(vertical, slideWhenMovingDown);
					}
				}
				else
				{
					// Move down
					AddMoveVector(vertical, slideWhenMovingDown);
				}
			}
			else
			{
				// Horizontal
				if (doStepOffset && CanStepOffset(horizontal))
				{
					// Move up, horizontal then down
					AddMoveVector(Vector3.up * tempStepOffset, false);
					AddMoveVector(horizontal);
					AddMoveVector(Vector3.down * tempStepOffset);
				}
				else
				{
					// Move horizontal
					AddMoveVector(horizontal);
				}
			}
		}

		// Add the movement vector to the moveVectors list.
		// 		moveVector: Move vector to add.
		// 		canSlide: Can the movement slide along obstacles?
		void AddMoveVector(Vector3 moveVector, bool canSlide = true)
		{
			m_MoveVectors.Add(new MoveVector(moveVector, canSlide));
		}

		// Is the movement vector almost zero (i.e. very small)?
		bool IsMoveVectorAlmostZero(Vector3 moveVector)
		{
			return (Mathf.Abs(moveVector.x) > k_SmallMoveVector ||
			    	Mathf.Abs(moveVector.y) > k_SmallMoveVector ||
			    	Mathf.Abs(moveVector.z) > k_SmallMoveVector) ? false : true;
		}

		// Test if character can stick to the ground, and set the down vector if so.
		// 		moveVector: The original movement vector.
		// 		getDownVector: Get the down vector.
		bool CanStickToGround(Vector3 moveVector, out MoveVector getDownVector)
		{
			var moveVectorNoY = new Vector3(moveVector.x, 0.0f, moveVector.z);
			var downDistance = Mathf.Max(moveVectorNoY.magnitude, k_MinStickToGroundDownDistance);
			if (moveVector.y < 0.0f)
			{
				downDistance = Mathf.Max(downDistance, Mathf.Abs(moveVector.y));
			}

			if (downDistance <= k_MaxStickToGroundDownDistance)
			{
				getDownVector = new MoveVector(Vector3.down * downDistance, false);
				return true;
			}

			getDownVector = new MoveVector(Vector3.zero);
			return false;
		}

		// Do two capsule casts. One excluding the capsule's skin width and one including the skin width.
		// 		direction: Direction to cast
		// 		distance: Distance to cast
		//		currentPosition: position of the character
		// 		smallRadiusHit: Did hit, excluding the skin width?
		// 		bigRadiusHit: Did hit, including the skin width?
		// 		smallRadiusHitInfo: Hit info for cast excluding the skin width.
		// 		bigRadiusHitInfo: Hit info for cast including the skin width.
		// 		offsetPosition: Offset position, if we want to test somewhere relative to the capsule's position.
		bool CapsuleCast(Vector3 direction, float distance, Vector3 currentPosition,
		                         out bool smallRadiusHit, out bool bigRadiusHit,
		                         out RaycastHit smallRadiusHitInfo, out RaycastHit bigRadiusHitInfo,
		                         Vector3 offsetPosition)
		{
			// Exclude the skin width in the test
			smallRadiusHit = SmallCapsuleCast(direction, distance, out smallRadiusHitInfo, offsetPosition, currentPosition);

			// Include the skin width in the test
			bigRadiusHit = BigCapsuleCast(direction, distance, out bigRadiusHitInfo, offsetPosition, currentPosition);

			return smallRadiusHit || bigRadiusHit;
		}

		// Do a capsule cast, excluding the skin width.
		//		direction: Direction to cast.
		// 		distance: Distance to cast.
		// 		smallRadiusHitInfo: Hit info.
		// 		offsetPosition: Offset position, if we want to test somewhere relative to the capsule's position.
		//		currentPosition: position of the character
		bool SmallCapsuleCast(Vector3 direction, float distance,
		                              out RaycastHit smallRadiusHitInfo,
		                              Vector3 offsetPosition, Vector3 currentPosition)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
			// when moving almost parallel to an obstacle for small distances).
			var extraDistance = scaledRadius;

			if (UnityEngine.Physics.CapsuleCast(GetTopSphereWorldPosition(currentPosition) + offsetPosition,
			                                    GetBottomSphereWorldPosition(currentPosition) + offsetPosition,
			                                    scaledRadius,
			                                    direction,
			                                    out smallRadiusHitInfo,
			                                    distance + extraDistance,
			                                    GetCollisionLayerMask(),
			                                    m_TriggerQuery))
			{
				return smallRadiusHitInfo.distance <= distance;
			}

			return false;
		}

		// Do a capsule cast, includes the skin width.
		//		direction: Direction to cast.
		// 		distance: Distance to cast.
		// 		bigRadiusHitInfo: Hit info.
		// 		offsetPosition: Offset position, if we want to test somewhere relative to the capsule's position.
		//		currentPosition: position of the character
		bool BigCapsuleCast(Vector3 direction, float distance,
		                            out RaycastHit bigRadiusHitInfo,
		                            Vector3 offsetPosition, Vector3 currentPosition)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
			// when moving almost parallel to an obstacle for small distances).
			var extraDistance = scaledRadius + GetSkinWidth();

			if (UnityEngine.Physics.CapsuleCast(GetTopSphereWorldPosition(currentPosition) + offsetPosition,
			                                    GetBottomSphereWorldPosition(currentPosition) + offsetPosition,
			                                    scaledRadius + GetSkinWidth(),
			                                    direction,
			                                    out bigRadiusHitInfo,
			                                    distance + extraDistance,
			                                    GetCollisionLayerMask(),
			                                    m_TriggerQuery))
			{
				return bigRadiusHitInfo.distance <= distance;
			}

			return false;
		}

		// Do a sphere cast, excludes the skin width. Sphere position is at the top or bottom of the capsule.
		// 		direction: Direction to cast.
		// 		distance: Distance to cast.
		// 		smallRadiusHitInfo: Hit info.
		// 		offsetPosition: Offset position, if we want to test somewhere relative to the capsule's position.
		// 		useBottomSphere: Use the sphere at the bottom of the capsule? If false then use the top sphere.
		//		currentPosition: position of the character
		bool SmallSphereCast(Vector3 direction, float distance,
		                             out RaycastHit smallRadiusHitInfo,
		                             Vector3 offsetPosition,
		                             bool useBottomSphere, Vector3 currentPosition)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
			// when moving almost parallel to an obstacle for small distances).
			var extraDistance = scaledRadius;

			var spherePosition = useBottomSphere ? GetBottomSphereWorldPosition(currentPosition) + offsetPosition 
												 : GetTopSphereWorldPosition(currentPosition) + offsetPosition;
			if (UnityEngine.Physics.SphereCast(spherePosition,
			                                   scaledRadius,
			                                   direction,
			                                   out smallRadiusHitInfo,
			                                   distance + extraDistance,
			                                   GetCollisionLayerMask(),
			                                   m_TriggerQuery))
			{
				return smallRadiusHitInfo.distance <= distance;
			}

			return false;
		}

		// Do a sphere cast, including the skin width. Sphere position is at the top or bottom of the capsule.
		// 		direction">Direction to cast.
		// 		distance">Distance to cast.
		//		currentPosition: position of the character
		// 		bigRadiusHitInfo">Hit info.
		// 		offsetPosition">Offset position, if we want to test somewhere relative to the capsule's position.
		// 		useBottomSphere">Use the sphere at the bottom of the capsule? If false then use the top sphere.
		bool BigSphereCast(Vector3 direction, float distance, Vector3 currentPosition,
		                           out RaycastHit bigRadiusHitInfo,
		                           Vector3 offsetPosition,
		                           bool useBottomSphere)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
			// when moving almost parallel to an obstacle for small distances).
			var extraDistance = scaledRadius + GetSkinWidth();

			var spherePosition = useBottomSphere ? GetBottomSphereWorldPosition(currentPosition) + offsetPosition 
												 : GetTopSphereWorldPosition(currentPosition) + offsetPosition;
			if (UnityEngine.Physics.SphereCast(spherePosition,
			                                   scaledRadius + GetSkinWidth(),
			                                   direction,
			                                   out bigRadiusHitInfo,
			                                   distance + extraDistance,
			                                   GetCollisionLayerMask(),
			                                   m_TriggerQuery))
			{
				return bigRadiusHitInfo.distance <= distance;
			}

			return false;
		}

		// Called when a capsule cast detected an obstacle. Move away from the obstacle and slide against it if needed.
		// 		moveVector: The movement vector.
		// 		hitInfoCapsule: Hit info of the capsule cast collision.
		// 		direction: Direction of the cast.
		// 		distance: Distance of the cast.
		// 		canSlide: Can slide against obstacles?
		// 		tryGrounding: Try grounding the player?
		// 		hitSmallCapsule: Did the collision occur with the small capsule (i.e. no skin width)?
		//		currentPosition: position of the character
		void MoveAwayFromObstacle(ref Vector3 moveVector, ref RaycastHit hitInfoCapsule,
		                                  Vector3 direction, float distance,
		                                  bool canSlide,
		                                  bool tryGrounding,
		                                  bool hitSmallCapsule, ref Vector3 currentPosition)
		{
			// IMPORTANT: This method must set moveVector.

			// When the small capsule hit then stop skinWidth away from obstacles
			var collisionOffset = hitSmallCapsule ? GetSkinWidth() : k_CollisionOffset;

			var hitDistance = Mathf.Max(hitInfoCapsule.distance - collisionOffset, 0.0f);
			// Note: remainingDistance is more accurate is we use hitDistance, but using hitInfoCapsule.distance gives a tiny 
			// bit of dampening when sliding along obstacles
			var remainingDistance = Mathf.Max(distance - hitInfoCapsule.distance, 0.0f);

			// Move to the collision point
			MovePosition(direction * hitDistance, direction, hitInfoCapsule, ref currentPosition);

			Vector3 hitNormal;
			RaycastHit hitInfoRay;
			var rayOrigin = currentPosition + transformedCenter;
			var rayDirection = hitInfoCapsule.point - rayOrigin;

			// Raycast returns a more accurate normal than SphereCast/CapsuleCast
			// Using angle <= k_MaxAngleToUseRaycastNormal gives a curve when collision is near an edge.
			if (UnityEngine.Physics.Raycast(rayOrigin,
			                                rayDirection,
			                                out hitInfoRay,
			                                rayDirection.magnitude * k_RaycastScaleDistance,
			                                GetCollisionLayerMask(),
			                                m_TriggerQuery) &&
			    hitInfoRay.collider == hitInfoCapsule.collider &&
			    Vector3.Angle(hitInfoCapsule.normal, hitInfoRay.normal) <= k_MaxAngleToUseRaycastNormal)
			{
				hitNormal = hitInfoRay.normal;
			}
			else
			{
				hitNormal = hitInfoCapsule.normal;
			}

			float penetrationDistance;
			Vector3 penetrationDirection;

			if (GetPenetrationInfo(out penetrationDistance, out penetrationDirection, currentPosition, true, null, hitInfoCapsule))
			{
				// Push away from the obstacle
				MovePosition(penetrationDirection * penetrationDistance, null, null, ref currentPosition);
			}

			var slopeIsSteep = false;
			if (tryGrounding || m_StuckInfo.isStuck)
			{
				// No further movement when grounding the character, or the character is stuck
				canSlide = false;
			}
			else if (moveVector.x.NotEqualToZero() || moveVector.z.NotEqualToZero())
			{
				// Test if character is trying to walk up a steep slope
				var slopeAngle = Vector3.Angle(Vector3.up, hitNormal);
				slopeIsSteep = slopeAngle > m_SlopeLimit && slopeAngle < k_MaxSlopeLimit && Vector3.Dot(direction, hitNormal) < 0.0f;
			}

			// Set moveVector
			if (canSlide && remainingDistance > 0.0f)
			{
				var slideNormal = hitNormal;

				if (slopeIsSteep && slideNormal.y > 0.0f)
				{
					// Do not move up the slope
					slideNormal.y = 0.0f;
					slideNormal.Normalize();
				}

				// Vector to slide along the obstacle
				var project = Vector3.Cross(direction, slideNormal);
				project = Vector3.Cross(slideNormal, project);

				if (slopeIsSteep && project.y > 0.0f)
				{
					// Do not move up the slope
					project.y = 0.0f;
				}

				project.Normalize();

				// Slide along the obstacle
				var isWallSlowingDown = m_SlowAgainstWalls && m_MinSlowAgainstWallsAngle < 90.0f;

				if (isWallSlowingDown)
				{
					// Cosine of angle between the movement direction and the tangent is equivalent to the sin of
					// the angle between the movement direction and the normal, which is the sliding component of
					// our movement.
					var cosine = Vector3.Dot(project, direction);
					var slowDownFactor = Mathf.Clamp01(cosine * m_InvRescaleFactor);
					
					moveVector = project * (remainingDistance * slowDownFactor);
				}
				else
				{
					// No slow down, keep the same speed even against walls.
					moveVector = project * remainingDistance;
				}
			}
			else
			{
				// Stop current move loop vector
				moveVector = Vector3.zero;
			}

			if (direction.y < 0.0f && Mathf.Approximately(direction.x, 0.0f) && Mathf.Approximately(direction.z, 0.0f))
			{
				// This is used by the sliding down slopes
				m_DownCollisionNormal = hitNormal;
			}
		}

		// Check for collision penetration, then try to de-penetrate if there is collision.
		bool Depenetrate(ref Vector3 currentPosition)
		{
			float distance;
			Vector3 direction;
			if (GetPenetrationInfo(out distance, out direction, currentPosition))
			{
				MovePosition(direction * distance, null, null, ref currentPosition);
				return true;
			}

			return false;
		}

		// Get direction and distance to move out of the obstacle.
		// 		getDistance: Get distance to move out of the obstacle.
		// 		getDirection: Get direction to move out of the obstacle.
		//		currentPosition: position of the character
		// 		includeSkinWidth: Include the skin width in the test?
		// 		offsetPosition: Offset position, if we want to test somewhere relative to the capsule's position.
		// 		hitInfo: The hit info.
		bool GetPenetrationInfo(out float getDistance, out Vector3 getDirection, Vector3 currentPosition,
		                                bool includeSkinWidth = true,
		                                Vector3? offsetPosition = null,
		                                RaycastHit? hitInfo = null)
		{
			getDistance = 0.0f;
			getDirection = Vector3.zero;

			var offset = offsetPosition != null ? offsetPosition.Value : Vector3.zero;
			var tempSkinWidth = includeSkinWidth ? GetSkinWidth() : 0.0f;
			var overlapCount = UnityEngine.Physics.OverlapCapsuleNonAlloc(GetTopSphereWorldPosition(currentPosition) + offset,
			                                                              GetBottomSphereWorldPosition(currentPosition) + offset,
			                                                              scaledRadius + tempSkinWidth,
			                                                              m_PenetrationInfoColliders,
			                                                              GetCollisionLayerMask(),
			                                                              m_TriggerQuery);
			if (overlapCount <= 0 || m_PenetrationInfoColliders.Length <= 0)
			{
				return false;
			}

			var result = false;
			var localPos = Vector3.zero;
			for (var i = 0; i < overlapCount; i++)
			{
				var collider = m_PenetrationInfoColliders[i];
				if (collider == null)
				{
					break;
				}

				Vector3 direction;
				float distance;
				var colliderTransform = collider.transform;
				if (ComputePenetration(offset,
				                       collider, colliderTransform.position, colliderTransform.rotation,
				                       out direction, out distance, includeSkinWidth, currentPosition))
				{
					localPos += direction * (distance + k_CollisionOffset);
					result = true;
				}
				else if (hitInfo != null && hitInfo.Value.collider == collider)
				{
					// We can use the hit normal to push away from the collider, because CapsuleCast generally returns a normal
					// that pushes away from the collider.
					localPos += hitInfo.Value.normal * k_CollisionOffset;
					result = true;
				}
			}

			if (result)
			{
				getDistance = localPos.magnitude;
				getDirection = localPos.normalized;
			}

			return result;
		}

		// Check if any colliders overlap the capsule.
		// 		includeSkinWidth: Include the skin width in the test?
		// 		offsetPosition: Offset position, if we want to test somewhere relative to the capsule's position.
		bool CheckCapsule(Vector3 currentPosition, bool includeSkinWidth = true,
		                          Vector3? offsetPosition = null)
		{
			var offset = offsetPosition != null ? offsetPosition.Value : Vector3.zero;
			var tempSkinWidth = includeSkinWidth ? GetSkinWidth() : 0.0f;
			return UnityEngine.Physics.CheckCapsule(GetTopSphereWorldPosition(currentPosition) + offset,
			                                        GetBottomSphereWorldPosition(currentPosition) + offset,
			                                        scaledRadius + tempSkinWidth,
			                                        GetCollisionLayerMask(),
			                                        m_TriggerQuery);
		}

		// Move the capsule position.
		// 		moveVector: Move vector.
		// 		collideDirection: Direction we encountered collision. Null if no collision.
		//		hitInfo: Hit info of the collision. Null if no collision.
		//		currentPosition: position of the character
		void MovePosition(Vector3 moveVector, Vector3? collideDirection, RaycastHit? hitInfo, ref Vector3 currentPosition)
		{
			if (moveVector.sqrMagnitude.NotEqualToZero())
			{
				currentPosition += moveVector;
			}

			if (collideDirection != null && hitInfo != null)
			{
				UpdateCollisionInfo(collideDirection.Value, hitInfo.Value, currentPosition);
			}
		}

		// Update the collision flags and info.
		// 		direction: The direction moved.
		//		hitInfo: The hit info of the collision.
		//		currentPosition: position of the character
		void UpdateCollisionInfo(Vector3 direction, RaycastHit? hitInfo, Vector3 currentPosition)
		{
			if (direction.x.NotEqualToZero() || direction.z.NotEqualToZero())
			{
				collisionFlags |= CollisionFlags.Sides;
			}

			if (direction.y > 0.0f)
			{
				collisionFlags |= CollisionFlags.CollidedAbove;
			}
			else if (direction.y < 0.0f)
			{
				collisionFlags |= CollisionFlags.CollidedBelow;
			}

			m_StuckInfo.hitCount++;

			if (hitInfo != null)
			{
				var collider = hitInfo.Value.collider;
				
				// We only care about the first collision with a collider
				if (!m_CollisionInfoDictionary.ContainsKey(collider))
				{
					var moved = currentPosition - m_StartPosition;
					var newCollisionInfo = new CollisionInfo(this, hitInfo.Value, direction, moved.magnitude);
					m_CollisionInfoDictionary.Add(collider, newCollisionInfo);
				}
			}
		}

		// Stop auto-slide down steep slopes.
		void StopSlideDownSlopes()
		{
			m_SlidingDownSlopeTime = 0.0f;
		}

		// Auto-slide down steep slopes.
		void UpdateSlideDownSlopes()
		{
			var dt = Time.deltaTime;
			if (!UpdateSlideDownSlopesInternal(dt))
			{
				if (isSlidingDownSlope)
				{
					m_SlidingDownSlopeTime += dt;
					m_DelayStopSlidingDownSlopeTime += dt;

					// Slight delay before we stop sliding down slopes. To handle cases where sliding test fails for a few frames.
					if (m_DelayStopSlidingDownSlopeTime > k_StopSlideDownSlopeDelay)
					{
						StopSlideDownSlopes();
					}
				}
				else
				{
					StopSlideDownSlopes();
				}
			}
			else
			{
				m_DelayStopSlidingDownSlopeTime = 0.0f;
			}
		}

		// Auto-slide down steep slopes.
		bool UpdateSlideDownSlopesInternal(float dt)
		{
			if (!m_SlideDownSlopes || !isGrounded)
			{
				return false;
			}

			Vector3 hitNormal;

			// Collided downwards during the last slide movement?
			if (isSlidingDownSlope && m_DownCollisionNormal != null)
			{
				hitNormal = m_DownCollisionNormal.Value;
			}
			else
			{
				RaycastHit hitInfoSphere;
				if (!SmallSphereCast(Vector3.down,
				                     GetSkinWidth() + k_SlideDownSlopeTestDistance,
				                     out hitInfoSphere,
				                     Vector3.zero,
				                     true, transform.position))
				{
					return false;
				}

				RaycastHit hitInfoRay;
				var rayOrigin = GetBottomSphereWorldPosition(transform.position);
				var rayDirection = hitInfoSphere.point - rayOrigin;

				// Raycast returns a more accurate normal than SphereCast/CapsuleCast
				if (UnityEngine.Physics.Raycast(rayOrigin,
				                                rayDirection,
				                                out hitInfoRay,
				                                rayDirection.magnitude * k_RaycastScaleDistance,
				                                GetCollisionLayerMask(),
				                                m_TriggerQuery) &&
				    hitInfoRay.collider == hitInfoSphere.collider)
				{
					hitNormal = hitInfoRay.normal;
				}
				else
				{
					hitNormal = hitInfoSphere.normal;
				}
			}

			var slopeAngle = Vector3.Angle(Vector3.up, hitNormal);
			var slopeIsSteep = slopeAngle > m_SlopeLimit;
			if (!slopeIsSteep || slopeAngle >= k_MaxSlopeSlideAngle)
			{
				return false;
			}

			var didSlide = true;
			m_SlidingDownSlopeTime += dt;

			// Pro tip: Here you can also use the friction of the physics material of the slope, to adjust the slide speed.

			// Speed increases as slope angle increases
			var slideSpeedScale = Mathf.Clamp01(slopeAngle / k_MaxSlopeSlideAngle);

			// Apply gravity and slide along the obstacle
			var gravity = Mathf.Abs(UnityEngine.Physics.gravity.y) * m_SlideGravityScale * slideSpeedScale;
			var verticalVelocity = Mathf.Clamp(gravity * m_SlidingDownSlopeTime, 0.0f, Mathf.Abs(m_SlideMaxSpeed));
			var moveVector = new Vector3(0.0f, -verticalVelocity, 0.0f) * dt;

			// Push slightly away from the slope
			var push = new Vector3(hitNormal.x, 0.0f, hitNormal.z).normalized * k_PushAwayFromSlopeDistance;
			moveVector = new Vector3(push.x, moveVector.y, push.z);

			// Preserve collision flags and velocity. Because user expects them to only be set when manually calling Move/SimpleMove.
			var oldCollisionFlags = collisionFlags;
			var oldVelocity = m_Velocity;

			MoveInternal(moveVector, true, true, true);
			if ((collisionFlags & CollisionFlags.CollidedSides) != 0)
			{
				// Stop sliding when hit something on the side
				didSlide = false;
			}

			collisionFlags = oldCollisionFlags;
			m_Velocity = oldVelocity;

			return didSlide;
		}

		// Update pending height and center when it is safe.
		void UpdatePendingHeightAndCenter()
		{
			if (m_PendingResize.heightTime == null && m_PendingResize.centerTime == null)
			{
				return;
			}

			// Use smallest time
			var time = m_PendingResize.heightTime != null ? m_PendingResize.heightTime.Value : float.MaxValue;
			time = Mathf.Min(time, m_PendingResize.centerTime != null ? m_PendingResize.centerTime.Value : float.MaxValue);
			if (time > Time.time)
			{
				return;
			}

			m_PendingResize.ClearTimers();

			if (m_PendingResize.height != null && m_PendingResize.center != null)
			{
				SetHeightAndCenter(m_PendingResize.height.Value, m_PendingResize.center.Value, true, false);
			}
			else if (m_PendingResize.height != null)
			{
				SetHeight(m_PendingResize.height.Value, false, true, false);
			}
			else if (m_PendingResize.center != null)
			{
				SetCenter(m_PendingResize.center.Value, true, false);
			}
		}

		// Sets the playerRootTransform's localPosition to the rootTransformOffset
		void SetRootToOffset()
		{
			if (m_PlayerRootTransform != null)
			{
				m_PlayerRootTransform.localPosition = m_RootTransformOffset;
			}
		}
	}
}