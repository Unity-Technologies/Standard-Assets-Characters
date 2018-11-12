using System;
using System.Collections.Generic;
using StandardAssets.Characters.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

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
		/// Collision info used by the OpenCharacterController and sent to the OnOpenCharacterControllerHit message.
		/// </summary>
		public struct CollisionInfo
		{
			/// <summary>
			/// The collider that was hit by the controller.
			/// </summary>
			readonly Collider m_Collider;

			/// <summary>
			/// The controller that hit the collider.
			/// </summary>
			readonly OpenCharacterController m_Controller;

			/// <summary>
			/// The game object that was hit by the controller.
			/// </summary>
			readonly GameObject m_GameObject;

			/// <summary>
			/// The direction the character Controller was moving in when the collision occured.
			/// </summary>
			readonly Vector3 m_MoveDirection;

			/// <summary>
			/// How far the character has travelled until it hit the collider.
			/// </summary>
			readonly float m_MoveLength;

			/// <summary>
			/// The normal of the surface we collided with in world space.
			/// </summary>
			readonly Vector3 m_Normal;

			/// <summary>
			/// The impact point in world space.
			/// </summary>
			readonly Vector3 m_Point;

			/// <summary>
			/// The rigidbody that was hit by the controller.
			/// </summary>
			readonly Rigidbody m_Rigidbody;

			/// <summary>
			/// The transform that was hit by the controller.
			/// </summary>
			readonly Transform m_Transform;

			public Collider collider
			{
				get { return m_Collider; }
			}

			public OpenCharacterController controller
			{
				get { return m_Controller; }
			}

			public GameObject gameObject
			{
				get { return m_GameObject; }
			}

			public Vector3 moveDirection
			{
				get { return m_MoveDirection; }
			}

			public float moveLength
			{
				get { return m_MoveLength; }
			}

			public Vector3 normal
			{
				get { return m_Normal; }
			}

			public Vector3 point
			{
				get { return m_Point; }
			}

			public Rigidbody rigidbody
			{
				get { return m_Rigidbody; }
			}

			public Transform transform
			{
				get { return m_Transform; }
			}

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

		/// <summary>
		/// A vector used by the OpenCharacterController.
		/// </summary>
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

#if UNITY_EDITOR
			public Vector3 debugOriginalVector { get; set; }
#endif

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

#if UNITY_EDITOR
				debugOriginalVector = newMoveVector;
#endif
			}
		}

		/// <summary>
		/// Resize info for OpenCharacterController (e.g. delayed resizing until it is safe to resize).
		/// </summary>
		class ResizeInfo
		{
			/// <summary>
			/// Intervals (seconds) in which to check if the capsule's height/center must be changed.
			/// </summary>
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

		/// <summary>
		/// Stuck info and logic used by the OpenCharacterController.
		/// </summary>
		class StuckInfo
		{
			/// <summary>
			/// If character's position does not change by more than this amount then we assume the character is stuck.
			/// </summary>
			const float k_StuckDistance = 0.001f;

			/// <summary>
			/// If character's position does not change by more than this amount then we assume the character is stuck.
			/// </summary>
			const float k_StuckSqrDistance = k_StuckDistance * k_StuckDistance;

			/// <summary>
			/// If character collided this number of times during the movement loop then test if character is stuck by
			/// examining the position
			/// </summary>
			const int k_HitCountForStuck = 6;

			/// <summary>
			/// Assume character is stuck if the position is the same for longer than this number of loop iterations
			/// </summary>
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
			/// For keeping track of the character's position, to determine when the character gets stuck.
			/// </summary>
			Vector3? m_StuckPosition;

			/// <summary>
			/// Count how long the character is in the same position.
			/// </summary>
			int m_StuckPositionCount;

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

		/// <summary>
		/// Max slope limit.
		/// </summary>
		const float k_MaxSlopeLimit = 90.0f;

		/// <summary>
		/// Max slope angle on which character can slide down automatically.
		/// </summary>
		const float k_MaxSlopeSlideAngle = 90.0f;

		/// <summary>
		/// Distance to test for ground when sliding down slopes.
		/// </summary>
		const float k_SlideDownSlopeTestDistance = 1.0f;

		/// <summary>
		/// Slight delay before we stop sliding down slopes. To handle cases where sliding test fails for a few frames.
		/// </summary>
		const float k_StopSlideDownSlopeDelay = 0.5f;

		/// <summary>
		/// Distance to push away from slopes when sliding down them.
		/// </summary>
		const float k_PushAwayFromSlopeDistance = 0.001f;

		/// <summary>
		/// Minimum distance to use when checking ahead for steep slopes, when checking if it's safe to do the step offset.
		/// </summary>
		const float k_MinCheckSteepSlopeAheadDistance = 0.2f;

		/// <summary>
		/// Min skin width.
		/// </summary>
		const float k_MinSkinWidth = 0.0001f;

		/// <summary>
		/// The maximum move iterations. Mainly used as a fail safe to prevent an infinite loop.
		/// </summary>
		const int k_MaxMoveIterations = 20;

		/// <summary>
		/// Stick to the ground if it is less than this distance from the character.
		/// </summary>
		const float k_MaxStickToGroundDownDistance = 1.0f;

		/// <summary>
		/// Min distance to test for the ground when sticking to the ground.
		/// </summary>
		const float k_MinStickToGroundDownDistance = 0.01f;

		/// <summary>
		/// Max colliders to use in the overlap methods.
		/// </summary>
		const int k_MaxOverlapColliders = 10;

		/// <summary>
		/// Offset to use when moving to a collision point, to try to prevent overlapping the colliders
		/// </summary>
		const float k_CollisionOffset = 0.001f;

		/// <summary>
		/// Distance to test beneath the character when doing the grounded test
		/// </summary>
		const float k_GroundedTestDistance = 0.001f;

		/// <summary>
		/// Minimum distance to move. This minimizes small penetrations and inaccurate casts (e.g. into the floor)
		/// </summary>
		const float k_MinMoveDistance = 0.0001f;

		/// <summary>
		/// Minimum sqr distance to move. This minimizes small penetrations and inaccurate casts (e.g. into the floor)
		/// </summary>
		const float k_MinMoveSqrDistance = k_MinMoveDistance * k_MinMoveDistance;

		/// <summary>
		/// Minimum step offset height to move (if character has a step offset).
		/// </summary>
		const float k_MinStepOffsetHeight = k_MinMoveDistance;

		/// <summary>
		/// Small value to test if the movement vector is small.
		/// </summary>
		const float k_SmallMoveVector = 1e-6f;

		/// <summary>
		/// If angle between raycast and capsule/sphere cast normal is less than this then use the raycast normal,
		/// which is more accurate.
		/// </summary>
		const float k_MaxAngleToUseRaycastNormal = 5.0f;

		/// <summary>
		/// Scale the capsule/sphere hit distance when doing the additional raycast to get a more accurate normal
		/// </summary>
		const float k_RaycastScaleDistance = 2.0f;

		/// <summary>
		/// Fired when the grounded state changed.
		/// </summary>
		public event Action<bool> onGroundedChanged;

		/// <summary>
		/// Fired when the velocity changed.
		/// </summary>
		public event Action<Vector3> onVelocityChanged;

		/// <summary>
		/// Fired when the collision flags changed.
		/// </summary>
		public event Action<CollisionFlags> onCollisionFlagsChanged;

		/// <summary>
		/// The root bone in the avatar.
		/// </summary>
		[FormerlySerializedAs("playerRootTransform")]
		[Header("Player Root")]
		[Tooltip("The root bone in the avatar.")]
		[SerializeField]
		Transform m_PlayerRootTransform;

		/// <summary>
		/// The root transform will be positioned at this offset.
		/// </summary>
		[FormerlySerializedAs("rootTransformOffset")]
		[Tooltip("The root transform will be positioned at this offset.")]
		[SerializeField]
		Vector3 m_RootTransformOffset = new Vector3(0, 0, 0);

		/// <summary>
		/// Limits the collider to only climb slopes that are less steep (in degrees) than the indicated value.
		/// </summary>
		[FormerlySerializedAs("slopeLimit")]
		[Header("Collision")]
		[Tooltip("Limits the collider to only climb slopes that are less steep (in degrees) than the indicated value.")]
		[SerializeField]
		float m_SlopeLimit = 45.0f;

		/// <summary>
		/// The character will step up a stair only if it is closer to the ground than the indicated value.
		/// This should not be greater than the Character Controller’s height or it will generate an error.
		/// Generally this should be kept as small as possible.
		/// </summary>
		[FormerlySerializedAs("stepOffset")]
		[Tooltip("The character will step up a stair only if it is closer to the ground than the indicated value. " +
		         "This should not be greater than the Character Controller’s height or it will generate an error. " +
		         "Generally this should be kept as small as possible.")]
		[SerializeField]
		float m_StepOffset = 0.3f;

		/// <summary>
		/// Two colliders can penetrate each other as deep as their Skin Width. Larger Skin Widths reduce jitter.
		/// Low Skin Width can cause the character to get stuck. A good setting is to make this value 10% of the Radius.
		/// </summary>
		[FormerlySerializedAs("skinWidth")]
		[Tooltip(
			"Two colliders can penetrate each other as deep as their Skin Width. Larger Skin Widths reduce jitter. " +
			"Low Skin Width can cause the character to get stuck. A good setting is to make this value 10% of the Radius.")]
		[SerializeField]
		float m_SkinWidth = 0.08f;

		/// <summary>
		/// If the character tries to move below the indicated value, it will not move at all. This can be used to reduce jitter.
		/// In most situations this value should be left at 0.
		/// </summary>
		[FormerlySerializedAs("minMoveDistance")]
		[Tooltip(
			"If the character tries to move below the indicated value, it will not move at all. This can be used to reduce jitter. " +
			"In most situations this value should be left at 0.")]
		[SerializeField]
		float m_MinMoveDistance = 0.0f;

		/// <summary>
		/// This will offset the Capsule Collider in world space, and won’t affect how the Character pivots.
		/// Ideally, x and z should be zero to avoid rotating into another collider.
		/// </summary>
		[FormerlySerializedAs("center")]
		[Tooltip("This will offset the Capsule Collider in world space, and won’t affect how the Character pivots. " +
		         "Ideally, x and z should be zero to avoid rotating into another collider.")]
		[SerializeField]
		Vector3 m_Center;

		/// <summary>
		/// Length of the Capsule Collider’s radius. This is essentially the width of the collider.
		/// </summary>
		[FormerlySerializedAs("radius")]
		[Tooltip("Length of the Capsule Collider’s radius. This is essentially the width of the collider.")]
		[SerializeField]
		float m_Radius = 0.5f;

		/// <summary>
		/// The Character’s Capsule Collider height. It should be at least double the radius.
		/// </summary>
		[FormerlySerializedAs("height")]
		[Tooltip("The Character’s Capsule Collider height. It should be at least double the radius.")]
		[SerializeField]
		float m_Height = 2.0f;

		/// <summary>
		/// Layers to test for collision.
		/// </summary>
		[FormerlySerializedAs("collisionLayerMask")]
		[Tooltip("Layers to test for collision.")]
		[SerializeField]
		LayerMask m_CollisionLayerMask = ~0; // ~0 sets it to Everything

		/// <summary>
		/// Is the character controlled by a local human? If true then more calculations are done for more accurate movement.
		/// </summary>
		[FormerlySerializedAs("localHumanControlled")]
		[Tooltip("Is the character controlled by a local human? If true then more calculations are done for more " +
		         "accurate movement.")]
		[SerializeField]
		bool m_LocalHumanControlled = true;

		/// <summary>
		/// Can character slide vertically when touching the ceiling? (For example, if ceiling is sloped.)
		/// </summary>
		[FormerlySerializedAs("canSlideAgainstCeiling")]
		[Tooltip("Can character slide vertically when touching the ceiling? (For example, if ceiling is sloped.)")]
		[SerializeField]
		bool m_CanSlideAgainstCeiling = true;

		/// <summary>
		/// Send "OnOpenCharacterControllerHit" messages to game objects? Messages are sent when the character hits a collider while performing a move.
		/// </summary>
		[FormerlySerializedAs("sendColliderHitMessages")]
		[Tooltip(
			"Send \"OnOpenCharacterControllerHit\" messages to game objects? Messages are sent when the character " +
			"hits a collider while performing a move. WARNING: This does create garbage.")]
		[SerializeField]
		bool m_SendColliderHitMessages;

		/// <summary>
		/// Should cast queries hit trigger colliders?
		/// </summary>
		[FormerlySerializedAs("queryTriggerInteraction")]
		[Tooltip("Should cast queries hit trigger colliders?")]
		[SerializeField]
		QueryTriggerInteraction m_QueryTriggerInteraction = QueryTriggerInteraction.Ignore;

		/// <summary>
		/// Slide down slopes when their angle is more than the slope limit?
		/// </summary>
		[FormerlySerializedAs("slideDownSlopes")]
		[Header("Slide Down Slopes")]
		[Tooltip("Slide down slopes when their angle is more than the slope limit?")]
		[SerializeField]
		bool m_SlideDownSlopes = true;

		/// <summary>
		/// The maximum speed that the character can slide downwards
		/// </summary>
		[FormerlySerializedAs("slideDownTerminalVelocity")]
		[Tooltip("The maximum speed that the character can slide downwards")]
		[SerializeField]
		float m_SlideDownTerminalVelocity = 10.0f;

		/// <summary>
		/// Scale gravity when sliding down slopes.
		/// </summary>
		[FormerlySerializedAs("slideDownGravityScale")]
		[Tooltip("Scale gravity when sliding down slopes.")]
		[SerializeField]
		float m_SlideDownGravityScale = 1.0f;

		/// <summary>
		/// The time after initiating a slide classified as a slide start. Used to disable jumping.
		/// </summary>
		[FormerlySerializedAs("slideDownStartDuration")]
		[Tooltip("The time after initiating a slide classified as a slide start. Used to disable jumping.")]
		[SerializeField]
		float m_SlideDownStartDuration = 0.25f;

		/// <summary>
		/// The capsule collider.
		/// </summary>
		CapsuleCollider m_CapsuleCollider;

		/// <summary>
		/// Cached reference to the transform.
		/// </summary>
		Transform m_CachedTransform;

		/// <summary>
		/// The position at the start of the movement.
		/// </summary>
		Vector3 m_StartPosition;

		/// <summary>
		/// Movement vectors used in the move loop.
		/// </summary>
		List<MoveVector> m_MoveVectors = new List<MoveVector>();

		/// <summary>
		/// Next index in the moveVectors list.
		/// </summary>
		int m_NextMoveVectorIndex;

		/// <summary>
		/// Surface normal of the last collision while moving down.
		/// </summary>
		Vector3? m_DownCollisionNormal;

		/// <summary>
		/// Stuck info.
		/// </summary>
		StuckInfo m_StuckInfo = new StuckInfo();

		/// <summary>
		/// The collision info when hitting colliders.
		/// </summary>
		Dictionary<Collider, CollisionInfo> m_CollisionInfoDictionary =
			new Dictionary<Collider, CollisionInfo>();

		/// <summary>
		/// Slight delay before stopping the sliding down slopes. 
		/// </summary>
		float m_DelayStopSlidingDownSlopeTime;

		/// <summary>
		/// Pending resize info to set when it is safe to do so.
		/// </summary>
		readonly ResizeInfo m_PendingResize = new ResizeInfo();

		/// <summary>
		/// Collider array used for <see cref="UnityEngine.Physics.OverlapCapsuleNonAlloc"/> in <see cref="GetPenetrationInfo"/>
		/// </summary>	
		readonly Collider[] m_PenetrationInfoColliders = new Collider[k_MaxOverlapColliders];

		/// <summary>
		/// Is the character on the ground? This is updated during Move or SetPosition.
		/// </summary>
		public bool isGrounded { get; private set; }

		/// <summary>
		/// Collision flags from the last move.
		/// </summary>
		public CollisionFlags collisionFlags { get; private set; }

		/// <summary>
		/// Velocity of the last movement. It's the new position minus the old position.
		/// </summary>
		public Vector3 velocity { get; private set; }

		/// <summary>
		/// Default height of the capsule (e.g. for resetting it).
		/// </summary>
		public float defaultHeight { get; private set; }

		/// <summary>
		/// Default center of the capsule (e.g. for resetting it).
		/// </summary>
		public Vector3 defaultCenter { get; private set; }

		/// <summary>
		/// Is character busy sliding down a steep slope?
		/// </summary>
		public bool isSlidingDownSlope
		{
			get { return slidingDownSlopeTime > 0.0f; }
		}

		/// <summary>
		/// Is the character sliding and has been sliding less than slideDownTimeUntilJumAllowed
		/// </summary>
		public bool startedSlide
		{
			get { return isSlidingDownSlope && slidingDownSlopeTime <= m_SlideDownStartDuration; }
		}

		/// <summary>
		/// How long has character been sliding down a steep slope? (Zero means not busy sliding.)
		/// </summary>
		public float slidingDownSlopeTime { get; private set; }

		/// <summary>
		/// The capsule center with scaling and rotation applied.
		/// </summary>
		public Vector3 transformedCenter
		{
			get { return m_CachedTransform.TransformVector(m_Center); }
		}

		/// <summary>
		/// The capsule radius with the relevant scaling applied (e.g. if object scale is not 1,1,1)
		/// </summary>
		public float scaledRadius
		{
			get
			{
				var scale = m_CachedTransform.lossyScale;
				var maxScale = Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);
				return m_Radius * maxScale;
			}
		}

		/// <summary>
		/// The capsule height with the relevant scaling applied (e.g. if object scale is not 1,1,1)
		/// </summary>
		public float scaledHeight
		{
			get { return m_Height * m_CachedTransform.lossyScale.y; }
		}

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
		/// Move the character. Velocity along the y-axis is ignored. Speed is in units/s. Gravity is automatically applied.
		/// Returns true if the character is grounded. The method will also apply delta time to the speed.
		/// </summary>
		/// <param name="speed">Move with this speed.</param>
		/// <returns>Whether the character is grounded.</returns>
		public bool SimpleMove(Vector3 speed)
		{
			// Reminder: Time.deltaTime returns the fixed delta time when called from inside FixedUpdate.
			var moveVector =
				new Vector3(speed.x, speed.y + UnityEngine.Physics.gravity.y, speed.z) * Time.deltaTime;

			MoveInternal(moveVector, false);
			return isGrounded;
		}

		/// <summary>
		/// Set the position of the character.
		/// </summary>
		/// <param name="position">Position to set.</param>
		/// <param name="updateGrounded">Update the grounded state? This uses a cast, so only set it to true if you need it.</param>
		public void SetPosition(Vector3 position, bool updateGrounded)
		{
			m_CachedTransform.position = position;

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
		/// <returns>True if found penetration.</returns>
		public bool ComputePenetration(Vector3 positionOffset,
		                               Collider collider, Vector3 colliderPosition, Quaternion colliderRotation,
		                               out Vector3 direction, out float distance,
		                               bool includeSkinWidth)
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
			                                                     m_CachedTransform.position + positionOffset,
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
		/// <returns>True if collision occurred.</returns>
		public bool CheckCollisionBelow(float distance, out RaycastHit hitInfo,
		                                Vector3 offsetPosition,
		                                bool useSphereCast = false,
		                                bool useSecondSphereCast = false,
		                                bool adjustPositionSlightly = false)
		{
			var didCollide = false;
			var extraDistance = adjustPositionSlightly
				                      ? k_CollisionOffset
				                      : 0.0f;
			if (!useSphereCast)
			{
				if (UnityEngine.Physics.Raycast(GetFootWorldPosition() + offsetPosition + Vector3.up * extraDistance,
				                                Vector3.down,
				                                out hitInfo,
				                                distance + extraDistance,
				                                GetCollisionLayerMask(),
				                                m_QueryTriggerInteraction))
				{
					didCollide = true;
					hitInfo.distance = Mathf.Max(0.0f, hitInfo.distance - extraDistance);
				}
			}
			else
			{
				if (SmallSphereCast(Vector3.down,
				                    GetSkinWidth() + distance,
				                    out hitInfo,
				                    offsetPosition,
				                    true))
				{
					didCollide = true;
					hitInfo.distance = Mathf.Max(0.0f, hitInfo.distance - GetSkinWidth());
				}

				if (!didCollide &&
				    useSecondSphereCast)
				{
					if (BigSphereCast(Vector3.down,
					                  distance + extraDistance,
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
		/// Check for collision above the character, using a ray or sphere cast.
		/// </summary>
		/// <param name="distance">Distance to check.</param>
		/// <param name="hitInfo">Get the hit info.</param>
		/// <param name="offsetPosition">Position offset. If we want to do a cast relative to the character's current position.</param>
		/// <param name="useSphereCast">Use a sphere cast? If false then use a ray cast.</param>
		/// <param name="useSecondSphereCast">The second cast includes the skin width. Ideally only needed for human controlled player, for more accuracy.</param>
		/// <param name="adjustPositionSlightly">Adjust position slightly down, in case it's already inside an obstacle.</param>
		/// <returns>True if collision occurred.</returns>
		public bool CheckCollisionAbove(float distance, out RaycastHit hitInfo,
		                                Vector3 offsetPosition,
		                                bool useSphereCast = false,
		                                bool useSecondSphereCast = false,
		                                bool adjustPositionSlightly = false)
		{
			var didCollide = false;
			var extraDistance = adjustPositionSlightly
				                      ? k_CollisionOffset
				                      : 0.0f;
			if (!useSphereCast)
			{
				if (UnityEngine.Physics.Raycast(GetHeadWorldPosition() + offsetPosition + Vector3.down * extraDistance,
				                                Vector3.up,
				                                out hitInfo,
				                                distance + extraDistance,
				                                GetCollisionLayerMask(),
				                                m_QueryTriggerInteraction))
				{
					didCollide = true;
					hitInfo.distance = Mathf.Max(0.0f, hitInfo.distance - extraDistance);
				}
			}
			else
			{
				if (SmallSphereCast(Vector3.up,
				                    GetSkinWidth() + distance,
				                    out hitInfo,
				                    offsetPosition,
				                    false))
				{
					didCollide = true;
					hitInfo.distance = Mathf.Max(0.0f, hitInfo.distance - GetSkinWidth());
				}

				if (!didCollide &&
				    useSecondSphereCast)
				{
					if (BigSphereCast(Vector3.up,
					                  distance + extraDistance,
					                  out hitInfo,
					                  offsetPosition + Vector3.down * extraDistance,
					                  false))
					{
						didCollide = true;
						hitInfo.distance = Mathf.Max(0.0f, hitInfo.distance - extraDistance);
					}
				}
			}

			return didCollide;
		}

		/// <summary>
		/// Get the CapsuleCollider.
		/// </summary>
		public CapsuleCollider GetCapsuleCollider()
		{
			return m_CapsuleCollider;
		}

		/// <summary>
		/// Get the slope limit.
		/// </summary>
		public float GetSlopeLimit()
		{
			return m_SlopeLimit;
		}

		/// <summary>
		/// Get the step offset.
		/// </summary>
		public float GetStepOffset()
		{
			return m_StepOffset;
		}

		/// <summary>
		/// Get the skin width.
		/// </summary>
		public float GetSkinWidth()
		{
			return m_SkinWidth;
		}

		/// <summary>
		/// Get the minimum move distance.
		/// </summary>
		public float GetMinMoveDistance()
		{
			return m_MinMoveDistance;
		}

		/// <summary>
		/// Get the minimum move sqr distance.
		/// </summary>
		public float GetMinMoveSqrDistance()
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
			var oldPosition = m_CachedTransform.position;
			var cancelPending = true;

			SetCenter(newCenter, false, false);
			SetHeight(newHeight, false, false, false);

			if (checkForPenetration)
			{
				if (Depenetrate())
				{
					// Inside colliders?
					if (CheckCapsule())
					{
						// Wait until it is safe to resize
						cancelPending = false;
						m_PendingResize.SetHeightAndCenter(newHeight, newCenter);
						// Restore data
						m_Height = oldHeight;
						m_Center = oldCenter;
						m_CachedTransform.position = oldPosition;
						ValidateCapsule(true);
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
			return SetHeightAndCenter(defaultHeight, defaultCenter, checkForPenetration, updateGrounded);
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
			var oldPosition = m_CachedTransform.position;
			var cancelPending = true;

			m_Center = newCenter;
			ValidateCapsule(true);

			if (checkForPenetration)
			{
				if (Depenetrate())
				{
					// Inside colliders?
					if (CheckCapsule())
					{
						// Wait until it is safe to resize
						cancelPending = false;
						m_PendingResize.SetCenter(newCenter);
						// Restore data
						m_Center = oldCenter;
						m_CachedTransform.position = oldPosition;
						ValidateCapsule(true);
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
		}

		/// <summary>
		/// Reset the capsule's center to the default value.
		/// </summary>
		/// <param name="checkForPenetration">Check for collision, and then de-penetrate if there's collision?</param>
		/// <param name="updateGrounded">Update the grounded state? This uses a cast, so only set it to true if you need it.</param>
		public void ResetCenter(bool checkForPenetration, bool updateGrounded)
		{
			SetCenter(defaultCenter, checkForPenetration, updateGrounded);
		}

		/// <summary>
		/// Get the capsule's radius (local).
		/// </summary>
		public float GetRadius()
		{
			return m_Radius;
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
		public float SetHeight(float newHeight, bool preserveFootPosition, bool checkForPenetration,
		                       bool updateGrounded)
		{
			var changeCenter = preserveFootPosition;
			var newCenter = changeCenter
				                    ? CalculateCenterWithSameFootPosition(newHeight)
				                    : m_Center;
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
			var oldPosition = m_CachedTransform.position;
			var cancelPending = true;

			if (changeCenter)
			{
				m_Center = newCenter;
			}

			m_Height = newHeight;
			ValidateCapsule(true);

			if (checkForPenetration)
			{
				if (Depenetrate())
				{
					// Inside colliders?
					if (CheckCapsule())
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

						m_CachedTransform.position = oldPosition;
						ValidateCapsule(true);
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
		/// Can the capsule's height be changed to the specified height (i.e. no collision will occur)?
		/// For example, check if character can stand up from a crouch.
		/// </summary>
		/// <param name="newHeight">The height we want to set to.</param>
		/// <param name="preserveFootPosition">Adjust the capsule's center to preserve the foot position?</param>
		/// <returns>True if the height can be set without any collision occurring.</returns>
		public bool CanSetHeight(float newHeight, bool preserveFootPosition)
		{
			if (newHeight <= m_Height)
			{
				// Don't need to check for collision when height gets smaller
				return true;
			}

			newHeight = ValidateHeight(newHeight);
			bool collided;
			RaycastHit hitInfo;
			var distance = newHeight - m_Height;
			if (preserveFootPosition)
			{
				// Check for collision above
				collided = CheckCollisionAbove(distance,
				                               out hitInfo,
				                               Vector3.zero,
				                               true,
				                               m_LocalHumanControlled,
				                               m_LocalHumanControlled);
			}
			else
			{
				// Check for collision above
				collided = CheckCollisionAbove(distance / 2.0f,
				                               out hitInfo,
				                               Vector3.zero,
				                               true,
				                               m_LocalHumanControlled,
				                               m_LocalHumanControlled);
				if (!collided)
				{
					// Check for collision below
					collided = CheckCollisionBelow(distance / 2.0f,
					                               out hitInfo,
					                               Vector3.zero,
					                               true,
					                               m_LocalHumanControlled,
					                               m_LocalHumanControlled);
				}
			}

			return !collided;
		}

		/// <summary>
		/// Get the layers to test for collision.
		/// </summary>
		public LayerMask GetCollisionLayerMask()
		{
			return m_CollisionLayerMask;
		}

		/// <summary>
		/// Get if the character is controlled by a local human.
		/// </summary>
		public bool GetLocalHumanControlled()
		{
			return m_LocalHumanControlled;
		}

		/// <summary>
		/// Get whether casts should hit trigger colliders.
		/// </summary>
		public QueryTriggerInteraction GetQueryTriggerInteraction()
		{
			return m_QueryTriggerInteraction;
		}

		/// <summary>
		/// Get the foot local position.
		/// </summary>
		public Vector3 GetFootLocalPosition()
		{
			return m_Center + (Vector3.down * (m_Height / 2.0f + m_SkinWidth));
		}

		/// <summary>
		/// Get the foot world position.
		/// </summary>
		public Vector3 GetFootWorldPosition()
		{
			return m_CachedTransform.position + transformedCenter + (Vector3.down * (scaledHeight / 2.0f + m_SkinWidth));
		}

		/// <summary>
		/// Get top of head local position.
		/// </summary>
		public Vector3 GetHeadLocalPosition()
		{
			return m_Center + (Vector3.up * (m_Height / 2.0f + m_SkinWidth));
		}

		/// <summary>
		/// Get top of head world position.
		/// </summary>
		public Vector3 GetHeadWorldPosition()
		{
			return m_CachedTransform.position + transformedCenter + (Vector3.up * (scaledHeight / 2.0f + m_SkinWidth));
		}

		/// <summary>
		/// Get the top sphere's local position.
		/// </summary>
		public Vector3 GetTopSphereLocalPosition()
		{
			var sphereOffsetY = Vector3.up * (m_Height / 2.0f - m_Radius);
			return m_Center + sphereOffsetY;
		}

		/// <summary>
		/// Get the bottom sphere's local position.
		/// </summary>
		public Vector3 GetBottomSphereLocalPosition()
		{
			var sphereOffsetY = Vector3.up * (m_Height / 2.0f - m_Radius);
			return m_Center - sphereOffsetY;
		}

		/// <summary>
		/// Get the top sphere's world position.
		/// </summary>
		public Vector3 GetTopSphereWorldPosition()
		{
			var sphereOffsetY = Vector3.up * (scaledHeight / 2.0f - scaledRadius);
			return m_CachedTransform.position + transformedCenter + sphereOffsetY;
		}

		/// <summary>
		/// Get the bottom sphere's world position.
		/// </summary>
		public Vector3 GetBottomSphereWorldPosition()
		{
			var sphereOffsetY = Vector3.up * (scaledHeight / 2.0f - scaledRadius);
			return m_CachedTransform.position + transformedCenter - sphereOffsetY;
		}

		/// <summary>
		/// Get the capsule's world position.
		/// </summary>
		public Vector3 GetCapsuleWorldPosition()
		{
			return m_CachedTransform.position + transformedCenter;
		}

		/// <summary>
		/// Returns a point on the capsule collider that is closest to a given location.
		/// </summary>
		/// <param name="position"></param>
		public Vector3 GetClosestPointOnCapsule(Vector3 position)
		{
			return m_CapsuleCollider.ClosestPoint(position);
		}

		/// <summary>
		/// Initialise the capsule and rigidbody, and set the root position.
		/// </summary>
		void Awake()
		{
			m_CachedTransform = transform;
			InitCapsuleColliderAndRigidbody();

			SetRootToOffset();
		}

#if UNITY_EDITOR
		/// <summary>
		/// Validate the capsule.
		/// </summary>
		void OnValidate()
		{
			ValidateCapsule(false);
			SetRootToOffset();
		}
#endif

		/// <summary>
		/// Set the root position.
		/// </summary>
		void LateUpdate()
		{
			SetRootToOffset();
		}

		/// <summary>
		/// Update sliding down slopes, and changes to the capsule's height and center.
		/// </summary>
		void Update()
		{
			UpdateSlideDownSlopes();
			UpdatePendingHeightAndCenter();
		}

#if UNITY_EDITOR
		/// <inheritdoc />
		void OnDrawGizmosSelected(Transform transform)
		{
			if (m_CachedTransform == null)
			{
				m_CachedTransform = transform;
			}

			// Foot position
			Gizmos.color = Color.cyan;
			var footPosition = GetFootWorldPosition();
			Gizmos.DrawLine(footPosition + Vector3.left * scaledRadius,
			                footPosition + Vector3.right * scaledRadius);
			Gizmos.DrawLine(footPosition + Vector3.back * scaledRadius,
			                footPosition + Vector3.forward * scaledRadius);

			// Top of head
			var headPosition = GetHeadWorldPosition();
			Gizmos.DrawLine(headPosition + Vector3.left * scaledRadius,
			                headPosition + Vector3.right * scaledRadius);
			Gizmos.DrawLine(headPosition + Vector3.back * scaledRadius,
			                headPosition + Vector3.forward * scaledRadius);

			// Center position
			var centerPosition = GetCapsuleWorldPosition();
			Gizmos.DrawLine(centerPosition + Vector3.left * scaledRadius,
			                centerPosition + Vector3.right * scaledRadius);
			Gizmos.DrawLine(centerPosition + Vector3.back * scaledRadius,
			                centerPosition + Vector3.forward * scaledRadius);

			var tempCapsuleCollider = m_CapsuleCollider;
			if (tempCapsuleCollider == null)
			{
				// Check if there's an attached collider
				tempCapsuleCollider =
					(CapsuleCollider) m_CachedTransform.gameObject.GetComponent(typeof(CapsuleCollider));
			}

			if (tempCapsuleCollider != null)
			{
				// No need to draw a fake collider, because the real collider will draw itself
				return;
			}
		}
#endif

		/// <summary>
		/// Initialize the capsule collider and the rigidbody
		/// </summary>
		void InitCapsuleColliderAndRigidbody()
		{
			var go = m_CachedTransform.gameObject;
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
			defaultCenter = m_Center;
		}

		/// <summary>
		/// Call this when the capsule's values change.
		/// </summary>
		/// <param name="updateCapsuleCollider">Update the capsule collider's values (e.g. center, height, radius)?</param>
		/// <param name="checkForPenetration">Check for collision, and then de-penetrate if there's collision?</param>
		/// <param name="updateGrounded">Update the grounded state? This uses a cast, so only set it to true if you need it.</param>
		void ValidateCapsule(bool updateCapsuleCollider,
		                             bool checkForPenetration = false,
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
				Depenetrate();
			}

			if (updateGrounded)
			{
				UpdateGrounded(CollisionFlags.None);
			}
		}

		/// <summary>
		/// Calculate a new center if the height changes and preserve the foot position.
		/// </summary>
		/// <param name="newHeight">New height.</param>
		Vector3 CalculateCenterWithSameFootPosition(float newHeight)
		{
			var localFootY = m_Center.y - (m_Height / 2.0f + m_SkinWidth);
			var newCenterY = localFootY + (newHeight / 2.0f + m_SkinWidth);
			return new Vector3(m_Center.x, newCenterY, m_Center.z);
		}

		/// <summary>
		/// Moves the characters.
		/// </summary>
		/// <param name="moveVector">Move vector.</param>
		/// <param name="slideWhenMovingDown">Slide against obstacles when moving down? (e.g. we don't want to slide when applying gravity while the charcter is grounded)</param>
		/// <param name="forceTryStickToGround">Force try to stick to ground? Only used if character is grounded before moving.</param>
		/// <param name="doNotStepOffset">Do not try to perform the step offset?</param>
		/// <returns>CollisionFlags is the summary of collisions that occurred during the move.</returns>
		CollisionFlags MoveInternal(Vector3 moveVector, bool slideWhenMovingDown,
		                                    bool forceTryStickToGround = false,
		                                    bool doNotStepOffset = false)
		{
			var sqrDistance = moveVector.sqrMagnitude;
			if (sqrDistance <= 0.0f ||
			    sqrDistance < GetMinMoveSqrDistance() ||
			    sqrDistance < k_MinMoveSqrDistance)
			{
				return CollisionFlags.None;
			}

			var wasGrounded = isGrounded;
			var oldVelocity = velocity;
			var oldCollisionFlags = collisionFlags;
			var moveVectorNoY = new Vector3(moveVector.x, 0.0f, moveVector.z);
			var tryToStickToGround = (wasGrounded &&
			                           moveVector.y <= 0.0f &&
			                           moveVectorNoY.sqrMagnitude.NotEqualToZero()) ||
			                          (wasGrounded &&
			                           forceTryStickToGround);

			m_StartPosition = m_CachedTransform.position;

			collisionFlags = CollisionFlags.None;
			m_CollisionInfoDictionary.Clear();
			m_DownCollisionNormal = null;

			// Stop sliding down slopes when character jumps
			if (moveVector.y > 0.0f &&
			    isSlidingDownSlope)
			{
				StopSlideDownSlopes();
			}

			// Do the move loop
			MoveLoop(moveVector, tryToStickToGround, slideWhenMovingDown, doNotStepOffset);

			var doDownCast = tryToStickToGround ||
			                  moveVector.y <= 0.0f;
			UpdateGrounded(collisionFlags, doDownCast);
			velocity = m_CachedTransform.position - m_StartPosition;

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

			if (m_SendColliderHitMessages)
			{
				SendHitMessages();
			}

			return collisionFlags;
		}

		/// <summary>
		/// Send hit messages.
		/// </summary>
		void SendHitMessages()
		{
			if (m_CollisionInfoDictionary == null ||
			    m_CollisionInfoDictionary.Count <= 0)
			{
				return;
			}

			foreach (var keyValuePair in m_CollisionInfoDictionary)
			{
				m_CachedTransform.gameObject.SendMessage("OnOpenCharacterControllerHit",
				                                       keyValuePair.Value,
				                                       SendMessageOptions.DontRequireReceiver);
			}
		}

		/// <summary>
		/// Determine if the character is grounded.
		/// </summary>
		/// <param name="movedCollisionFlags">Moved collision flags of the current move. Set to None if not moving.</param>
		/// <param name="doDownCast">Do a down cast? We want to avoid this when the character is jumping upwards.</param>
		void UpdateGrounded(CollisionFlags movedCollisionFlags, bool doDownCast = true)
		{
			var wasGrounded = isGrounded;

			if ((movedCollisionFlags & CollisionFlags.CollidedBelow) != 0)
			{
				isGrounded = true;
			}
			else if (doDownCast)
			{
				RaycastHit hitinfo;
				isGrounded = CheckCollisionBelow(k_GroundedTestDistance,
				                                 out hitinfo,
				                                 Vector3.zero,
				                                 true,
				                                 m_LocalHumanControlled,
				                                 m_LocalHumanControlled);
			}
			else
			{
				isGrounded = false;
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
		/// <param name="doNotStepOffset">Do not try to perform the step offset?</param>
		void MoveLoop(Vector3 moveVector, bool tryToStickToGround, bool slideWhenMovingDown,
		                      bool doNotStepOffset)
		{
			m_MoveVectors.Clear();
			m_NextMoveVectorIndex = 0;

			// Split the move vector into horizontal and vertical components.
			SplitMoveVector(moveVector, slideWhenMovingDown, doNotStepOffset);
			var remainingMoveVector = m_MoveVectors[m_NextMoveVectorIndex];
			m_NextMoveVectorIndex++;

			var didTryToStickToGround = false;
			m_StuckInfo.OnMoveLoop();

			// The loop
			for (var i = 0; i < k_MaxMoveIterations; i++)
			{
				var refMoveVector = remainingMoveVector.moveVector;
				var collided = MoveMajorStep(ref refMoveVector,
				                              remainingMoveVector.canSlide,
				                              didTryToStickToGround);

				remainingMoveVector.moveVector = refMoveVector;

				// Character stuck?
				if (m_StuckInfo.UpdateStuck(m_CachedTransform.position,
				                          remainingMoveVector.moveVector,
				                          moveVector))
				{
					// Stop current move loop vector
					remainingMoveVector = new MoveVector(Vector3.zero);
				}
				else if (!m_LocalHumanControlled &&
				         collided)
				{
					// Only slide once for non-human controlled characters
					remainingMoveVector.canSlide = false;
				}

				// Not collided OR vector used up (i.e. vector is zero)?
				if (!collided ||
				    remainingMoveVector.moveVector.sqrMagnitude.IsEqualToZero())
				{
					// Are there remaining movement vectors?
					if (m_NextMoveVectorIndex < m_MoveVectors.Count)
					{
						remainingMoveVector = m_MoveVectors[m_NextMoveVectorIndex];
						m_NextMoveVectorIndex++;
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
		}

		/// <summary>
		/// A single movement major step. Returns true when there is collision.
		/// </summary>
		/// <param name="moveVector">The move vector.</param>
		/// <param name="canSlide">Can slide against obsyacles?</param>
		/// <param name="tryGrounding">Try grounding the player?</param>
		bool MoveMajorStep(ref Vector3 moveVector, bool canSlide, bool tryGrounding)
		{
			var direction = moveVector.normalized;
			var distance = moveVector.magnitude;
			RaycastHit bigRadiusHitInfo;
			RaycastHit smallRadiusHitInfo;
			bool smallRadiusHit;
			bool bigRadiusHit;

			if (!CapsuleCast(direction, distance,
			                 out smallRadiusHit, out bigRadiusHit,
			                 out smallRadiusHitInfo, out bigRadiusHitInfo,
			                 Vector3.zero))
			{
				// No collision, so move to the position
				MovePosition(moveVector, null, null);

				// Check for penetration
				float penetrationDistance;
				Vector3 penetrationDirection;
				if (GetPenetrationInfo(out penetrationDistance, out penetrationDirection))
				{
					// Push away from obstacles
					MovePosition(penetrationDirection * penetrationDistance, null, null);
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
				                     true);

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
				                     true);
				return true;
			}

			MoveAwayFromObstacle(ref moveVector, ref bigRadiusHitInfo,
			                     direction, distance,
			                     canSlide,
			                     tryGrounding,
			                     false);

			return true;
		}

		/// <summary>
		/// Can the character perform a step offset?
		/// </summary>
		/// <param name="moveVector">Horizontal movement vector.</param>
		bool CanStepOffset(Vector3 moveVector)
		{
			var moveVectorMagnitude = moveVector.magnitude;
			RaycastHit hitInfo;

			// Only step up if there's an obstacle at the character's feet (e.g. do not step when only character's head collides)
			if (!SmallSphereCast(moveVector, moveVectorMagnitude, out hitInfo, Vector3.zero, true) &&
			    !BigSphereCast(moveVector, moveVectorMagnitude, out hitInfo, Vector3.zero, true))
			{
				return false;
			}

			var tempStepOffset = GetStepOffset();
			var upDistance = Mathf.Max(tempStepOffset, k_MinStepOffsetHeight);

			// We only step over obstacles if we can partially fit on it (i.e. fit the capsule's radius)
			var horizontal = moveVector * scaledRadius;
			var horizontalSize = horizontal.magnitude;
			horizontal.Normalize();

			// Any obstacles ahead (after we moved up)?
			var up = Vector3.up * upDistance;
			if (SmallCapsuleCast(horizontal, GetSkinWidth() + horizontalSize, out hitInfo, up) ||
			    BigCapsuleCast(horizontal, horizontalSize, out hitInfo, up))
			{
				return false;
			}

			return !CheckSteepSlopeAhead(moveVector);
		}

		/// <summary>
		/// Returns true if there's a steep slope ahead.
		/// </summary>
		/// <param name="moveVector">The movement vector.</param>
		/// <param name="alsoCheckForStepOffset">Do a second test where the step offset will move the player to?</param>
		bool CheckSteepSlopeAhead(Vector3 moveVector, bool alsoCheckForStepOffset = true)
		{
			var direction = moveVector.normalized;
			var distance = moveVector.magnitude;

			if (CheckSteepSlopAhead(direction, distance, Vector3.zero))
			{
				return true;
			}

			// Only need to do the second test for human controlled character
			if (!alsoCheckForStepOffset ||
			    !m_LocalHumanControlled)
			{
				return false;
			}

			// Check above where the step offset will move the player to
			return CheckSteepSlopAhead(direction,
			                           Mathf.Max(distance, k_MinCheckSteepSlopeAheadDistance),
			                           Vector3.up * GetStepOffset());
		}

		/// <summary>
		/// Returns true if there's a steep slope ahead.
		/// </summary>
		bool CheckSteepSlopAhead(Vector3 direction, float distance, Vector3 offsetPosition)
		{
			RaycastHit bigRadiusHitInfo;
			RaycastHit smallRadiusHitInfo;
			bool smallRadiusHit;
			bool bigRadiusHit;

			if (!CapsuleCast(direction, distance,
			                 out smallRadiusHit, out bigRadiusHit,
			                 out smallRadiusHitInfo, out bigRadiusHitInfo,
			                 offsetPosition))
			{
				// No collision
				return false;
			}

			RaycastHit hitInfoCapsule;

			// Did the big radius not hit an obstacle?
			if (!bigRadiusHit)
			{
				// The small radius hit an obstacle
				hitInfoCapsule = smallRadiusHitInfo;
			}
			else
			{
				// Use the nearest collision point (e.g. to handle cases where 2 or more colliders' edges meet)
				if (smallRadiusHit &&
				    smallRadiusHitInfo.distance < bigRadiusHitInfo.distance)
				{
					hitInfoCapsule = smallRadiusHitInfo;
				}
				else
				{
					hitInfoCapsule = bigRadiusHitInfo;
				}
			}

			RaycastHit hitInfoRay;
			var rayOrigin = GetCapsuleWorldPosition() + offsetPosition;
			var rayDirection = hitInfoCapsule.point - rayOrigin;

			// Raycast returns a more accurate normal than SphereCast/CapsuleCast
			if (UnityEngine.Physics.Raycast(rayOrigin,
			                                rayDirection,
			                                out hitInfoRay,
			                                rayDirection.magnitude * k_RaycastScaleDistance,
			                                GetCollisionLayerMask(),
			                                m_QueryTriggerInteraction) &&
			    hitInfoRay.collider == hitInfoCapsule.collider)
			{
				hitInfoCapsule = hitInfoRay;
			}

			var slopeAngle = Vector3.Angle(Vector3.up, hitInfoCapsule.normal);
			var slopeIsSteep = slopeAngle > GetSlopeLimit() &&
			                    slopeAngle < k_MaxSlopeLimit &&
			                    Vector3.Dot(direction, hitInfoCapsule.normal) < 0.0f;

			return slopeIsSteep;
		}

		/// <summary>
		/// Split the move vector into horizontal and vertical components. The results are added to the moveVectors list.
		/// </summary>
		/// <param name="moveVector">The move vector.</param>
		/// <param name="slideWhenMovingDown">Slide against obstacles when moving down? (e.g. we don't want to slide when applying gravity while the charcter is grounded)</param>
		/// <param name="doNotStepOffset">Do not try to perform the step offset?</param>
		void SplitMoveVector(Vector3 moveVector, bool slideWhenMovingDown,
		                             bool doNotStepOffset)
		{
			var horizontal = new Vector3(moveVector.x, 0.0f, moveVector.z);
			var vertical = new Vector3(0.0f, moveVector.y, 0.0f);
			var horizontalIsAlmostZero = IsMoveVectorAlmostZero(horizontal);
			var tempStepOffset = GetStepOffset();
			var doStepOffset = isGrounded &&
			                    !doNotStepOffset &&
			                    !Mathf.Approximately(tempStepOffset, 0.0f) &&
			                    !horizontalIsAlmostZero;

			// Note: Vector is split in this order: up, horizontal, down

			if (vertical.y > 0.0f)
			{
				// Up
				if (horizontal.x.NotEqualToZero() ||
				    horizontal.z.NotEqualToZero())
				{
					// Move up then horizontal
					AddMoveVector(vertical, m_CanSlideAgainstCeiling);
					AddMoveVector(horizontal);
				}
				else
				{
					// Move up
					AddMoveVector(vertical, m_CanSlideAgainstCeiling);
				}
			}
			else if (vertical.y < 0.0f)
			{
				// Down
				if (horizontal.x.NotEqualToZero() ||
				    horizontal.z.NotEqualToZero())
				{
					if (doStepOffset &&
					    CanStepOffset(horizontal))
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
				if (doStepOffset &&
				    CanStepOffset(horizontal))
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

		/// <summary>
		/// Add the movement vector to the moveVectors list.
		/// </summary>
		/// <param name="moveVector">Move vector to add.</param>
		/// <param name="canSlide">Can the movement slide along obstacles?</param>
		int AddMoveVector(Vector3 moveVector, bool canSlide = true)
		{
			m_MoveVectors.Add(new MoveVector(moveVector, canSlide));
			return m_MoveVectors.Count - 1;
		}

		/// <summary>
		/// Insert the movement vector into the moveVectors list.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="moveVector">Move vector to add.</param>
		/// <param name="canSlide">Can the movement slide along obstacles?</param>
		/// <returns>The index where it was inserted.</returns>
		int InsertMoveVector(int index, Vector3 moveVector, bool canSlide = true)
		{
			if (index < 0)
			{
				index = 0;
			}

			if (index >= m_MoveVectors.Count)
			{
				m_MoveVectors.Add(new MoveVector(moveVector, canSlide));
				return m_MoveVectors.Count - 1;
			}

			m_MoveVectors.Insert(index, new MoveVector(moveVector, canSlide));
			return index;
		}

		/// <summary>
		/// Is the move loop on the final move vector?
		/// </summary>
		bool IsFinalMoveVector()
		{
			return m_MoveVectors.Count == 0 ||
			       m_NextMoveVectorIndex >= m_MoveVectors.Count;
		}

		/// <summary>
		/// Is the movement vector almost zero (i.e. very small)?
		/// </summary>
		bool IsMoveVectorAlmostZero(Vector3 moveVector)
		{
			if (Mathf.Abs(moveVector.x) > k_SmallMoveVector ||
			    Mathf.Abs(moveVector.y) > k_SmallMoveVector ||
			    Mathf.Abs(moveVector.z) > k_SmallMoveVector)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Test if character can stick to the ground, and set the down vector if so.
		/// </summary>
		/// <param name="moveVector">The original movement vector.</param>
		/// <param name="getDownVector">Get the down vector.</param>
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

		/// <summary>
		/// Do two capsule casts. One excluding the capsule's skin width and one including the skin width.
		/// </summary>
		/// <param name="direction">Direction to cast.</param>
		/// <param name="distance">Distance to cast.</param>
		/// <param name="smallRadiusHit">Did hit, excluding the skin width?</param>
		/// <param name="bigRadiusHit">Did hit, including the skin width?</param>
		/// <param name="smallRadiusHitInfo">Hit info for cast exlucing the skin width.</param>
		/// <param name="bigRadiusHitInfo">Hit info for cast including the skin width.</param>
		/// <param name="offsetPosition">Offset position, if we want to test somewhere relative to the capsule's position.</param>
		bool CapsuleCast(Vector3 direction, float distance,
		                         out bool smallRadiusHit, out bool bigRadiusHit,
		                         out RaycastHit smallRadiusHitInfo, out RaycastHit bigRadiusHitInfo,
		                         Vector3 offsetPosition)
		{
			// Exclude the skin width in the test
			smallRadiusHit = SmallCapsuleCast(direction, distance, out smallRadiusHitInfo, offsetPosition);

			// Include the skin width in the test
			bigRadiusHit = BigCapsuleCast(direction, distance, out bigRadiusHitInfo, offsetPosition);

			return smallRadiusHit ||
			       bigRadiusHit;
		}

		/// <summary>
		/// Do a capsule cast, exlucliding the skin width.
		/// </summary>
		/// <param name="direction">Direction to cast.</param>
		/// <param name="distance">Distance to cast.</param>
		/// <param name="smallRadiusHitInfo">Hit info.</param>
		/// <param name="offsetPosition">Offset position, if we want to test somewhere relative to the capsule's position.</param>
		bool SmallCapsuleCast(Vector3 direction, float distance,
		                              out RaycastHit smallRadiusHitInfo,
		                              Vector3 offsetPosition)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
			// when moving almost parallel to an obstacle for small distances).
			var extraDistance = scaledRadius;

			if (UnityEngine.Physics.CapsuleCast(GetTopSphereWorldPosition() + offsetPosition,
			                                    GetBottomSphereWorldPosition() + offsetPosition,
			                                    scaledRadius,
			                                    direction,
			                                    out smallRadiusHitInfo,
			                                    distance + extraDistance,
			                                    GetCollisionLayerMask(),
			                                    m_QueryTriggerInteraction))
			{
				return smallRadiusHitInfo.distance <= distance;
			}

			return false;
		}

		/// <summary>
		/// Do a capsule cast, includes the skin width.
		/// </summary>
		/// <param name="direction">Direction to cast.</param>
		/// <param name="distance">Distance to cast.</param>
		/// <param name="bigRadiusHitInfo">Hit info.</param>
		/// <param name="offsetPosition">Offset position, if we want to test somewhere relative to the capsule's position.</param>
		bool BigCapsuleCast(Vector3 direction, float distance,
		                            out RaycastHit bigRadiusHitInfo,
		                            Vector3 offsetPosition)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
			// when moving almost parallel to an obstacle for small distances).
			var extraDistance = scaledRadius + GetSkinWidth();

			if (UnityEngine.Physics.CapsuleCast(GetTopSphereWorldPosition() + offsetPosition,
			                                    GetBottomSphereWorldPosition() + offsetPosition,
			                                    scaledRadius + GetSkinWidth(),
			                                    direction,
			                                    out bigRadiusHitInfo,
			                                    distance + extraDistance,
			                                    GetCollisionLayerMask(),
			                                    m_QueryTriggerInteraction))
			{
				return bigRadiusHitInfo.distance <= distance;
			}

			return false;
		}

		/// <summary>
		/// Do a sphere cast, excludes the skin width. Sphere position is at the top or bottom of the capsule.
		/// </summary>
		/// <param name="direction">Direction to cast.</param>
		/// <param name="distance">Distance to cast.</param>
		/// <param name="smallRadiusHitInfo">Hit info.</param>
		/// <param name="offsetPosition">Offset position, if we want to test somewhere relative to the capsule's position.</param>
		/// <param name="useBottomSphere">Use the sphere at the bottom of the capsule? If false then use the top sphere.</param>
		bool SmallSphereCast(Vector3 direction, float distance,
		                             out RaycastHit smallRadiusHitInfo,
		                             Vector3 offsetPosition,
		                             bool useBottomSphere)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
			// when moving almost parallel to an obstacle for small distances).
			var extraDistance = scaledRadius;

			var spherePosition = useBottomSphere
				                         ? GetBottomSphereWorldPosition() + offsetPosition
				                         : GetTopSphereWorldPosition() + offsetPosition;
			if (UnityEngine.Physics.SphereCast(spherePosition,
			                                   scaledRadius,
			                                   direction,
			                                   out smallRadiusHitInfo,
			                                   distance + extraDistance,
			                                   GetCollisionLayerMask(),
			                                   m_QueryTriggerInteraction))
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
		/// <param name="offsetPosition">Offset position, if we want to test somewhere relative to the capsule's position.</param>
		/// <param name="useBottomSphere">Use the sphere at the bottom of the capsule? If false then use the top sphere.</param>
		bool BigSphereCast(Vector3 direction, float distance,
		                           out RaycastHit bigRadiusHitInfo,
		                           Vector3 offsetPosition,
		                           bool useBottomSphere)
		{
			// Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
			// when moving almost parallel to an obstacle for small distances).
			var extraDistance = scaledRadius + GetSkinWidth();

			var spherePosition = useBottomSphere
				                         ? GetBottomSphereWorldPosition() + offsetPosition
				                         : GetTopSphereWorldPosition() + offsetPosition;
			if (UnityEngine.Physics.SphereCast(spherePosition,
			                                   scaledRadius + GetSkinWidth(),
			                                   direction,
			                                   out bigRadiusHitInfo,
			                                   distance + extraDistance,
			                                   GetCollisionLayerMask(),
			                                   m_QueryTriggerInteraction))
			{
				return bigRadiusHitInfo.distance <= distance;
			}

			return false;
		}

		/// <summary>
		/// Called whan a capsule cast detected an obstacle. Move away from the obstacle and slide against it if needed.
		/// </summary>
		/// <param name="moveVector">The movement vector.</param>
		/// <param name="hitInfoCapsule">Hit info of the capsule cast collision.</param>
		/// <param name="direction">Direction of the cast.</param>
		/// <param name="distance">Distance of the cast.</param>
		/// <param name="canSlide">Can slide against obstacles?</param>
		/// <param name="tryGrounding">Try grounding the player?</param>
		/// <param name="hitSmallCapsule">Did the collision occur with the small capsule (i.e. no skin width)?</param>
		void MoveAwayFromObstacle(ref Vector3 moveVector, ref RaycastHit hitInfoCapsule,
		                                  Vector3 direction, float distance,
		                                  bool canSlide,
		                                  bool tryGrounding,
		                                  bool hitSmallCapsule)
		{
			// IMPORTANT: This method must set moveVector.

			// When the small capsule hit then stop skinWidth away from obstacles
			var collisionOffset = hitSmallCapsule
				                        ? GetSkinWidth()
				                        : k_CollisionOffset;

			var hitDistance = Mathf.Max(hitInfoCapsule.distance - collisionOffset, 0.0f);
			// Note: remainingDistance is more accurate is we use hitDistance, but using hitInfoCapsule.distance gives a tiny 
			// bit of dampening when sliding along obstacles
			var remainingDistance = Mathf.Max(distance - hitInfoCapsule.distance, 0.0f);

			// Move to the collision point
			MovePosition(direction * hitDistance, direction, hitInfoCapsule);

			Vector3 hitNormal;
			RaycastHit hitInfoRay;
			var rayOrigin = GetCapsuleWorldPosition();
			var rayDirection = hitInfoCapsule.point - rayOrigin;

			// Raycast returns a more accurate normal than SphereCast/CapsuleCast
			// Using angle <= k_MaxAngleToUseRaycastNormal gives a curve when collision is near an edge.
			if (UnityEngine.Physics.Raycast(rayOrigin,
			                                rayDirection,
			                                out hitInfoRay,
			                                rayDirection.magnitude * k_RaycastScaleDistance,
			                                GetCollisionLayerMask(),
			                                m_QueryTriggerInteraction) &&
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

			if (GetPenetrationInfo(out penetrationDistance, out penetrationDirection, true, null, hitInfoCapsule))
			{
				// Push away from the obstacle
				MovePosition(penetrationDirection * penetrationDistance, null, null);
			}

			var slopeIsSteep = false;
			if (tryGrounding ||
			    m_StuckInfo.isStuck)
			{
				// No further movement when grounding the character, or the character is stuck
				canSlide = false;
			}
			else if (moveVector.x.NotEqualToZero() ||
			         moveVector.z.NotEqualToZero())
			{
				// Test if character is trying to walk up a steep slope
				var slopeAngle = Vector3.Angle(Vector3.up, hitNormal);
				slopeIsSteep = slopeAngle > GetSlopeLimit() &&
				               slopeAngle < k_MaxSlopeLimit &&
				               Vector3.Dot(direction, hitNormal) < 0.0f;
			}

			// Set moveVector
			if (canSlide &&
			    remainingDistance > 0.0f)
			{
				var slideNormal = hitNormal;

				if (slopeIsSteep &&
				    slideNormal.y > 0.0f)
				{
					// Do not move up the slope
					slideNormal.y = 0.0f;
					slideNormal.Normalize();
				}

				// Vector to slide along the obstacle
				var project = Vector3.Cross(direction, slideNormal);
				project = Vector3.Cross(slideNormal, project);

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
				// Stop current move loop vector
				moveVector = Vector3.zero;
			}

			if (direction.y < 0.0f &&
			    Mathf.Approximately(direction.x, 0.0f) &&
			    Mathf.Approximately(direction.z, 0.0f))
			{
				// This is used by the sliding down slopes
				m_DownCollisionNormal = hitNormal;
			}
		}

		/// <summary>
		/// Check for collision penetration, then try to de-penetrate if there is collision.
		/// </summary>
		bool Depenetrate()
		{
			float distance;
			Vector3 direction;
			if (GetPenetrationInfo(out distance, out direction))
			{
				MovePosition(direction * distance, null, null);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Get direction and distance to move out of the obstacle.
		/// </summary>
		/// <param name="getDistance">Get distance to move out of the obstacle.</param>
		/// <param name="getDirection">Get direction to move out of the obstacle.</param>
		/// <param name="includSkinWidth">Include the skin width in the test?</param>
		/// <param name="offsetPosition">Offset position, if we want to test somewhere relative to the capsule's position.</param>
		/// <param name="hitInfo">The hit info.</param>
		bool GetPenetrationInfo(out float getDistance, out Vector3 getDirection,
		                                bool includSkinWidth = true,
		                                Vector3? offsetPosition = null,
		                                RaycastHit? hitInfo = null)
		{
			getDistance = 0.0f;
			getDirection = Vector3.zero;

			var offset = offsetPosition != null
				                 ? offsetPosition.Value
				                 : Vector3.zero;
			var tempSkinWidth = includSkinWidth
				                      ? GetSkinWidth()
				                      : 0.0f;
			var overlapCount = UnityEngine.Physics.OverlapCapsuleNonAlloc(GetTopSphereWorldPosition() + offset,
			                                                              GetBottomSphereWorldPosition() + offset,
			                                                              scaledRadius + tempSkinWidth,
			                                                              m_PenetrationInfoColliders,
			                                                              GetCollisionLayerMask(),
			                                                              m_QueryTriggerInteraction);
			if (overlapCount <= 0 ||
			    m_PenetrationInfoColliders.Length <= 0)
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
				                       out direction, out distance, includSkinWidth))
				{
					localPos += direction * (distance + k_CollisionOffset);
					result = true;
				}
				else if (hitInfo != null &&
				         hitInfo.Value.collider == collider)
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

		/// <summary>
		/// Check if any colliders overlap the capsule.
		/// </summary>
		/// <param name="includSkinWidth">Include the skin width in the test?</param>
		/// <param name="offsetPosition">Offset position, if we want to test somewhere relative to the capsule's position.</param>
		bool CheckCapsule(bool includSkinWidth = true,
		                          Vector3? offsetPosition = null)
		{
			var offset = offsetPosition != null
				                 ? offsetPosition.Value
				                 : Vector3.zero;
			var tempSkinWidth = includSkinWidth
				                      ? GetSkinWidth()
				                      : 0.0f;
			return UnityEngine.Physics.CheckCapsule(GetTopSphereWorldPosition() + offset,
			                                        GetBottomSphereWorldPosition() + offset,
			                                        scaledRadius + tempSkinWidth,
			                                        GetCollisionLayerMask(),
			                                        m_QueryTriggerInteraction);
		}

		/// <summary>
		/// Move the capsule position.
		/// </summary>
		/// <param name="moveVector">Move vector.</param>
		/// <param name="collideDirection">Direction we encountered collision. Null if no collision.</param>
		/// <param name="hitInfo">Hit info of the collision. Null if no collision.</param>
		void MovePosition(Vector3 moveVector, Vector3? collideDirection, RaycastHit? hitInfo)
		{
			if (moveVector.sqrMagnitude.NotEqualToZero())
			{
				m_CachedTransform.position += moveVector;
			}

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
		void UpdateCollisionInfo(Vector3 direction, RaycastHit? hitInfo)
		{
			if (direction.x.NotEqualToZero() ||
			    direction.z.NotEqualToZero())
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

			m_StuckInfo.hitCount++;

			if (m_SendColliderHitMessages &&
			    hitInfo != null)
			{
				var collider = hitInfo.Value.collider;
				// We only care about the first collision with a collider
				if (!m_CollisionInfoDictionary.ContainsKey(collider))
				{
					var moved = m_CachedTransform.position - m_StartPosition;
					var newCollisionInfo =
						new CollisionInfo(this,
						                  hitInfo.Value,
						                  direction,
						                  moved.magnitude);
					m_CollisionInfoDictionary.Add(collider, newCollisionInfo);
				}
			}
		}

		/// <summary>
		/// Stop auto-slide down steep slopes.
		/// </summary>
		void StopSlideDownSlopes()
		{
			slidingDownSlopeTime = 0.0f;
		}

		/// <summary>
		/// Auto-slide down steep slopes.
		/// </summary>
		void UpdateSlideDownSlopes()
		{
			var dt = Time.deltaTime;
			if (!UpdateSlideDownSlopesInternal(dt))
			{
				if (isSlidingDownSlope)
				{
					slidingDownSlopeTime += dt;
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

		/// <summary>
		/// Auto-slide down steep slopes.
		/// </summary>
		bool UpdateSlideDownSlopesInternal(float dt)
		{
			if (!m_SlideDownSlopes ||
			    !isGrounded)
			{
				return false;
			}

			Vector3 hitNormal;

			// Collided downwards during the last slide movement?
			if (isSlidingDownSlope &&
			    m_DownCollisionNormal != null)
			{
				// This fixes bug: character does not slide off the edge of a slope
				hitNormal = m_DownCollisionNormal.Value;
			}
			else
			{
				RaycastHit hitInfoSphere;
				if (!SmallSphereCast(Vector3.down,
				                     GetSkinWidth() + k_SlideDownSlopeTestDistance,
				                     out hitInfoSphere,
				                     Vector3.zero,
				                     true))
				{
					return false;
				}

				RaycastHit hitInfoRay;
				var rayOrigin = GetBottomSphereWorldPosition();
				var rayDirection = hitInfoSphere.point - rayOrigin;

				// Raycast returns a more accurate normal than SphereCast/CapsuleCast
				if (UnityEngine.Physics.Raycast(rayOrigin,
				                                rayDirection,
				                                out hitInfoRay,
				                                rayDirection.magnitude * k_RaycastScaleDistance,
				                                GetCollisionLayerMask(),
				                                m_QueryTriggerInteraction) &&
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
			var slopeIsSteep = slopeAngle > GetSlopeLimit();
			if (!slopeIsSteep ||
			    slopeAngle >= k_MaxSlopeSlideAngle)
			{
				return false;
			}

			var didSlide = true;
			slidingDownSlopeTime += dt;

			// Pro tip: Here you can also use the friction of the physics material of the slope, to adjust the slide speed.

			// Speed increases as slope angle increases
			var slideSpeedScale = Mathf.Clamp01(slopeAngle / k_MaxSlopeSlideAngle);

			// Apply gravity and slide along the obstacle
			var gravity = Mathf.Abs(UnityEngine.Physics.gravity.y) * m_SlideDownGravityScale * slideSpeedScale;
			var verticalVelocity =
				Mathf.Clamp(gravity * slidingDownSlopeTime, 0.0f, Mathf.Abs(m_SlideDownTerminalVelocity));
			var moveVector = new Vector3(0.0f, -verticalVelocity, 0.0f) * dt;

			// Push slightly away from the slope
			var push = new Vector3(hitNormal.x, 0.0f, hitNormal.z).normalized * k_PushAwayFromSlopeDistance;
			moveVector = new Vector3(push.x, moveVector.y, push.z);

			// Preserve collision flags and velocity. Because user expects them to only be set when manually calling Move/SimpleMove.
			var oldCollisionFlags = collisionFlags;
			var oldVelocity = velocity;

			MoveInternal(moveVector, true, true, true);
			if ((collisionFlags & CollisionFlags.CollidedSides) != 0)
			{
				// Stop sliding when hit something on the side
				didSlide = false;
			}

			collisionFlags = oldCollisionFlags;
			velocity = oldVelocity;

			return didSlide;
		}

		/// <summary>
		/// Update pending height and center when it is safe.
		/// </summary>
		void UpdatePendingHeightAndCenter()
		{
			if (m_PendingResize.heightTime == null &&
			    m_PendingResize.centerTime == null)
			{
				return;
			}

			// Use smallest time
			var time = m_PendingResize.heightTime != null
				             ? m_PendingResize.heightTime.Value
				             : float.MaxValue;
			time = Mathf.Min(time, m_PendingResize.centerTime != null
				                       ? m_PendingResize.centerTime.Value
				                       : float.MaxValue);
			if (time > Time.time)
			{
				return;
			}

			m_PendingResize.ClearTimers();

			if (m_PendingResize.height != null &&
			    m_PendingResize.center != null)
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

		/// <summary>
		/// Sets the playerRootTransform's localPosition to the rootTransformOffset
		/// </summary>
		void SetRootToOffset()
		{
			if (m_PlayerRootTransform != null)
			{
				m_PlayerRootTransform.localPosition = m_RootTransformOffset;
			}
		}
	}
}