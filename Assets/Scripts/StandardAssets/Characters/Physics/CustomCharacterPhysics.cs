using System;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace StandardAssets.Characters.Physics
{
	public class CustomCharacterPhysics : MonoBehaviour, ICharacterPhysics
	{

		[Header("Player")]
		[Tooltip("The root bone in the avatar - used to compensate for the animator issues")]
		public GameObject playerRootTransform;
		public Vector3 playerCentre = new Vector3(0,0,0);
		public float playerHeight = 1.56f;
		public float playerRadius = 0.45f;

		[Header("Gravity")]
		public float gravity = 2.8f;

		[Header("Grounding")]
		public float maxGroundingDist = 50;
		[Tooltip("The point at which we check that the player is grounded")]
		public Vector3 groundCheckPoint = new Vector3(0, -0.85f, 0);
		public bool enableInterpolation = true;
		public float groundCheckRadius = 0.35f;
		public float pushupDist;
		public float interpolationSpeed = 10f;

		[Header("Jumping")]
		public float jumpDecrease = 3f;
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
		public SphereCollider playerSphereCollider;

		[Header("Debug")]
		[Tooltip("Enable additional debugging visuals in scene view")]
		public bool enableDebug;

		public bool isGrounded { get; private set; }
		
		public Action landed { get; set; }
		public Action jumpVelocitySet { get; set; }
		public Action startedFalling { get; set; }
		public float airTime { get; private set; }
		public float fallTime { get; private set; }

		private Vector3 groundClamp;
		private float minGroundingDist = 2f;
		private Vector3 playerVelocity;
		private Vector3 previousVelocity;
		
		//TODO: Clean up namings
		private bool inputJump;
		private bool jumpingHappening;
		private bool jumping;
		private float jumpVelocity;
		private float jumpHeight;

		private void FixedUpdate()
		{
			ApplyGravity();
			CheckGrounding();
			CheckCollision();
			HandleJump();
		}

		private void LateUpdate()
		{
			if (playerRootTransform != null)
			{
				playerRootTransform.transform.localPosition = playerCentre;
			}
		}
		

		public void Move(Vector3 moveVector3)
		{
			playerVelocity += moveVector3;
			Vector3 playerMovementVector = (new Vector3(playerVelocity.x, playerVelocity.y, playerVelocity.z));// * movementSpeed) * deltaTime ;

			transform.position += playerMovementVector;

			if (enableDebug)
			{
				Debug.DrawRay(transform.position, playerMovementVector, Color.green, 1f);
			}

			previousVelocity = playerVelocity;
			playerVelocity = Vector3.zero;
		}

		public void SetJumpVelocity(float initialJumpVelocity)
		{
			jumpVelocity = initialJumpVelocity;
			jumping = true;
		}

		private void HandleJump()
		{
			if (jumping)
			{	
				bool canJump = !UnityEngine.Physics.Raycast(new Ray(transform.position, Vector3.up), jumpCeiling, excludePlayer);

				if (isGrounded && jumpHeight > 0.2f || jumpHeight <= 0.2f && isGrounded)
				{
					jumpHeight = 0;
					inputJump = false;
				}

				if (isGrounded && canJump)
				{
					inputJump = true;
					jumpHeight += jumpVelocity;
					jumpingHappening = true;

				}
				else
				{
					if (!isGrounded)
					{
						jumpHeight -= (jumpHeight * jumpDecrease * Time.deltaTime);
					}
				}

				playerVelocity.y += jumpHeight;
			}
		}
		
		private void ApplyGravity()
		{
			if (!isGrounded)
			{
				playerVelocity.y -= gravity * Time.fixedDeltaTime;
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

			groundClamp = new Vector3(transform.position.x, groundHit.point.y + groundCheckRadius / 2, transform.position.z);
			Collider[] collisions = new Collider[3];
			int num = UnityEngine.Physics.OverlapSphereNonAlloc(transform.TransformPoint(groundCheckPoint), groundCheckRadius, collisions, excludePlayer);

			isGrounded = false;
			for (int x = 0; x < num; x++)
			{
				if (collisions[x].transform != groundHit.transform)
				{
					continue;
				}

				if (jumpingHappening)
				{
					if (landed != null)
					{
						previousVelocity = Vector3.zero;
						jumping = false;
						jumpingHappening = false;
						landed();
					}
				}
				
				isGrounded = true;
				
				if (groundHit.point.y <= transform.position.y + maxStepHeight)
				{
					SnapToGround(groundHit);
				}
				break;
			}

			if (num > 1 || inputJump || !(groundHit.distance <= minGroundingDist))
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

			if (groundHit.point.y <= transform.position.y + maxStepHeight && !inputJump)
			{
				SnapToGround(groundHit);
			}
		}

		private void SnapToGround(RaycastHit groundHit)
		{
			if (!enableInterpolation)
			{
				transform.position = new Vector3(transform.position.x, (groundHit.point.y + playerHeight / 2 + pushupDist),
				                                 transform.position.z);
			}
			else
			{
				transform.position = Vector3.Lerp(transform.position,
				                                  new Vector3(transform.position.x,
				                                              (groundHit.point.y + playerHeight / 2 + pushupDist),
				                                              transform.position.z), interpolationSpeed * Time.deltaTime);
			}
		}

		private void CheckCollision()
		{
		
			Collider[] collisions = new Collider[4];

			int numberOfCollisions = UnityEngine.Physics.OverlapSphereNonAlloc(transform.TransformPoint(playerSphereCollider.center),
			                                        playerSphereCollider.radius, collisions, excludePlayer,
			                                        QueryTriggerInteraction.UseGlobal);

			for (int x = 0; x < numberOfCollisions; x++)
			{
				Transform t = collisions[x].transform;
				Vector3 direction;
				float distance;

				if (!UnityEngine.Physics.ComputePenetration(playerSphereCollider, transform.position, transform.rotation,
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

