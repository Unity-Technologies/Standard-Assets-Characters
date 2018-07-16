using UnityEngine;
using StandardAssets.GizmosHelpers;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Capsule used for a character's movement and collision detection.
	/// Note: The capsule is always upright. It ignores rotation.
	/// </summary>
	public class CharacterCapsule : MonoBehaviour
	{
		/// <summary>
		/// Max slope limit.
		/// </summary>
		private const float k_MaxSlopeLimit = 180.0f;

		/// <summary>
		/// Min skin width.
		/// </summary>
		private const float k_MinSkinWidth = 0.0001f;

		/// <summary>
		/// Limits the collider to only climb slopes that are less steep (in degrees) than the indicated value.
		/// </summary>
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
		[Tooltip(
			"The Character’s Capsule Collider height. Changing this will scale the collider along the Y axis in both positive and negative directions.")]
		[SerializeField]
		private float height = 2.0f;

		/// <summary>
		/// Layer to use for the collider. Leave empty to use the same layer as the game object to which this component is attached.
		/// </summary>
		[Tooltip(
			"Layer to use for the collider. Leave empty to use the same layer as the game object to which this component is attached.")]
		[SerializeField]
		private string colliderLayer;

		/// <summary>
		/// Layers to test for collision.
		/// </summary>
		[Tooltip("Layers to test for collision.")]
		[SerializeField]
		private LayerMask collisionLayerMask;

		/// <summary>
		/// Add a kinematic Rigidbody? Physics works better when moving Colliders have a kinematic Rigidbody.
		/// </summary>
		[Tooltip("Add a kinematic Rigidbody? Physics works better when moving Colliders have a kinematic Rigidbody.")]
		[SerializeField]
		private bool addKinematicRigidbody = true;

		/// <summary>
		/// The capsule collider.
		/// </summary>
		private CapsuleCollider capsuleCollider;

		/// <summary>
		/// Cached reference to the transform.
		/// </summary>
		private Transform cachedTransform;

		/// <summary>
		/// The capsule center with the relevant scaling applied (e.g. if object scale is not 1,1,1)
		/// </summary>
		public Vector3 scaledCenter
		{
			get
			{
				Vector3 scale = transform.lossyScale;
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
				Vector3 scale = transform.lossyScale;
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
				return height * transform.lossyScale.y;
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
		/// Returns a point on the capsule collider that is closest to a given location.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public Vector3 GetClosestPointOnCapsule(Vector3 position)
		{
			return capsuleCollider.ClosestPoint(position);
		}
		
		/// <inheritdoc />
		private void Awake()
		{
			cachedTransform = transform;
			InitCapsuleCollider();
		}

		/// <summary>
		/// Initialize the capsule collider.
		/// </summary>
		private void InitCapsuleCollider()
		{
			// Create a child object and add the capsule collider to it
			GameObject go = new GameObject("CapsuleCollider");
			go.transform.SetParent(cachedTransform);
			if (string.IsNullOrEmpty(colliderLayer))
			{
				go.layer = gameObject.layer;
			}
			else
			{
				go.layer = LayerMask.NameToLayer(colliderLayer);
			}

			capsuleCollider = (CapsuleCollider)go.AddComponent(typeof(CapsuleCollider));
			if (capsuleCollider != null)
			{
				// Must be enabled for ComputePenetration to work.
				capsuleCollider.enabled = true;

				capsuleCollider.transform.localPosition = Vector3.zero;
				capsuleCollider.transform.localRotation = Quaternion.identity;
				capsuleCollider.transform.localScale = Vector3.one;
			}

			if (addKinematicRigidbody)
			{
				Rigidbody rigidbody = (Rigidbody)go.AddComponent(typeof(Rigidbody));
				if (rigidbody != null)
				{
					rigidbody.isKinematic = true;
					rigidbody.useGravity = false;
				}
			}

			ValidateCapsule();
		}

		/// <summary>
		/// Call this when the capsule's values change.
		/// </summary>
		private void ValidateCapsule()
		{
			slopeLimit = Mathf.Clamp(slopeLimit, 0.0f, k_MaxSlopeLimit);
			skinWidth = Mathf.Clamp(skinWidth, k_MinSkinWidth, float.MaxValue);
			
			if (capsuleCollider != null)
			{
				capsuleCollider.center = center;
				capsuleCollider.radius = radius;
				capsuleCollider.height = height;
			}
		}

		#if UNITY_EDITOR
		/// <inheritdoc />
		private void OnValidate()
		{
			ValidateCapsule();
		}

		/// <inheritdoc />
		private void OnDrawGizmosSelected()
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
			
			if (capsuleCollider != null)
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
	}
}