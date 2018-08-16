using System;
using System.Collections.Generic;
using Attributes;
using UnityEngine;
using Util;

#if UNITY_EDITOR
using StandardAssets.GizmosHelpers;
#endif

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Open character controller. Handles the movement of a character, by using a capsule for movement and collision detection.
	/// Note: The capsule is always upright. It ignores rotation.
	/// </summary>
	[Serializable]
	public class OpenCharacterController
	{
		/// <summary>
		/// Max slope limit.
		/// </summary>
		private const float k_MaxSlopeLimit = 90.0f;

		/// <summary>
		/// Max slope angle on which character can slide down automatically.
		/// </summary>
		private const float k_MaxSlopeSlideAngle = 90.0f;

		/// <summary>
		/// Distance to test for ground when sliding down slopes.
		/// </summary>
		private const float k_SlideDownSlopeTestDistance = 1.0f;

		/// <summary>
		/// Min skin width.
		/// </summary>
		private const float k_MinSkinWidth = 0.0001f;
		
		/// <summary>
		/// The maximum move itterations. Mainly used as a fail safe to prevent an infinite loop.
		/// </summary>
		private const int k_MaxMoveItterations = 20;

		/// <summary>
		/// Stick to the ground if it is less than this distance from the character.
		/// </summary>
		private const float k_MaxStickToGroundDownDistance = 1.0f;

		/// <summary>
		/// Min distance to test for the ground when sticking to the ground.
		/// </summary>
		private const float k_MinStickToGroundDownDistance = 0.01f;

		/// <summary>
		/// Max colliders to use in the overlap methods.
		/// </summary>
		private const int k_MaxOverlapColliders = 10;

		/// <summary>
		/// Offset to use when moving to a collision point, to try to prevent overlapping the colliders
		/// </summary>
		private const float k_CollisionOffset = 0.001f;

		/// <summary>
		/// Distance to test beneath the character when doing the grounded test
		/// </summary>
		private const float k_GroundedTestDistance = 0.001f;

		/// <summary>
		/// Minimum distance to move. This minimizes small penetrations and inaccurate casts (e.g. into the floor)
		/// </summary>
		private const float k_MinMoveDistance = 0.0001f;
		
		/// <summary>
		/// Minimum sqr distance to move. This minimizes small penetrations and inaccurate casts (e.g. into the floor)
		/// </summary>
		private const float k_MinMoveSqrDistance = k_MinMoveDistance * k_MinMoveDistance;
		
		/// <summary>
		/// Small value to test if the movement vector is small.
		/// </summary>
		private const float k_SmallMoveVector = 1e-6f;

		/// <summary>
		/// If angle between raycast and capsule/sphere cast normal is less than this then use the raycast normal, which is more accurate.
		/// </summary>
		private const float k_MaxAngleToUseRaycastNormal = 5.0f;
		
		/// <summary>
		/// Scale the capsule/sphere hit distance when doing the additional raycast to get a more accurate normal
		/// </summary>
		private const float k_RaycastScaleDistance = 2.0f;
		
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
		[Header("Player Root")]
		[Tooltip("The root bone in the avatar.")]
		[SerializeField]
		private Transform playerRootTransform;

		/// <summary>
		/// The root transform will be positioned at this offset.
		/// </summary>
		[Tooltip("The root transform will be positioned at this offset.")]
		[SerializeField]
		private Vector3 rootTransformOffset = new Vector3(0, 0, 0);
		
		/// <summary>
		/// Limits the collider to only climb slopes that are less steep (in degrees) than the indicated value.
		/// </summary>
		[Header("Collision")]
		[Tooltip("Limits the collider to only climb slopes that are less steep (in degrees) than the indicated value.")]
		[SerializeField]
		private float slopeLimit = 45.0f;

		/// <summary>
		/// The character will step up a stair only if it is closer to the ground than the indicated value.
		/// This should not be greater than the Character Controller’s height or it will generate an error.
		/// Generally this should be kept as small as possible.
		/// </summary>
		[Tooltip("The character will step up a stair only if it is closer to the ground than the indicated value. " +
		         "This should not be greater than the Character Controller’s height or it will generate an error. " +
		         "Generally this should be kept as small as possible.")]
		[SerializeField]
		private float stepOffset = 0.3f;

		/// <summary>
		/// Two colliders can penetrate each other as deep as their Skin Width. Larger Skin Widths reduce jitter.
		/// Low Skin Width can cause the character to get stuck. A good setting is to make this value 10% of the Radius.
		/// </summary>
		[Tooltip("Two colliders can penetrate each other as deep as their Skin Width. Larger Skin Widths reduce jitter. " +
		         "Low Skin Width can cause the character to get stuck. A good setting is to make this value 10% of the Radius.")]
		[SerializeField]
		private float skinWidth = 0.08f;

		/// <summary>
		/// If the character tries to move below the indicated value, it will not move at all. This can be used to reduce jitter.
		/// In most situations this value should be left at 0.
		/// </summary>
		[Tooltip(
			"If the character tries to move below the indicated value, it will not move at all. This can be used to reduce jitter. " +
			"In most situations this value should be left at 0.")]
		[SerializeField]
		private float minMoveDistance = 0.0f;

		/// <summary>
		/// This will offset the Capsule Collider in world space, and won’t affect how the Character pivots.
		/// </summary>
		[Tooltip("This will offset the Capsule Collider in world space, and won’t affect how the Character pivots.")]
		[DisableAtRuntime]
		[SerializeField]
		public Vector3 center;

		/// <summary>
		/// Length of the Capsule Collider’s radius. This is essentially the width of the collider.
		/// </summary>
		[Tooltip("Length of the Capsule Collider’s radius. This is essentially the width of the collider.")]
		[SerializeField]
		private float radius = 0.5f;

		/// <summary>
		/// The Character’s Capsule Collider height. Changing this will scale the collider along the Y axis in both positive and negative directions.
		/// </summary>
		[Tooltip("The Character’s Capsule Collider height. Changing this will scale the collider along the Y axis in both " +
		         "positive and negative directions.")]
		[SerializeField]
		private float height = 2.0f;

		/// <summary>
		/// Add a kinematic Rigidbody? (Only if no Rigidbody is already attached.) Physics works better when moving Colliders have a kinematic Rigidbody.
		/// </summary>
		[Tooltip("Add a kinematic Rigidbody? (Only if no Rigidbody is already attached.) Physics works better when moving Colliders have a kinematic Rigidbody.")]
		[SerializeField]
		private bool addKinematicRigidbody = true;

		/// <summary>
		/// Add the collider (and Rigidbody) to a child object? (Only if no collider or Rigidbody are already attached.) It will create a new child object.
		/// </summary>
		[Tooltip("Add the collider (and Rigidbody) to a child object? (Only if no collider or Rigidbody are already attached.) It will create a new child object.")]
		[SerializeField]
		private bool addColliderAsAChild = true;
		
		/// <summary>
		/// Layer to use for the collider (if it's added to a child object). Leave empty to use the same layer as the parent.
		/// </summary>
		[Tooltip("Layer to use for the collider (if it's added to a child object). Leave empty to use the same layer as the parent.")]
		[SerializeField]
		private string colliderLayer;

		/// <summary>
		/// Layers to test for collision.
		/// </summary>
		[Tooltip("Layers to test for collision.")]
		[SerializeField]
		private LayerMask collisionLayerMask = ~0;	// ~0 sets it to Everything

		/// <summary>
		/// Is the character controlled by a local human? If true then more caluclations are done for more accurate movement.
		/// </summary>
		[Tooltip("Is the character controlled by a local human? If true then more caluclations are done for more " +
		         "accurate movement.")]
		[SerializeField]
		private bool localHumanControlled = true;

		/// <summary>
		/// Can character slide vertically when touching the ceiling?
		/// </summary>
		[Tooltip("Can character slide vertically when touching the ceiling?")]
		[SerializeField]
		private bool canSlideAgainstCeiling = true;

		/// <summary>
		/// Send "OnOpenCharacterControllerHit" messages to game objects? Messages are sent when the character hits a collider while performing a move.
		/// </summary>
		[Tooltip("Send \"OnOpenCharacterControllerHit\" messages to game objects? Messages are sent when the character hits a collider while performing a move.")]
		[SerializeField]
		private bool sendColliderHitMessages = true;
		
		/// <summary>
		/// Slide down slopes when their angle is more than the slope limit?
		/// </summary>
		[Header("Slide Down Slopes")]
		[Tooltip("Slide down slopes when their angle is more than the slope limit?")]
		[SerializeField]
		private bool slideDownSlopes = true;
		
		/// <summary>
		/// The maximum speed that the character can slide downwards
		/// </summary>
		[Tooltip("The maximum speed that the character can slide downwards")]
		[SerializeField]
		protected float slideDownTerminalVelocity = 10.0f;

		/// <summary>
		/// Scale gravity when sliding down slopes.
		/// </summary>
		[Tooltip("Scale gravity when sliding down slopes.")]
		[SerializeField]
		private float slideDownGravityScale = 1.0f;
		
		/// <summary>
		/// The time after initiating a slide classified as a slide start. Used to disable jumping.
		/// </summary>
		[Tooltip("The time after initiating a slide classified as a slide start. Used to disable jumping.")]
		[SerializeField]
		private float slideDownStartDuration = 0.25f;

		/// <summary>
		/// Enable additional debugging info and visuals in the editor
		/// </summary>
		[Header("Debug")]
		[Tooltip("Enable additional debugging info and visuals in the editor")]
		[SerializeField]
		private bool enableDebug;
		
		/// <summary>
		/// The capsule collider.
		/// </summary>
		private CapsuleCollider capsuleCollider;

		/// <summary>
		/// Cached reference to the transform.
		/// </summary>
		private Transform cachedTransform;

		/// <summary>
		/// The position at the start of the movement.
		/// </summary>
		private Vector3 startPosition;
		
		/// <summary>
		/// Movement vectors used in the move loop.
		/// </summary>
		private List<OpenCharacterControllerVector> moveVectors = new List<OpenCharacterControllerVector>();
		
		/// <summary>
		/// Next index in the moveVectors list.
		/// </summary>
		private int nextMoveVectorIndex;
		
		/// <summary>
		/// Stuck info.
		/// </summary>
		private OpenCharacterControllerStuckInfo stuckInfo = new OpenCharacterControllerStuckInfo();

		/// <summary>
		/// The collision info when hitting colliders.
		/// </summary>
		private Dictionary<Collider, OpenCharacterControllerCollisionInfo> collisionInfoDictionary = new Dictionary<Collider, OpenCharacterControllerCollisionInfo>();

		/// <summary>
		/// Slight delay before stopping the sliding down slopes. 
		/// </summary>
		private float delayStopSlidingDownSlopeTime;
		
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
			get { return isSlidingDownSlope && slidingDownSlopeTime <= slideDownStartDuration; }
		}
		
		/// <summary>
		/// How long has character been sliding down a steep slope? (Zero means not busy sliding.)
		/// </summary>
		public float slidingDownSlopeTime { get; private set; }
		
		/// <summary>
		/// The capsule center with the relevant scaling applied (e.g. if object scale is not 1,1,1)
		/// </summary>
		public Vector3 scaledCenter
		{
			get
			{
				Vector3 scale = cachedTransform.lossyScale;
				return new Vector3(center.x * scale.x, center.y * scale.y, center.z * scale.z);
			}
		}

		/// <summary>
		/// The capsule radius with the relevant scaling applied (e.g. if object scale is not 1,1,1)
		/// </summary>
		public float scaledRadius
		{
			get
			{
				Vector3 scale = cachedTransform.lossyScale;
				float maxScale = Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);
				return radius * maxScale;
			}
		}

		/// <summary>
		/// The capsule height with the relevant scaling applied (e.g. if object scale is not 1,1,1)
		/// </summary>
		public float scaledHeight
		{
			get
			{
				return height * cachedTransform.lossyScale.y;
			}
		}

		/// <inheritdoc />
		public float GetPredicitedFallDistance()
		{
			RaycastHit groundHit;
			bool hit = UnityEngine.Physics.Raycast(GetFootWorldPosition(),
			                                       Vector3.down,
			                                       out groundHit,
			                                       float.MaxValue,
			                                       GetCollisionLayerMask());
			return hit? groundHit.distance : float.MaxValue;
		}

		/// <summary>
		/// Move the character. This function does not apply any gravity.
		/// </summary>
		/// <param name="moveVector">Move along this vector.</param>
		/// <returns>CollisionFlags is the summary of collisions that occurred during the Move.</returns>
		public CollisionFlags Move(Vector3 moveVector)
		{
			MoveInternal(moveVector, true);

			#if UNITY_EDITOR
			if (enableDebug)
			{
				Debug.DrawRay(cachedTransform.position + rootTransformOffset, moveVector, Color.green, 1f);
			}
			#endif

			return collisionFlags;
		}

		/// <summary>
		/// Move the character. Velocity along the y-axis is ignored. Speed is in units/s. Gravity is automatically applied.
		/// Returns true if the character is grounded. The method will also apply delta time to the speed.
		/// </summary>
		/// <param name="speed">Move along this vector.</param>
		/// <returns>Whether the character is grounded.</returns>
		public bool SimpleMove(Vector3 speed)
		{
			// Reminder: Time.deltaTime returns the fixed delta time when called from inside FixedUpdate.
			Vector3 moveVector = new Vector3(speed.x, speed.y + UnityEngine.Physics.gravity.y, speed.z) * Time.deltaTime;

			MoveInternal(moveVector, false);

			#if UNITY_EDITOR
			if (enableDebug)
			{
				Debug.DrawRay(cachedTransform.position + rootTransformOffset, moveVector, Color.green, 1f);
			}
			#endif

			return isGrounded;
		}

		/// <summary>
		/// Set the position of the character.
		/// </summary>
		/// <param name="position">Position to set.</param>
		/// <param name="updateGrounded">Update the grounded state? This uses a cast, so only set it to true if you need it.</param>
		public void SetPosition(Vector3 position, bool updateGrounded)
		{
			cachedTransform.position = position;

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
		/// <returns></returns>
		public bool ComputePenetration(Vector3 positionOffset,
									   Collider collider, Vector3 colliderPosition, Quaternion colliderRotation, 
		                               out Vector3 direction, out float distance,
									   bool includeSkinWidth)
		{
			if (collider == capsuleCollider)
			{
				// Ignore self
				direction = Vector3.one;
				distance = 0.0f;
				return false;
			}

			if (includeSkinWidth)
			{
				capsuleCollider.radius = radius + skinWidth;
				capsuleCollider.height = height + (skinWidth * 2.0f);
			}

			// Note: Physics.ComputePenetration does not always return values when the colliders overlap.
			bool result = UnityEngine.Physics.ComputePenetration(capsuleCollider,
			                                              		 cachedTransform.position + positionOffset,
			                                              		 cachedTransform.rotation,
			                                              		 collider, colliderPosition, colliderRotation,
																 out direction, out distance);
			if (includeSkinWidth)
			{
				capsuleCollider.radius = radius;
				capsuleCollider.height = height;
			}

			return result;
		}

		/// <summary>
		/// Get the slope limit.
		/// </summary>
		/// <returns></returns>
		public float GetSlopeLimit()
		{
			return slopeLimit;
		}

		/// <summary>
		/// Get the step offset.
		/// </summary>
		/// <returns></returns>
		public float GetStepOffset()
		{
			return stepOffset;
		}

		/// <summary>
		/// Get the skin width.
		/// </summary>
		/// <returns></returns>
		public float GetSkinWidth()
		{
			return skinWidth;
		}
		
		/// <summary>
		/// Get the minimum move distance.
		/// </summary>
		/// <returns></returns>
		public float GetMinMoveDistance()
		{
			return minMoveDistance;
		}

		/// <summary>
		/// Get the minimum move sqr distance.
		/// </summary>
		/// <returns></returns>
		public float GetMinMoveSqrDistance()
		{
			return minMoveDistance * minMoveDistance;
		}

		/// <summary>
		/// Get the center (local).
		/// </summary>
		/// <returns></returns>
		public Vector3 GetCenter()
		{
			return center;
		}
		
		/// <summary>
		/// Get the radius (local).
		/// </summary>
		/// <returns></returns>
		public float GetRadius()
		{
			return radius;
		}

		/// <summary>
		/// Get the height (local).
		/// </summary>
		/// <returns></returns>
		public float GetHeight()
		{
			return height;
		}

		/// <summary>
		/// Get the layers to test for collision.
		/// </summary>
		/// <returns></returns>
		public LayerMask GetCollisionLayerMask()
		{
			return collisionLayerMask;
		}

		/// <summary>
		/// Get if the character is controlled by a local human.
		/// </summary>
		/// <returns></returns>
		public bool GetLocalHumanControlled()
		{
			return localHumanControlled;
		}

		/// <summary>
		/// Get the foot local position.
		/// </summary>
		/// <returns></returns>
		public Vector3 GetFootLocalPosition()
		{
			return center + (Vector3.down * (height / 2.0f + skinWidth));
		}

		/// <summary>
		/// Get the foot world position.
		/// </summary>
		/// <returns></returns>
		public Vector3 GetFootWorldPosition()
		{
			return cachedTransform.position + scaledCenter + (Vector3.down * (scaledHeight / 2.0f + skinWidth));
		}

		/// <summary>
		/// Get the top sphere's local position.
		/// </summary>
		/// <returns></returns>
		public Vector3 GetTopSphereLocalPosition()
		{
			Vector3 sphereOffsetY = Vector3.up * (height / 2.0f - radius);
			return center + sphereOffsetY;
		}

		/// <summary>
		/// Get the bottom sphere's local position.
		/// </summary>
		/// <returns></returns>
		public Vector3 GetBottomSphereLocalPosition()
		{
			Vector3 sphereOffsetY = Vector3.up * (height / 2.0f - radius);
			return center - sphereOffsetY;
		}
		
		/// <summary>
		/// Get the top sphere's world position.
		/// </summary>
		/// <returns></returns>
		public Vector3 GetTopSphereWorldPosition()
		{
			Vector3 sphereOffsetY = Vector3.up * (scaledHeight / 2.0f - scaledRadius);
			return cachedTransform.position + scaledCenter + sphereOffsetY;
		}

		/// <summary>
		/// Get the bottom sphere's world position.
		/// </summary>
		/// <returns></returns>
		public Vector3 GetBottomSphereWorldPosition()
		{
			Vector3 sphereOffsetY = Vector3.up * (scaledHeight / 2.0f - scaledRadius);
			return cachedTransform.position + scaledCenter - sphereOffsetY;
		}

		/// <summary>
		/// Get the capsule's world position.
		/// </summary>
		/// <returns></returns>
		public Vector3 GetCapsuleWorldPosition()
		{
			return cachedTransform.position + scaledCenter;
		}
		
		/// <summary>
		/// Returns a point on the capsule collider that is closest to a given location.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public Vector3 GetClosestPointOnCapsule(Vector3 position)
		{
			return capsuleCollider.ClosestPoint(position);
		}

		/// <inheritdoc />
		public void Awake(Transform transform)
		{			
			cachedTransform = transform;
			InitCapsuleCollider();
		}
		
		#if UNITY_EDITOR
		/// <inheritdoc />
		public void OnValidate()
		{
			ValidateCapsule(false);
		}
		#endif

		/// <inheritdoc />
		public void LateUpdate()
		{
			if (playerRootTransform != null)
			{
				playerRootTransform.localPosition = rootTransformOffset;
			}
		}

		/// <inheritdoc />
		public void Update()
		{
			UpdateSlideDownSlopes();
		}
		
		#if UNITY_EDITOR
		/// <inheritdoc />
		public void OnDrawGizmosSelected(Transform transform)
		{
			if (cachedTransform == null)
			{
				cachedTransform = transform;
			}
			
			// Foot position
			Gizmos.color = Color.cyan;
			Vector3 footPosition = GetFootWorldPosition();
			Gizmos.DrawLine(footPosition + Vector3.left * scaledRadius,
			                footPosition + Vector3.right * scaledRadius);
			Gizmos.DrawLine(footPosition + Vector3.back * scaledRadius,
			                footPosition + Vector3.forward * scaledRadius);

			CapsuleCollider tempCapsuleCollider = capsuleCollider;
			if (tempCapsuleCollider == null)
			{
				// Check if there's an attached collider
				tempCapsuleCollider = (CapsuleCollider)cachedTransform.gameObject.GetComponent(typeof(CapsuleCollider));
			}
			if (tempCapsuleCollider != null)
			{
				// No need to draw a fake collider, because the real collider will draw itself
				return;
			}

			// Draw capsule collider
			GizmosHelper.DrawCapsule(GetTopSphereWorldPosition(), GetBottomSphereWorldPosition(), 
			                         scaledRadius, Color.green);
			
			// Big capsule collider
			GizmosHelper.DrawCapsule(GetTopSphereWorldPosition(), GetBottomSphereWorldPosition(), 
			                         scaledRadius + skinWidth, new Color(1.0f, 1.0f, 0.0f, 0.1f));
		}
		#endif
		
		/// <summary>
		/// Initialize the capsule collider.
		/// </summary>
		private void InitCapsuleCollider()
		{
			GameObject go = cachedTransform.gameObject;
			capsuleCollider = (CapsuleCollider)go.GetComponent(typeof(CapsuleCollider));
			Rigidbody rigidbody = (Rigidbody)go.GetComponent(typeof(Rigidbody));
			bool addCapsule = capsuleCollider == null;
			bool addRigidbody = rigidbody == null;
			
			if (addCapsule &&
			    addColliderAsAChild)
			{
				// Create a child object to which to add the capsule collider and rigidbody
				go = new GameObject("CapsuleCollider");
				go.transform.SetParent(cachedTransform);
				if (string.IsNullOrEmpty(colliderLayer))
				{
					go.layer = cachedTransform.gameObject.layer;
				}
				else
				{
					go.layer = LayerMask.NameToLayer(colliderLayer);
				}
			}

			if (addCapsule)
			{
				capsuleCollider = (CapsuleCollider)go.AddComponent(typeof(CapsuleCollider));
			}
			if (capsuleCollider != null)
			{
				// Must be enabled for ComputePenetration to work.
				capsuleCollider.enabled = true;

				if (addCapsule && 
				    addColliderAsAChild)
				{
					capsuleCollider.transform.localPosition = Vector3.zero;
					capsuleCollider.transform.localRotation = Quaternion.identity;
					capsuleCollider.transform.localScale = Vector3.one;
				}
			}

			if (addRigidbody && 
			    addKinematicRigidbody)
			{
				rigidbody = (Rigidbody)go.AddComponent(typeof(Rigidbody));
				if (rigidbody != null)
				{
					rigidbody.isKinematic = true;
					rigidbody.useGravity = false;
				}
			}

			if (!addCapsule &&
			    capsuleCollider != null)
			{
				// Copy settings from the capsule collider
				center = capsuleCollider.center;
				radius = capsuleCollider.radius;
				height = capsuleCollider.height;
			}

			ValidateCapsule(addCapsule);
		}

		/// <summary>
		/// Call this when the capsule's values change.
		/// </summary>
		/// <param name="addedCapsuleCollider">Was the capsule collider automatically added?</param>
		private void ValidateCapsule(bool addedCapsuleCollider)
		{
			slopeLimit = Mathf.Clamp(slopeLimit, 0.0f, k_MaxSlopeLimit);
			skinWidth = Mathf.Clamp(skinWidth, k_MinSkinWidth, float.MaxValue);
			
			if (addedCapsuleCollider && 
			    capsuleCollider != null)
			{
				// Copy settings to the capsule collider
				capsuleCollider.center = center;
				capsuleCollider.radius = radius;
				capsuleCollider.height = height;
			}
		}
		
		/// <summary>
		/// Moves the characters.
		/// </summary>
		/// <param name="moveVector">Move vector.</param>
		/// <param name="slideWhenMovingDown">Slide against obstacles when moving down? (e.g. we don't want to slide when applying gravity while the charcter is grounded)</param>
		/// <param name="forceTryStickToGround">Force try to stick to ground? Only used if character is grounded before moving.</param>
		/// <returns>CollisionFlags is the summary of collisions that occurred during the move.</returns>
		private CollisionFlags MoveInternal(Vector3 moveVector, bool slideWhenMovingDown,
		                                    bool forceTryStickToGround = false)
		{
			float sqrDistance = moveVector.sqrMagnitude;
			if (sqrDistance <= 0.0f ||
				sqrDistance < GetMinMoveSqrDistance() ||
				sqrDistance < k_MinMoveSqrDistance)
			{
				return CollisionFlags.None;
			}

			bool wasGrounded = isGrounded;
			Vector3 oldVelocity = velocity;
			CollisionFlags oldCollisionFlags = collisionFlags;
			Vector3 moveVectorNoY = new Vector3(moveVector.x, 0.0f, moveVector.z);
			bool tryToStickToGround = (wasGrounded &&
			                           moveVector.y <= 0.0f &&
			                           moveVectorNoY.sqrMagnitude.NotEqualToZero()) ||
			                          (wasGrounded &&
			                           forceTryStickToGround);
			
			startPosition = cachedTransform.position;
			
			collisionFlags = CollisionFlags.None;
			collisionInfoDictionary.Clear();

			// Do the move loop
			MoveLoop(moveVector, tryToStickToGround, slideWhenMovingDown);
			
			UpdateGrounded(collisionFlags);
			velocity = cachedTransform.position - startPosition;

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

			if (sendColliderHitMessages)
			{
				SendHitMessages();
			}

			return collisionFlags;
		}

		/// <summary>
		/// Send hit messages.
		/// </summary>
		private void SendHitMessages()
		{
			if (collisionInfoDictionary == null ||
			    collisionInfoDictionary.Count <= 0)
			{
				return;
			}

			foreach (var keyValuePair in collisionInfoDictionary)
			{
				cachedTransform.gameObject.SendMessage("OnOpenCharacterControllerHit", 
				                                       keyValuePair.Value, 
				                                       SendMessageOptions.DontRequireReceiver);
			}
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
				isGrounded = SmallSphereCast(Vector3.down, k_GroundedTestDistance + GetSkinWidth(), out hitinfo,
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
			
			// Split the move vector into horizontal and vertical components.
			SplitMoveVector(moveVector, slideWhenMovingDown);
			OpenCharacterControllerVector remainingMoveVector = moveVectors[nextMoveVectorIndex];
			nextMoveVectorIndex++;

			bool didTryToStickToGround = false;
			stuckInfo.OnMoveLoop();
			
			// The loop
			for (int i = 0; i < k_MaxMoveItterations; i++)
			{
				bool collided = MoveMajorStep(ref remainingMoveVector.moveVector, 
				                              remainingMoveVector.canSlide,
				                              didTryToStickToGround);
				
				// Character stuck?
				if (stuckInfo.UpdateStuck(cachedTransform.position,
				                          remainingMoveVector.moveVector,
				                          moveVector))
				{
					// Stop current move loop vector
					remainingMoveVector = new OpenCharacterControllerVector(Vector3.zero);
				}
				else if (localHumanControlled && 
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
				if (enableDebug && 
					i == k_MaxMoveItterations - 1)
				{
					Debug.LogWarning(string.Format("reached k_MaxMoveItterations!     (remainingMoveVector: {0}, {1}, {2})     " +
					                               "(moveVector: {3}, {4}, {5})     hitCount: {6}",
					                               remainingMoveVector.moveVector.x, remainingMoveVector.moveVector.y,
					                               remainingMoveVector.moveVector.z,
					                               moveVector.x, moveVector.y, moveVector.z,
					                               stuckInfo.hitCount));
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
		/// <returns>True when there is collision.</returns>
		private bool MoveMajorStep(ref Vector3 moveVector, bool canSlide, bool tryGrounding)
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

				// Stop current move loop vector
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
		/// Returns true if there's a steep slope ahead.
		/// </summary>
		private bool CheckSteepSlopeAhead(Vector3 moveVector)
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
				// No collision
				return false;
			}

			RaycastHit hitInfoCapsule;

			// Did the big radius not hit an obstacle?
			if (bigRadiusHit == false)
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
			Vector3 rayOrigin = GetCapsuleWorldPosition();
			Vector3 rayDirection = hitInfoCapsule.point - rayOrigin;
			
			// Raycast returns a more accurate normal than CapsuleCast
			if (UnityEngine.Physics.Raycast(rayOrigin,
			                                rayDirection,
			                                out hitInfoRay,
			                                rayDirection.magnitude * k_RaycastScaleDistance,
			                                GetCollisionLayerMask()) &&
			    hitInfoRay.collider == hitInfoCapsule.collider)
			{
				hitInfoCapsule = hitInfoRay;
			}
			
			float slopeAngle = Vector3.Angle(Vector3.up, hitInfoCapsule.normal);
			bool slopeIsSteep = slopeAngle > GetSlopeLimit() &&
			                    slopeAngle < k_MaxSlopeLimit &&
			                    Vector3.Dot(direction, hitInfoCapsule.normal) < 0.0f;

			return slopeIsSteep;
		}

		/// <summary>
		/// Split the move vector into horizontal and vertical components. The results are added to the moveVectors list.
		/// </summary>
		/// <param name="moveVector">The move vector.</param>
		/// <param name="slideWhenMovingDown">Slide against obstacles when moving down? (e.g. we don't want to slide when applying gravity while the charcter is grounded)</param>
		private void SplitMoveVector(Vector3 moveVector, bool slideWhenMovingDown)
		{
			Vector3 horizontal = new Vector3(moveVector.x, 0.0f, moveVector.z);
			Vector3 vertical = new Vector3(0.0f, moveVector.y, 0.0f);
			bool horizontalIsAlmostZero = IsMoveVectorAlmostZero(moveVector);
			float tempStepOffset = GetStepOffset();
			bool doStepOffset = !Mathf.Approximately(tempStepOffset, 0.0f) &&
			                    !horizontalIsAlmostZero;

			if (vertical.y > 0.0f)
			{
				if (horizontal.x.NotEqualToZero() || 
				    horizontal.z.NotEqualToZero())
				{
					// Move up then horizontal
					AddMoveVector(vertical, canSlideAgainstCeiling);
					AddMoveVector(horizontal);
				}
				else
				{
					// Move up
					AddMoveVector(vertical, canSlideAgainstCeiling);
				}
			}
			else if (vertical.y < 0.0f)
			{
				if (horizontal.x.NotEqualToZero() || 
				    horizontal.z.NotEqualToZero())
				{
					if (doStepOffset &&
					    !CheckSteepSlopeAhead(horizontal))
					{
						// Move up, horizontal then down
						AddMoveVector(Vector3.up * tempStepOffset, false);
						AddMoveVector(horizontal);
						if (slideWhenMovingDown)
						{
							AddMoveVector(vertical);
							AddMoveVector(Vector3.down * tempStepOffset, false);
						}
						else
						{
							AddMoveVector(vertical + Vector3.down * tempStepOffset, false);
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
				if (doStepOffset &&
				    !CheckSteepSlopeAhead(horizontal))
				{
					// Move up, horizontal then down
					AddMoveVector(Vector3.up * tempStepOffset, false);
					AddMoveVector(horizontal);
					AddMoveVector(Vector3.down * tempStepOffset, false);
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
		private int AddMoveVector(Vector3 moveVector, bool canSlide = true)
		{
			moveVectors.Add(new OpenCharacterControllerVector(moveVector, canSlide));
			return moveVectors.Count - 1;
		}

		/// <summary>
		/// Insert the movement vector into the moveVectors list.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="moveVector">Move vector to add.</param>
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
				moveVectors.Add(new OpenCharacterControllerVector(moveVector, canSlide));
				return moveVectors.Count - 1;
			}

			moveVectors.Insert(index, new OpenCharacterControllerVector(moveVector, canSlide));
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
		/// Is the movement vector almost zero (i.e. very small)?
		/// </summary>
		private bool IsMoveVectorAlmostZero(Vector3 moveVector)
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
		/// <returns></returns>
		private bool CanStickToGround(Vector3 moveVector, out OpenCharacterControllerVector getDownVector)
		{
			Vector3 moveVectorNoY = new Vector3(moveVector.x, 0.0f, moveVector.z);
			float downDistance = Mathf.Max(moveVectorNoY.magnitude, k_MinStickToGroundDownDistance);
			if (moveVector.y < 0.0f)
			{
				downDistance = Mathf.Max(downDistance, Mathf.Abs(moveVector.y));
			}

			if (downDistance <= k_MaxStickToGroundDownDistance)
			{
				getDownVector = new OpenCharacterControllerVector(Vector3.down * downDistance, false);
				return true;
			}
			
			getDownVector = new OpenCharacterControllerVector(Vector3.zero);
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
			float extraDistance = scaledRadius;

			if (UnityEngine.Physics.CapsuleCast(GetTopSphereWorldPosition() + offsetPosition,
			                                    GetBottomSphereWorldPosition() + offsetPosition,
			                                    scaledRadius,
			                                    direction,
			                                    out smallRadiusHitInfo,
			                                    distance + extraDistance,
			                                    GetCollisionLayerMask()))
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
			float extraDistance = scaledRadius + GetSkinWidth();

			if (UnityEngine.Physics.CapsuleCast(GetTopSphereWorldPosition() + offsetPosition,
			                                    GetBottomSphereWorldPosition() + offsetPosition,
			                                    scaledRadius + GetSkinWidth(),
			                                    direction,
			                                    out bigRadiusHitInfo,
			                                    distance + extraDistance,
			                                    GetCollisionLayerMask()))
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
			float extraDistance = scaledRadius;

			Vector3 spherePosition = useBottomSphere
				? GetBottomSphereWorldPosition() + offsetPosition
				: GetTopSphereWorldPosition() + offsetPosition;
			if (UnityEngine.Physics.SphereCast(spherePosition,
			                                    scaledRadius,
			                                    direction,
			                                    out smallRadiusHitInfo,
			                                    distance + extraDistance,
			                                    GetCollisionLayerMask()))
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
			float extraDistance = scaledRadius + GetSkinWidth();

			Vector3 spherePosition = useBottomSphere
				? GetBottomSphereWorldPosition() + offsetPosition
				: GetTopSphereWorldPosition() + offsetPosition;
			if (UnityEngine.Physics.SphereCast(spherePosition,
			                                    scaledRadius + GetSkinWidth(),
			                                    direction,
			                                    out bigRadiusHitInfo,
			                                    distance + extraDistance,
			                                    GetCollisionLayerMask()))
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
		private void MoveAwayFromObstacle(ref Vector3 moveVector, ref RaycastHit hitInfoCapsule,
		                                  Vector3 direction, float distance,
		                                  bool canSlide,
										  bool tryGrounding, 
		                                  bool hitSmallCapsule)
		{
			// IMPORTANT: This method must set moveVector.
			
			// When the small capsule hit then stop skinWidth away from obstacles
			float collisionOffset = hitSmallCapsule
				? GetSkinWidth()
				: k_CollisionOffset;
			
			float hitDistance = Mathf.Max(hitInfoCapsule.distance - collisionOffset, 0.0f);
			// Note: remainingDistance is more accurate is we use hitDistance, but using hitInfoCapsule.distance gives a tiny 
			// bit of dampening when sliding along obstacles
			float remainingDistance = Mathf.Max(distance - hitInfoCapsule.distance, 0.0f);
			
			// Move to the collision point
			MovePosition(direction * hitDistance, direction, hitInfoCapsule);
			
			Vector3 hitNormal;
			RaycastHit hitInfoRay;
			Vector3 rayOrigin = GetCapsuleWorldPosition();
			Vector3 rayDirection = hitInfoCapsule.point - rayOrigin;
			
			// Raycast returns a more accurate normal than CapsuleCast
			if (UnityEngine.Physics.Raycast(rayOrigin,
			                                rayDirection,
			                                out hitInfoRay,
			                                rayDirection.magnitude * k_RaycastScaleDistance,
			                                GetCollisionLayerMask()) &&
			    hitInfoRay.collider == hitInfoCapsule.collider &&
			    Vector3.Angle(hitInfoCapsule.normal, hitInfoRay.normal) <= k_MaxAngleToUseRaycastNormal)
			{
				hitNormal = hitInfoRay.normal;
			}
			else
			{
				hitNormal = hitInfoCapsule.normal;
			}

			float skinPenetrationDistance;
			Vector3 skinPenetrationVector;

			GetPenetrationInfo(out skinPenetrationDistance, out skinPenetrationVector, true, null, hitInfoCapsule);
			
			// Push away from the obstacle
			MovePosition(skinPenetrationVector * skinPenetrationDistance, null, null);

			bool slopeIsSteep = false;
			if (tryGrounding ||
			    stuckInfo.isStuck)
			{
				// No further movement when grounding the character, or the character is stuck
				canSlide = false;
			}
			else if (moveVector.x.NotEqualToZero() ||  
			         moveVector.z.NotEqualToZero())
			{
				// Test if character is trying to walk up a steep slope
				float slopeAngle = Vector3.Angle(Vector3.up, hitNormal);
				slopeIsSteep = slopeAngle > GetSlopeLimit() &&
				               slopeAngle < k_MaxSlopeLimit &&
				               Vector3.Dot(direction, hitNormal) < 0.0f;
			}

			// Set moveVector
			if (canSlide &&
			    remainingDistance > 0.0f)
			{
				Vector3 slideNormal = hitNormal;

				if (slopeIsSteep &&
				    slideNormal.y > 0.0f)
				{
					// Do not move up the slope
					slideNormal.y = 0.0f;
					slideNormal.Normalize();
				}
				
				// Vector to slide along the obstacle
				//Vector3 project = Vector3.ProjectOnPlane(direction, slideNormal);
				
				Vector3 project = Vector3.Cross(direction, slideNormal);
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
		}
		
		/// <summary>
		/// Get direction and distance to move out of the obstacle.
		/// </summary>
		/// <param name="getDistance">Get distance to move out of the obstacle.</param>
		/// <param name="getDirection">Get direction to move out of the obstacle.</param>
		/// <param name="includSkinWidth">Include the skin width in the test?</param>
		/// <param name="offsetPosition">Offset position, if we want to test somewhere other than the capsule's position.</param>
		/// <param name="hitInfo">The hit info.</param>
		private bool GetPenetrationInfo(out float getDistance, out Vector3 getDirection,
		                                bool includSkinWidth = true,
		                                Vector3? offsetPosition = null,
		                                RaycastHit? hitInfo = null)
		{
			getDistance = 0.0f;
			getDirection = Vector3.zero;

			Collider[] colliders = new Collider[k_MaxOverlapColliders];
			Vector3 offset = offsetPosition != null
				? offsetPosition.Value
				: Vector3.zero;
			float tempSkinWidth = includSkinWidth
				? GetSkinWidth()
				: 0.0f;
			int overlapCount = UnityEngine.Physics.OverlapCapsuleNonAlloc(GetTopSphereWorldPosition() + offset,
			                                                              GetBottomSphereWorldPosition() + offset,
			                                                              scaledRadius + tempSkinWidth,
			                                                              colliders,
			                                                              GetCollisionLayerMask());
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
				if (ComputePenetration(offset, 
				                       collider, colliderTransform.position, colliderTransform.rotation, 
				                       out direction, out distance, includSkinWidth))
				{
					getDistance += distance + k_CollisionOffset;
					getDirection += direction;
					result = true;
				}
				else if (hitInfo != null &&
				         hitInfo.Value.collider == collider)
				{
					// We can use the hit normal to push away from the collider, because CapsuleCast generally returns a normal
					// that pushes away from the collider.
					getDistance += k_CollisionOffset;
					getDirection -= hitInfo.Value.normal;
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
			if (moveVector.sqrMagnitude.NotEqualToZero())
			{
				cachedTransform.position += moveVector;
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
		private void UpdateCollisionInfo(Vector3 direction, RaycastHit? hitInfo)
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

			stuckInfo.hitCount++;

			if (sendColliderHitMessages && 
			    hitInfo != null)
			{
				Collider collider = hitInfo.Value.collider;
				// We only care about the first collision with a collider
				if (collisionInfoDictionary.ContainsKey(collider) == false)
				{
					Vector3 moved = cachedTransform.position - startPosition;
					OpenCharacterControllerCollisionInfo newCollisionInfo =
						new OpenCharacterControllerCollisionInfo(this,
						                                       hitInfo.Value,
						                                       direction,
						                                       moved.magnitude);
					collisionInfoDictionary.Add(collider, newCollisionInfo);
				}
			}
		}

		/// <summary>
		/// Auto-slide down steep slopes.
		/// </summary>
		private void UpdateSlideDownSlopes()
		{
			float dt = Time.deltaTime;
			if (!UpdateSlideDownSlopesInternal(dt))
			{
				if (isSlidingDownSlope)
				{
					slidingDownSlopeTime += dt;
					delayStopSlidingDownSlopeTime += dt;
					
					// Slight delay before we stop sliding down slopes. To handle cases where sliding test fails for 1 frame.
					if (delayStopSlidingDownSlopeTime > 0.2f)
					{
						slidingDownSlopeTime = 0.0f;
					}
				}
				else
				{
					slidingDownSlopeTime = 0.0f;
				}
			}
			else
			{
				delayStopSlidingDownSlopeTime = 0.0f;
			}
		}
		
		/// <summary>
		/// Auto-slide down steep slopes.
		/// </summary>
		private bool UpdateSlideDownSlopesInternal(float dt)
		{
			if (!slideDownSlopes ||
			    !isGrounded)
			{
				return false;
			}
			
			RaycastHit hitInfoSphere;
			if (!SmallSphereCast(Vector3.down, 
			                     GetSkinWidth() + k_SlideDownSlopeTestDistance, 
			                     out hitInfoSphere, 
			                     Vector3.zero, 
			                     true))
			{
				return false;
			}

			Vector3 hitNormal;
			RaycastHit hitInfoRay;
			Vector3 rayOrigin = GetBottomSphereWorldPosition();
			Vector3 rayDirection = hitInfoSphere.point - rayOrigin;
			
			// Raycast returns a more accurate normal than SphereCast
			if (UnityEngine.Physics.Raycast(rayOrigin,
			                                rayDirection,
			                                out hitInfoRay,
			                                rayDirection.magnitude * k_RaycastScaleDistance,
			                                GetCollisionLayerMask()) &&
			    hitInfoRay.collider == hitInfoSphere.collider)
			{
				hitNormal = hitInfoRay.normal;
			}
			else
			{
				hitNormal = hitInfoSphere.normal;
			}
			
			float slopeAngle = Vector3.Angle(Vector3.up, hitNormal);
			bool slopeIsSteep = slopeAngle > GetSlopeLimit();
			if (!slopeIsSteep || 
			    slopeAngle >= k_MaxSlopeSlideAngle)
			{
				return false;
			}

			bool didSlide = true;
			slidingDownSlopeTime += dt;
			
			// Pro tip: Here you can also use the friction of the physics material of the slope, to adjust the slide speed.
			
			// Speed increases as slope angle increases
			float slideSpeedScale = Mathf.Clamp01(slopeAngle / k_MaxSlopeSlideAngle);
			// Apply gravity and slide along the obstacle
			float gravity = Mathf.Abs(UnityEngine.Physics.gravity.y) * slideDownGravityScale * slideSpeedScale;
			float verticalVelocity = Mathf.Clamp(gravity * slidingDownSlopeTime,  0.0f, Mathf.Abs(slideDownTerminalVelocity));
			Vector3 moveVector = new Vector3(0.0f, -verticalVelocity, 0.0f) * dt;
			
			// Preserve collision flags, velocity and grounded state. Because user expects them to only be set when manually calling Move/SimpleMove.
			CollisionFlags oldCollisionFlags = collisionFlags;
			Vector3 oldVelocity = velocity;
			bool oldIsGrounded = isGrounded;
			
			MoveInternal(moveVector, true, true);
			if ((collisionFlags & CollisionFlags.CollidedSides) != 0)
			{
				// Stop sliding when hit something on the side
				didSlide = false;
			}

			collisionFlags = oldCollisionFlags;
			velocity = oldVelocity;
			isGrounded = oldIsGrounded;

			return didSlide;
		}
	}
}