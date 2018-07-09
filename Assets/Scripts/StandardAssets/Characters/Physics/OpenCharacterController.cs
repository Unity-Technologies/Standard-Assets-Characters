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

		/// <inheritdoc cref=""/>
		public bool isGrounded { get; private set; }
		
		/// <summary>
		/// Velocity of the last movement. It's the new position minus the old position.
		/// </summary>
		public Vector3 velocity { get; private set; }

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

		/// <inheritdoc />
		public void Move(Vector3 moveVector)
		{
			capsuleMover.Move(moveVector);

			if (enableDebug)
			{
				Debug.DrawRay(transform.position + rootTransformOffset, moveVector, Color.green, 1f);
			}
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

		/// <inheritdoc />
		private void OnEnable()
		{
			capsuleMover.onGroundedChanged -= OnGroundedChanged;
			capsuleMover.onGroundedChanged += OnGroundedChanged;
			
			capsuleMover.onVelocityChanged -= OnVelocityChanged;
			capsuleMover.onVelocityChanged += OnVelocityChanged;
		}

		/// <inheritdoc />
		private void OnDisable()
		{
			capsuleMover.onGroundedChanged -= OnGroundedChanged;
			capsuleMover.onVelocityChanged -= OnVelocityChanged;
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
		
		/// <summary>
		/// Called when the grounded state changes.
		/// </summary>
		/// <param name="onGround"></param>
		private void OnGroundedChanged(bool onGround)
		{
			isGrounded = onGround;
		}
		
		/// <summary>
		/// Called when the velocity changes.
		/// </summary>
		/// <param name="newVelocity"></param>
		private void OnVelocityChanged(Vector3 newVelocity)
		{
			velocity = newVelocity;
		}

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