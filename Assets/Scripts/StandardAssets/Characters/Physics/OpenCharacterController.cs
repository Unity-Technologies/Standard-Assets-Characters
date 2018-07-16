using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	public class OpenCharacterController : MonoBehaviour
	{
		/// <summary>
		/// The root bone in the avatar.
		/// </summary>
		[Header("Player")]
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
		/// The character capsule.
		/// </summary>
		[Header("Collision")]
		[SerializeField]
		[Tooltip("The character capsule.")]
		private CharacterCapsule characterCapsule;

		[Header("Debug")]
		[Tooltip("Enable additional debugging visuals in scene view")]
		[SerializeField]
		private bool enableDebug;

		/// <summary>
		/// The character capsule mover.
		/// </summary>
		private ICharacterCapsuleMover capsuleMover;

		/// <summary>
		/// Was touching the ground during the last move, or when the position was set via SetPosition?
		/// </summary>
		public bool isGrounded
		{
			get
			{
				return capsuleMover.isGrounded;
			}
		}
		
		/// <summary>
		/// Velocity of the last movement. It's the new position minus the old position.
		/// </summary>
		public Vector3 velocity
		{
			get
			{
				return capsuleMover.velocity;
			}
		}

		/// <summary>
		/// What part of the capsule collided with the environment during the last Move call.
		/// </summary>
		public CollisionFlags collisionFlags
		{
			get
			{
				return capsuleMover.collisionFlags;
			}
		}

		/// <inheritdoc />
		public float GetPredicitedFallDistance()
		{
			RaycastHit groundHit;
			bool hit = UnityEngine.Physics.Raycast(characterCapsule.GetFootWorldPosition(),
			                                       Vector3.down,
			                                       out groundHit,
			                                       float.MaxValue,
			                                       characterCapsule.GetCollisionLayerMask());
			float landAngle = hit
				? Vector3.Angle(Vector3.up, groundHit.normal)
				: 0.0f;
			return hit && landAngle <= characterCapsule.GetSlopeLimit() 
				? groundHit.distance 
				: float.MaxValue;
		}

		/// <summary>
		/// Move the character. This function does not apply any gravity.
		/// </summary>
		/// <param name="moveVector">Move along this vector.</param>
		/// <returns>CollisionFlags is the summary of collisions that occurred during the Move.</returns>
		public CollisionFlags Move(Vector3 moveVector)
		{
			capsuleMover.Move(moveVector);

			#if UNITY_EDITOR
			if (enableDebug)
			{
				Debug.DrawRay(transform.position + rootTransformOffset, moveVector, Color.green, 1f);
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
			bool result = capsuleMover.SimpleMove(speed);

			#if UNITY_EDITOR
			if (enableDebug)
			{
				Debug.DrawRay(transform.position + rootTransformOffset, speed, Color.green, 1f);
			}
			#endif

			return result;
		}

		/// <summary>
		/// Set the position of the character.
		/// </summary>
		/// <param name="position">Position to set.</param>
		/// <param name="updateGrounded">Update the grounded state? This uses a cast, so only set it to true if you need it.</param>
		public void SetPosition(Vector3 position, bool updateGrounded)
		{
			capsuleMover.SetPosition(position, updateGrounded);
		}

		/// <inheritdoc />
		private void Awake()
		{
			if (characterCapsule == null)
			{
				characterCapsule = (CharacterCapsule)GetComponent(typeof(CharacterCapsule));
			}
			capsuleMover = (ICharacterCapsuleMover)GetComponentInChildren(typeof(ICharacterCapsuleMover));
		}

		#if UNITY_EDITOR
		/// <inheritdoc />
		private void OnValidate()
		{
			if (characterCapsule == null)
			{
				characterCapsule = (CharacterCapsule)GetComponent(typeof(CharacterCapsule));
			}
		}
		#endif

		/// <inheritdoc />
		private void LateUpdate()
		{
			if (playerRootTransform != null)
			{
				playerRootTransform.localPosition = rootTransformOffset;
			}
		}
	}
}