using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	public class OpenCharacterController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("The root bone in the avatar - used to compensate for the animator issues")]
		[SerializeField]
		private GameObject playerRootTransform;

		/// <summary>
		/// The root transform will be positioned at this centre.
		/// </summary>
		[Tooltip("The root transform will be positioned at this centre.")]
		[SerializeField]
		private Vector3 playerCentre = new Vector3(0, 0, 0);
		
		[SerializeField]
		private float playerHeight = 1.56f;

		[Header("Grounding")]
		[SerializeField]
		private float maxGroundingDist = 50;
		
		[SerializeField]
		private float groundingEasing = 10f;

		[Tooltip("The point at which we check that the player is grounded")]
		[SerializeField]
		private Vector3 groundCheckPoint = new Vector3(0, -0.85f, 0);

		[SerializeField]
		private float groundCheckRadius = 0.35f;

		[Header("Sloping")]
		[Range(0, 90)]
		[SerializeField]
		private float minSlope = 0;

		[Range(0, 90)]
		[SerializeField]
		private float maxSlope = 60;

		[Range(0, 90)]
		[SerializeField]
		private float maxModifiedSlope = 45f;

		[SerializeField]
		private float minimumGroundingDist = 2f;

		[Header("Auto-Step")]
		[SerializeField]
		private float maxStepHeight = 1.5f;

		[SerializeField]
		private Vector3 liftSlopePoint = new Vector3(0, 1f, 0);
		
		[SerializeField]
		private float liftSlopeRadius = 0.2f;

		[Header("Layer Options")]
		[Tooltip("Select all layers except the Player layer")]
		[SerializeField]
		private LayerMask excludePlayer;

		/// <summary>
		/// The character capsule.
		/// </summary>
		[Header("Collision")]
		[SerializeField]
		[Tooltip("The character capsule.")]
		private CharacterCapsule characterCapsule;

		/// <summary>
		/// Max colliders to test for during the penetration tests. Must be at least 1.
		/// </summary>
		[SerializeField]
		[Tooltip("Max colliders to test for during the penetration tests. Must be at least 1.")]
		private int maxPenetrationTests = 4;

		[Header("Debug")]
		[Tooltip("Enable additional debugging visuals in scene view")]
		[SerializeField]
		private bool enableDebug;

		private float minGroundingDist = 2f;
		private Vector3 playerMovement;
		private float precalculatedGroundY;

		/// <summary>
		/// Colliders to use during the penetration tests.
		/// </summary>
		private Collider[] penetrationColliders;

		public bool isGrounded { get; private set; }

		public float GetPredicitedFallDistance()
		{
			RaycastHit groundHit;
			bool hit = UnityEngine.Physics.Raycast(transform.TransformPoint(groundCheckPoint), Vector3.down, 
											   out groundHit, float.MaxValue, excludePlayer);
			float landAngle = Vector3.Angle(Vector3.up, groundHit.normal);
			return hit && landAngle <= maxSlope ? groundHit.distance : float.MaxValue;
		}

		public void Move(Vector3 moveVector)
		{
			CheckPossibleCollision(moveVector, out playerMovement);
			transform.position += playerMovement;

			if (enableDebug)
			{
				Debug.DrawRay(transform.position + playerCentre, playerMovement, Color.green, 1f);
			}
		}

		private void Awake()
		{
			if (characterCapsule == null)
			{
				characterCapsule = (CharacterCapsule)GetComponent(typeof(CharacterCapsule));
			}
			penetrationColliders = new Collider[maxPenetrationTests];
		}

		private void FixedUpdate()
		{	
			CheckGrounding();
			CheckCollisionPenetrations();
			EaseGrounding();
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

			RaycastHit groundHit;
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

			isGrounded = false;
			if (!(currentSlope >= minSlope) || !(currentSlope <= maxSlope))
			{
				return;
			}

			Collider[] collisions = new Collider[3];
			int num = UnityEngine.Physics.OverlapSphereNonAlloc(transform.TransformPoint(groundCheckPoint),
																groundCheckRadius, collisions, excludePlayer);

			for (int x = 0; x < num; x++)
			{
				if (collisions[x].transform != groundHit.transform)
				{
					continue;
				}

				isGrounded = true;
				if (groundHit.point.y <= transform.position.y + maxStepHeight)
				{
					PrecalculateGroundY(groundHit);
				}

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

			if (groundHit.point.y <= transform.position.y + maxStepHeight && playerMovement.y < 0)
			{
				PrecalculateGroundY(groundHit);
			}
		}

		/// <summary>
		/// Check if the character penetrates other colliders, and push the character out of them.
		/// </summary>
		private void CheckCollisionPenetrations()
		{
			// TODO: Need a better way to determine excludeGroundCheckHeight (here we exclude it when moving down, i.e. gravity)
			bool excludeGroundCheckHeight = playerMovement.y < 0.0f;
			Vector3? safeVector = GetCollisionFreeVector(Vector3.zero, -playerMovement, excludeGroundCheckHeight);
			if (safeVector == null)
			{
				return;
			}

			transform.position += safeVector.Value;
			
			// TODO: Why using projection?
			Vector3 velocityProjected = Vector3.Project(playerMovement, -safeVector.Value.normalized);
			playerMovement -= velocityProjected;
		}

		/// <summary>
		/// Check for possible collisions along the movement vector.
		/// </summary>
		/// <param name="moveVector">The movement vector.</param>
		/// <param name="getMoveVector">Get the adjusted movement vector. It will be the same as moveVector if there are no possible collisions.</param>
		/// <returns>True if possible collisions.</returns>
		private bool CheckPossibleCollision(Vector3 moveVector, out Vector3 getMoveVector)
		{
			getMoveVector = moveVector;

			bool foundPossibleCollision = false;
			Vector3 movedPosition = Vector3.zero;
			Vector3 previousPosition = movedPosition;
			// TODO: Expose possibleCollisionMaxSteps in the inspector
			int possibleCollisionMaxSteps = 10;
			for (int i = 0; i < possibleCollisionMaxSteps; i++)
			{
				if (CheckPossibleCollisionStep(getMoveVector, out getMoveVector, 
				                               movedPosition))
				{
					foundPossibleCollision = true;

					Vector3? safeVector = GetCollisionFreeVector(movedPosition + getMoveVector, -getMoveVector, true);
					if (safeVector != null)
					{
						// Move to a safe position
						previousPosition = movedPosition;
						movedPosition = movedPosition + getMoveVector;
						movedPosition = movedPosition + safeVector.Value;
						getMoveVector = movedPosition - previousPosition;
					}
					else
					{
						previousPosition = movedPosition;
						movedPosition += getMoveVector;
					}
				}
				else
				{					
					break;
				}
			}
			
			return foundPossibleCollision;
		}
		
		/// <summary>
		/// A single step for checking possible collisions along the movement vector.
		/// </summary>
		/// <param name="moveVector">The movement vector.</param>
		/// <param name="getMoveVector">Get the adjusted movement vector.</param>
		/// <param name="movedPosition">The local position moved by the previous steps.</param>
		/// <returns></returns>
		private bool CheckPossibleCollisionStep(Vector3 moveVector, out Vector3 getMoveVector,
		                                        Vector3 movedPosition)
		{
			getMoveVector = moveVector;
			
			Vector3 direction = moveVector.normalized;
			float distance = moveVector.magnitude;
			float testDistance = distance;
			Vector3 topSphereCenter;
			Vector3 bottomSphereCenter;
			GetCapsuleTopAndBottomCenters(out topSphereCenter, out bottomSphereCenter, true);

			RaycastHit hitInfo;
			if (!UnityEngine.Physics.CapsuleCast(transform.TransformPoint(topSphereCenter) + movedPosition,
			                                     transform.TransformPoint(bottomSphereCenter) + movedPosition,
			                                     characterCapsule.worldRadius,
			                                     direction,
			                                     out hitInfo,
			                                     testDistance,
			                                     excludePlayer,
			                                     QueryTriggerInteraction.UseGlobal))
			{
				return false;
			}

			// Bounce (reflect) off the obstacle
			Vector3 reflect = Vector3.Reflect(direction * testDistance, hitInfo.normal);

			getMoveVector = (direction * hitInfo.distance) + (reflect.normalized * (testDistance - hitInfo.distance));
			return true;
		}

		/// <summary>
		/// Get a movement vector, which will move the character to a position where there are no collisions.
		/// </summary>
		/// <param name="position">Offset position.</param>
		/// <param name="preferredDirection">The preferred direction to be pushed to.</param>
		/// <param name="excludeGroundCheckHeight">Exclude ground check height in the test? (i.e. do not test for collision at the character's feet)</param>
		/// <returns>The movement vector. Null if current position is safe, or no safe position could be found.</returns>
		private Vector3? GetCollisionFreeVector(Vector3 position, Vector3 preferredDirection, bool excludeGroundCheckHeight)
		{
			Vector3 topSphereCenter;
			Vector3 bottomSphereCenter;
			GetCapsuleTopAndBottomCenters(out topSphereCenter, out bottomSphereCenter, excludeGroundCheckHeight);

			int collisionCount = UnityEngine.Physics.OverlapCapsuleNonAlloc(transform.TransformPoint(topSphereCenter) + position,
			                                                                transform.TransformPoint(bottomSphereCenter) + position,
			                                                                characterCapsule.worldRadius,
			                                                                penetrationColliders,
			                                                                excludePlayer,
			                                                                QueryTriggerInteraction.UseGlobal);

			if (collisionCount <= 0)
			{
				return null;
			}

			Vector3 preferredDirectionNormalized = preferredDirection.normalized;
			Vector3? safeDirection = null;
			float safeDistance = 0.0f;
			float safeAngle = 0.0f;
			Vector3? possibleSafeDirection = null;
			float possibleDistance = 0.0f;
			float possibleAngle = 0.0f;
			
			for (int i = 0; i < collisionCount; i++)
			{
				Collider collider = penetrationColliders[i];
				Transform t = collider.transform;
				Vector3 direction;
				float distance;

				if (!characterCapsule.ComputePenetration(position,
				                                         collider, t.position, t.rotation,
				                                         out direction, out distance))
				{
					continue;
				}

				float angle = Vector3.Angle(direction, preferredDirectionNormalized);
				
				if (possibleSafeDirection == null ||
				    possibleAngle > angle)
				{
					possibleSafeDirection = direction;
					possibleDistance = distance;
					possibleAngle = angle;
				}
				
				// We try to get the preferred direction, so ignore large angles
				if (angle > 90)
				{
					continue;
				}

				if (safeDirection == null ||
				    safeAngle > angle)
				{
					safeDirection = direction;
					safeDistance = distance;
					safeAngle = angle;
				}
			}

			if (safeDirection == null &&
			    possibleSafeDirection != null &&
			    possibleAngle < 91)
			{
				// Test if the possible direction has no collision, for cases where angle is just more
				// than 90 degrees (e.g. gravity pushing a character down and back away from an obstacle)
				Vector3 testPosition = position + possibleSafeDirection.Value * possibleDistance;
				int count = UnityEngine.Physics.OverlapCapsuleNonAlloc(transform.TransformPoint(topSphereCenter) + testPosition,
				                                                       transform.TransformPoint(bottomSphereCenter) + testPosition,
				                                                       characterCapsule.worldRadius,
				                                                       penetrationColliders,
				                                                       excludePlayer,
				                                                       QueryTriggerInteraction.UseGlobal);
				if (count <= 0)
				{
					safeDirection = possibleSafeDirection;
					safeDistance = possibleDistance;
				}
			}

			if (safeDirection == null)
			{				
				return null;
			}

			return safeDirection * safeDistance;
		}
		
		/// <summary>
		/// Get the capsule's top and bottom center local positions.
		/// </summary>
		/// <param name="getTop">Get top position.</param>
		/// <param name="getBottom">Get bottom position.</param>
		/// <param name="excludeGroundCheckHeight">Move the bottom position up to exclude the ground check height?</param>
		private void GetCapsuleTopAndBottomCenters(out Vector3 getTop, out Vector3 getBottom, bool excludeGroundCheckHeight)
		{
			Vector3 sphereOffsetY = Vector3.up * (characterCapsule.height * 0.5f - characterCapsule.radius);
			float capsuleBottom = characterCapsule.center.y - sphereOffsetY.y - characterCapsule.radius;
			// TODO: Should be using "groundCheckTop = groundCheckPoint.y + groundCheckRadius", but there's a bug
			//float groundCheckTop = groundCheckPoint.y + groundCheckRadius;
			float groundCheckTop = -(playerHeight / 2.0f) + (maxStepHeight / 2.0f);
			Vector3 bottomOffset = excludeGroundCheckHeight
				? Vector3.up * (groundCheckTop - capsuleBottom)
				: Vector3.zero;
			getTop = characterCapsule.center + sphereOffsetY;
			getBottom = characterCapsule.center - sphereOffsetY + bottomOffset;
		}

		private void OnDrawGizmos()
		{
			if (enableDebug)
			{
				if (characterCapsule == null)
				{
					characterCapsule = (CharacterCapsule)gameObject.GetComponent(typeof(CharacterCapsule));
				}
				float tempDiameter = characterCapsule != null
					? characterCapsule.radius * 2.0f
					: 1.0f;
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireCube(transform.position, new Vector3(tempDiameter, playerHeight, tempDiameter));

				Gizmos.color = !isGrounded ? Color.red : Color.blue;

				Gizmos.DrawWireSphere(transform.TransformPoint(groundCheckPoint), groundCheckRadius);
			}
		}

		private void PrecalculateGroundY(RaycastHit groundHit)
		{
			precalculatedGroundY = (groundHit.point.y + playerHeight / 2);
		}

		private void EaseGrounding()
		{
			if (!isGrounded)
			{
				return;
			}

			transform.position = Vector3.Lerp(transform.position,
											  new Vector3(transform.position.x,
														  precalculatedGroundY,
														  transform.position.z), groundingEasing * Time.fixedDeltaTime);
		}
	}
}