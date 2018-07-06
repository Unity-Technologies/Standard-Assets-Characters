using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Capsule used for a character's movement and collision detection.
	/// </summary>
	public class CharacterCapsule : MonoBehaviour
	{
		/// <summary>
		/// Center of the capsule.
		/// </summary>
		[Tooltip("Center of the capsule.")]
		public Vector3 center;

		/// <summary>
		/// Radius of the capsule.
		/// </summary>
		[Tooltip("Radius of the capsule.")]
		public float radius = 0.5f;

		/// <summary>
		/// Height of the capsule.
		/// </summary>
		[Tooltip("Height of the capsule.")]
		public float height = 2.0f;

		/// <summary>
		/// The capsule collider.
		/// </summary>
		private CapsuleCollider capsuleCollider;

		/// <summary>
		/// Cached reference to the transform.
		/// </summary>
		private Transform cachedTransform;

		/// <summary>
		/// The capsule radius in world units, with the relevant scaling applied.
		/// </summary>
		public float worldRadius
		{
			get
			{
				Vector3 scale = transform.lossyScale;
				float maxScale = Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);
				return radius * maxScale;
			}
		}
		
		/// <summary>
		/// Compute the minimal translation required to separate the character from the collider.
		/// </summary>
		/// <param name="positionOffset">Position offset to add to the capsule's position.</param>
		/// <param name="collider">The collider to test.</param>
		/// <param name="position">Position of the collider.</param>
		/// <param name="rotation">Rotation of the collider.</param>
		/// <param name="direction">Direction along which the translation required to separate the colliders apart is minimal.</param>
		/// <param name="distance">The distance along direction that is required to separate the colliders apart.</param>
		/// <returns></returns>
		public bool ComputePenetration(Vector3 positionOffset,
										Collider collider, Vector3 position, Quaternion rotation, 
		                               out Vector3 direction, out float distance)
		{
			if (collider == capsuleCollider)
			{
				// Skip self
				direction = Vector3.one;
				distance = 0.0f;
				return false;
			}

			return UnityEngine.Physics.ComputePenetration(capsuleCollider,
			                                              cachedTransform.position + positionOffset,
			                                              cachedTransform.rotation,
			                                              collider, position, rotation,
			                                              out direction, out distance);
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
			GameObject go = new GameObject("CapsuleCollider");
			go.transform.SetParent(cachedTransform);
			go.layer = LayerMask.NameToLayer("Ignore Raycast");
			capsuleCollider = (CapsuleCollider)go.AddComponent(typeof(CapsuleCollider));
			if (capsuleCollider != null)
			{
				// Must be enabled for ComputePenetration to work.
				capsuleCollider.enabled = true;

				capsuleCollider.transform.localPosition = Vector3.zero;
				capsuleCollider.transform.localRotation = Quaternion.identity;
				capsuleCollider.transform.localScale = Vector3.one;
				
				capsuleCollider.center = center;
				capsuleCollider.radius = radius;
				capsuleCollider.height = height;
			}
		}

		/// <inheritdoc />
		private void OnDrawGizmosSelected()
		{
			if (capsuleCollider != null)
			{
				// No need to draw when the collider exists, because it will draw itself
				return;
			}
			
			// Draw fake capsule collider
			Gizmos.color = Color.green;
			Vector3 scale = transform.lossyScale;
			float scaledRadius = worldRadius;
			Vector3 scaledCenter = new Vector3(center.x * scale.x, center.y * scale.y, center.z * scale.z);
			Vector3 sphereOffsetY = Vector3.up * (height * scale.y * 0.5f - scaledRadius);
			Vector3 topSphereCenter = transform.position + scaledCenter + sphereOffsetY;
			Vector3 bottomSphereCenter = transform.position + scaledCenter - sphereOffsetY;
			// Spheres
			Gizmos.DrawWireSphere(topSphereCenter, scaledRadius);
			Gizmos.DrawWireSphere(bottomSphereCenter, scaledRadius);
			// 4 lines on the sides
			Gizmos.DrawLine(topSphereCenter + Vector3.right * scaledRadius, 
			                bottomSphereCenter + Vector3.right * scaledRadius);
			Gizmos.DrawLine(topSphereCenter + Vector3.left * scaledRadius, 
			                bottomSphereCenter + Vector3.left * scaledRadius);
			Gizmos.DrawLine(topSphereCenter + Vector3.forward * scaledRadius, 
			                bottomSphereCenter + Vector3.forward * scaledRadius);
			Gizmos.DrawLine(topSphereCenter + Vector3.back * scaledRadius, 
			                bottomSphereCenter + Vector3.back * scaledRadius);
		}
	}
}