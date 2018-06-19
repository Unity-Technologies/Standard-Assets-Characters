using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	public class OpenCharacterController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("The root bone in the avatar - used to compensate for the animator issues")]
		public GameObject playerRootTransform;

		public Vector3 playerCentre = new Vector3(0, 0, 0);
		public float playerHeight = 1.56f;
		public float playerRadius = 0.45f;

		[Header("Grounding")]
		public float maxGroundingDist = 50;

		[Tooltip("The point at which we check that the player is grounded")]
		public Vector3 groundCheckPoint = new Vector3(0, -0.85f, 0);

		public float groundCheckRadius = 0.35f;

		[Header("Jumping")]
		public float jumpCeiling = 2f;

		[Header("Sloping")]
		[Range(0, 90)]
		public float minSlope = 0;

		[Range(0, 90)]
		public float maxSlope = 60;

		[Range(0, 90)]
		public float maxModifiedSlope = 45f;

		public float minimumGroundingDist = 2f;

		[Header("Auto-Step")]
		public float maxStepHeight = 1.5f;

		public Vector3 liftSlopePoint = new Vector3(0, 1f, 0);
		public float liftSlopeRadius = 0.2f;

		[Header("Layer Options")]
		[Tooltip("Select all layers except the Player layer")]
		public LayerMask excludePlayer;

		[Header("Collision")]
		public CapsuleCollider capsuleCollider;

		[Header("Debug")]
		[Tooltip("Enable additional debugging visuals in scene view")]
		public bool enableDebug;

		private Vector3 groundClamp;
		private float minGroundingDist = 2f;
		private Vector3 playerVelocity;

		public bool isGrounded { get; private set; }
		
		public void Move(Vector3 moveVector)
		{
			playerVelocity += moveVector;
			Vector3 playerMovementVector = (new Vector3(playerVelocity.x, playerVelocity.y, playerVelocity.z));

			transform.position += playerMovementVector;

			if (enableDebug)
			{
				Debug.DrawRay(transform.position, playerMovementVector, Color.green, 1f);
			}

			playerVelocity = Vector3.zero;
		}

		private void Awake()
		{
			if (capsuleCollider == null)
			{
				capsuleCollider = GetComponent<CapsuleCollider>();
			}
		}

		private void FixedUpdate()
		{
			CheckGrounding();
			CheckCollision();
		}

		private void LateUpdate()
		{
			if (playerRootTransform != null)
			{
				playerRootTransform.transform.localPosition = playerCentre;
			}
		}


		private void CheckGrounding()
		{
			var ray = new Ray(transform.TransformPoint(liftSlopePoint), Vector3.down);

			var groundHit = new RaycastHit();

			if (UnityEngine.Physics.SphereCast(ray, liftSlopeRadius, out groundHit, maxGroundingDist, excludePlayer))
			{
				ConfirmGround(groundHit);
			}
			else
			{
				isGrounded = false;
			}
		}

		private void ConfirmGround(RaycastHit groundHit)
		{
			float currentSlope = Vector3.Angle(groundHit.normal, Vector3.up);
			minGroundingDist = currentSlope >= maxModifiedSlope ? minimumGroundingDist * 2 : minimumGroundingDist;

			if (!(currentSlope >= minSlope) || !(currentSlope <= maxSlope))
			{
				return;
			}

			groundClamp = new Vector3(transform.position.x, groundHit.point.y + groundCheckRadius / 2,
			                          transform.position.z);
			Collider[] collisions = new Collider[3];
			int num = UnityEngine.Physics.OverlapSphereNonAlloc(transform.TransformPoint(groundCheckPoint),
			                                                    groundCheckRadius, collisions, excludePlayer);

			isGrounded = false;
			for (int x = 0; x < num; x++)
			{
				if (collisions[x].transform != groundHit.transform)
				{
					continue;
				}

				isGrounded = true;
				break;
			}

			if (num > 1 || !(groundHit.distance <= minGroundingDist))
			{
				return;
			}

			if (collisions[0] != null)
			{
				var ray = new Ray(transform.TransformPoint(liftSlopePoint), Vector3.down);
				RaycastHit hit;

				if (UnityEngine.Physics.Raycast(ray, out hit, minGroundingDist, excludePlayer))
				{
					if (hit.transform != collisions[0].transform)
					{
						isGrounded = false;
						return;
					}
				}
			}

			isGrounded = true;
		}

		private void CheckCollision()
		{
			Collider[] collisions = new Collider[4];

			Vector3 yOffset = Vector3.up * (capsuleCollider.height * 0.5f - capsuleCollider.radius);
			Vector3 point0 = capsuleCollider.center + yOffset;
			Vector3 point1 = capsuleCollider.center - yOffset;
			                 

			int numberOfCollisions = UnityEngine.Physics.OverlapCapsuleNonAlloc(
				transform.TransformPoint(point0),
				transform.TransformPoint(point1),
				capsuleCollider.radius, collisions, excludePlayer,
				QueryTriggerInteraction.UseGlobal);

			for (int x = 0; x < numberOfCollisions; x++)
			{
				Transform t = collisions[x].transform;
				Vector3 direction;
				float distance;

				if (!UnityEngine.Physics.ComputePenetration(capsuleCollider, transform.position, transform.rotation,
				                                            collisions[x], t.position, t.rotation,
				                                            out direction, out distance))
				{
					continue;
				}

				Vector3 penetrationVector = direction * distance;
				Vector3 velocityProjected = Vector3.Project(playerVelocity, -direction);
				transform.position = transform.position + penetrationVector;
				playerVelocity -= velocityProjected;
			}
		}

		private void OnDrawGizmos()
		{
			if (enableDebug)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawWireCube(transform.position, new Vector3(playerRadius, playerHeight, playerRadius));

				Gizmos.color = !isGrounded ? Color.red : Color.blue;

				Gizmos.DrawWireSphere(transform.TransformPoint(groundCheckPoint), groundCheckRadius);
			}
		}
	}
}