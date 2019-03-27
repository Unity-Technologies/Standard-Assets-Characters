using System;
using Cinemachine;
using StandardAssets.Characters.Helpers;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityPhysics = UnityEngine.Physics;

namespace StandardAssets.Characters.Common
{
	/// <summary>
	/// Abstract bass class for character brains
	/// </summary>
	[RequireComponent(typeof(OpenCharacterController))]
	public abstract class CharacterBrain : MonoBehaviour
	{
		[SerializeField, Tooltip("Settings for the OpenCharacterController.")]
		ControllerAdapter m_OCCSettings;

		// cached last position vector
		Vector3 m_LastPosition;
		
		/// <summary>
		/// Gets the <see cref="ControllerAdapter"/> used to move the character
		/// </summary>
		public ControllerAdapter controllerAdapter { get { return m_OCCSettings; } }
		
		/// <summary>
		/// Gets the planar speed (i.e. ignoring the displacement) of the CharacterBrain
		/// </summary>
		public float planarSpeed { get; private set; }
		
		/// <summary>
		/// Gets the normalized forward speed of the character
		/// </summary>
		public abstract float normalizedForwardSpeed { get;}

		/// <summary>
		/// Gets/sets the target Y Rotation of the character
		/// </summary>
		public abstract float targetYRotation { get; set; }
		
		/// <summary>
		/// Gets the planar displacement vector of the character
		/// </summary>
		protected Vector3 planarDisplacement { get; private set; }

		/// <summary>
		/// Get controller adapters and input on Awake
		/// </summary>
		protected virtual void Awake()
		{
			m_LastPosition = transform.position;

			if (m_OCCSettings != null)
			{
				m_OCCSettings.Awake(transform);
			}
			else
			{
				gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Call the controller adapter's OnEnable
		/// </summary>
		protected virtual void OnEnable()
		{
			if (m_OCCSettings != null)
			{
				m_OCCSettings.OnEnable();
			}
		}
		
		/// <summary>
		/// Call the controller adapter's OnDisable
		/// </summary>
		protected virtual void OnDisable()
		{
			if (m_OCCSettings != null)
			{
				m_OCCSettings.OnDisable();
			}
		}

#if UNITY_EDITOR
		void OnDrawGizmosSelected()
		{
			controllerAdapter.OnDrawGizmosSelected();
		}
#endif
		
		/// <summary>
		/// Calculates the planarSpeed of the CharacterBrain
		/// </summary>
		protected virtual void Update()
		{
			var newPosition = transform.position;
			newPosition.y = 0f;
			planarDisplacement = m_LastPosition - newPosition;
			var displacement = planarDisplacement.magnitude;
			if (Time.deltaTime < Mathf.Epsilon)
			{
				planarSpeed = 0;
			}
			else
			{
				planarSpeed = displacement / Time.deltaTime;
			}
			m_LastPosition = newPosition;
		}

		/// <summary>
		/// Can moving platforms rotate the current camera?
		/// </summary>
		public abstract bool MovingPlatformCanRotateCamera();
	}

	/// <summary>
	/// Wrapper for the OpenCharacterController
	/// </summary>
	[Serializable]
	public class ControllerAdapter
	{
		// Scales used to adjust gravity depending on aerial state.
		[Serializable]
		class GravityScales
		{
			[SerializeField, Tooltip("Curve that defines the gravity scale to be applied during the rising motion of a jump, relative to the character's forward speed")]
			AnimationCurve m_JumpHold = AnimationCurve.Linear(0.0f, 0.85f, 1.0f, 0.8f);

			[SerializeField, Tooltip("Curve that defines the gravity scale applied during the rising motion of a jump if the jump button is no longer being held, relative to the character's forward speed")]
			AnimationCurve m_JumpRelease = AnimationCurve.Linear(0.0f, 1.5f, 1.0f, 2.0f);

			[SerializeField, Tooltip("Curve that defines the gravity scale to be applied while falling, relative to the character's forward speed")]
			AnimationCurve m_Falling = AnimationCurve.Linear(0.0f, 1.6f, 1.0f, 1.4f);
		
			[SerializeField, Tooltip("Gravity scale applied when falling less that the Grounding Distance (set below)")]
			float m_Grounding = 5.0f;

			public float grounding
			{
				get { return m_Grounding; }
			}

			public AnimationCurve falling
			{
				get { return m_Falling; }
			}

			public AnimationCurve jumpRelease
			{
				get { return m_JumpRelease; }
			}

			public AnimationCurve jumpHold
			{
				get { return m_JumpHold; }
			}
		}

		// Moving platforms fields and properties
		[Serializable]
		class MovingPlatforms
		{
			[SerializeField, Tooltip("Can moving platforms rotate the cameras? (E.g. Set to true for a human player if " +
			                         "the player has a first person or strafe camera)")]
			bool m_CanPlatformsRotateCameras;

			[SerializeField, Tooltip("Set this to true for human controlled characters. It removes sliding on slightly " +
			                         "tilted platforms. It uses additional collision detection, so avoid using it for " +
			                         "all characters.")]
			bool m_PreventSlidingOnPlatforms;
			
			/// <summary>
			/// Can moving platforms rotate the cameras? (E.g. Set to true for a human player if the player has a first
			/// person or strafe camera.)
			/// </summary>
			public bool canRotateCameras
			{
				get { return m_CanPlatformsRotateCameras; }
			}

			/// <summary>
			/// Prevent sliding on slightly tilted platforms.
			/// </summary>
			public bool preventSliding
			{
				get { return m_PreventSlidingOnPlatforms; }
			}
			
			/// <summary>
			/// Treat an object as a moving platform when its surface normal Y is greater/equal than this (i.e. it is
			/// pointing up).
			/// </summary>
			public float minNormalY { get; set; }
			
			/// <summary>
			/// Active moving platform the character is standing on.
			/// </summary>
			public Transform activePlatform { get; set; }
			
			/// <summary>
			/// The moving platform whose properties were recorded in the previous move.
			/// </summary>
			public Transform recordedPlatform { get; set; }
			
			/// <summary>
			/// The character's position relative to the moving platform.
			/// </summary>
			public Vector3 activePlatformLocalPoint { get; set; }
			
			/// <summary>
			/// The character's position during the last platform movement.
			/// </summary>
			public Vector3 activePlatformGlobalPoint { get; set; }
			
			/// <summary>
			/// The platform's velocity. Added to the player's jump velocity when jumping while on a platform.
			/// </summary>
			public Vector3 activePlatformVelocity { get; set; }
			
			/// <summary>
			/// The platform's movement vector. This is only set when <see cref="m_PreventSlidingOnPlatforms"/> is false.
			/// </summary>
			public Vector3 activePlatformMoveVector { get; set; }
			
			/// <summary>
			/// The active moving platform's last position.
			/// </summary>
			public Vector3 activePlatformLastPosition { get; set; }

			/// <summary>
			/// Gets whether the player is on a platform that has moved.
			/// </summary>
			public bool didPlatformMove
			{
				get { return  activePlatform != null && activePlatformLastPosition != activePlatform.position; }
			}
			
			/// <summary>
			/// The character's rotation relative to the moving platform.
			/// </summary>
			public Quaternion activePlatformLocalRotation { get; set; }
			
			/// <summary>
			/// The character's rotation during the last platform movement.
			/// </summary>
			public Quaternion activePlatformGlobalRotation { get; set; }
			
			/// <summary>
			/// The rotation the moving platform applied to the character.
			/// </summary>
			public Quaternion activePlatformRotation { get; set; }
			
			/// <summary>
			/// Horizontal force to add to the character when jumping off a moving platform.
			/// </summary>
			public Vector3 aerialVector { get; set; }
			
			/// <summary>
			/// The Cinemachine brain that controls the character's cameras.
			/// </summary>
			public CinemachineBrain cinemachineBrain { get; set; }
			
			/// <summary>
			/// The live camera's max rotation speed of its horizontal axis.
			/// </summary>
			public float? cameraMaxRotationSpeed { get; set; }
		}
		
		// the number of time sets used for trajectory prediction
		const int k_TrajectorySteps = 60;

		// the time step used between physics trajectory steps
		const float k_TrajectoryPredictionTimeStep = 0.016f;
		
		// the scale that is used to convert character's current velocity to force applied to rigidbody
		const float k_VelocityToForceScale = 0.1f;
		
		// Check for moving platforms this distance below the character.
		const float k_TestPlatformDistance = 0.01f;

		// Max angle of a moving platform's surface normal, relative to the world up vector. If angle is greater than
		// this then the object is not considered to be a moving platform.
		const float k_MaxPlatformAngle = 60.0f;
		
		[SerializeField, Tooltip("Maximum speed that the character can move downwards")]
		float m_TerminalVelocity = 10f;

		[SerializeField, Tooltip("Scales used to adjust gravity depending on aerial state.")]
		GravityScales m_GravityScales;

		[SerializeField, Tooltip("A fall less than this will trigger 'grounding'. GroundingGravityScale is applied to quickly ground the character.")]
		float m_GroundingDistance = 0.5f;

		[SerializeField, Tooltip("How quickly the Gravity that affects the character is allowed to change, when it is" +
		                         " being dynamically modified by Jumping or Falling Gravity modifiers")]
		float m_MaxGravityDelta = 10f;

		[SerializeField, Tooltip("Moving platform properties")]
		MovingPlatforms m_MovingPlatforms;
		
		// Event for when the character lands
		public event Action landed;

		// Event for jump
		public event Action jumpVelocitySet;

		// Event for falling, includes the predicted fall distance
		public event Action<float> startedFalling;

		// colliders used in physics checks for landing prediction
		readonly Collider[] m_TrajectoryPredictionColliders = new Collider[1];

		// the current value of gravity
		float m_Gravity;

		// the source of the normalized forward speed
		CharacterBrain m_CharacterBrain;

		// The predicted landing position of the character. Null if a position could not be predicted
		Vector3? m_PredictedLandingPosition;

		// The initial jump velocity
		float m_InitialJumpVelocity;

		// The current vertical velocity
		float m_CurrentVerticalVelocity;

		// The last used ground (vertical velocity excluded ie 0) velocity
		Vector3 m_CachedGroundVelocity;

		// The current vertical vector
		Vector3 m_VerticalVector = Vector3.zero;

		// Cached character input
		CharacterInput m_CharacterInput;

		// If the character is being 'grounded' by a gravity multiplier (m_GroundingGravityScale) during a short fall (m_GroundingDistance);
		bool m_IsGrounding;

		// Did the character jump
		bool m_DidJump;

#if UNITY_EDITOR
		// Debug vector array for handling jump steps
		readonly Vector3[] m_JumpSteps = new Vector3[k_TrajectorySteps];

		// Number of array elements in debug vector array
		int m_JumpStepCount;
#endif

		/// <summary>
		/// Gets/sets if the character is grounded
		/// </summary>
		public bool isGrounded { get; private set; }
		
		/// <summary>
		/// Gets/sets the <see cref="OpenCharacterController"/> use to do movement
		/// </summary>
		public OpenCharacterController characterController { get; private set; }

		/// <summary>
		/// Gets if the character has started a slide
		/// </summary>
		public bool startedSlide { get { return characterController.startedSlide; } }

		/// <summary>
		/// Gets/sets the character's current fall time
		/// </summary>
		public float fallTime { get; private set; }

		/// <summary>
		/// Gets/sets the transform 
		/// </summary>
		public Transform cachedTransform { get; private set; }

		/// <summary>
		/// Gets/sets the character's normalized vertical speed
		/// </summary>
		public float normalizedVerticalSpeed { get; private set; }

		// Gets/sets the character's air time
		float airTime { get; set; }

		// Gets the radius of the character
		float radius { get { return characterController.scaledRadius + characterController.GetSkinWidth(); } }

		// Gets the character's world foot position
		Vector3 footWorldPosition { get { return characterController.GetFootWorldPosition(); } }

		// Gets the collision layer mask used for physics grounding
		LayerMask collisionLayerMask { get { return characterController.GetCollisionLayerMask(); } }		

		// Gets the current jump gravity multiplier
		float jumpGravityMultiplier { get { return m_GravityScales.jumpHold.Evaluate(m_CharacterBrain.normalizedForwardSpeed); } }
		
		// Gets the current short jump gravity multiplier
		float shortJumpGravityMultiplier { get { return m_GravityScales.jumpRelease.Evaluate( m_CharacterBrain.normalizedForwardSpeed); } }
		
		// Gets the current fall gravity multiplier
		float fallGravityMultiplier { get { return m_GravityScales.falling.Evaluate(m_CharacterBrain.normalizedForwardSpeed); } }

		/// <summary>
		/// Moves the character
		/// </summary>
		/// <param name="moveVector">Movement vector</param>
		/// <param name="deltaTime">Time since last call</param>
		public void Move(Vector3 moveVector, float deltaTime)
		{
			isGrounded = characterController.isGrounded;
			Transform previousActivePlatform = m_MovingPlatforms.activePlatform;
			UpdateMovingPlatform(deltaTime);
			AerialMovement(deltaTime);
			MoveCharacter(moveVector + m_VerticalVector + m_MovingPlatforms.activePlatformMoveVector +
			              m_MovingPlatforms.aerialVector * deltaTime);
			m_CachedGroundVelocity = moveVector / deltaTime;
			PostUpdateMovingPlatform(previousActivePlatform);
		}
		
		/// <summary>
		/// Calculates whether the current fall is defined as a short fall
		/// </summary>
		/// <returns>True is the current fall distance is less than <see cref="m_GroundingDistance"/> false otherwise</returns>
		public bool IsPredictedFallShort(out float distance)
		{
			distance = GetPredictedFallDistance();
			return distance <= m_GroundingDistance;
		}
		
		/// <summary>
		/// Calculates whether the current fall is defined as a short fall
		/// </summary>
		/// <returns>True is the current fall distance is less than <see cref="m_GroundingDistance"/> false otherwise</returns>
		public bool IsPredictedFallShort()
		{
			return GetPredictedFallDistance() <= m_GroundingDistance;
		}

		/// <summary>
		/// Tries to jump
		/// </summary>
		/// <param name="initialVelocity"></param>
		public void SetJumpVelocity(float initialVelocity)
		{
			m_DidJump = true;
			m_CurrentVerticalVelocity = m_InitialJumpVelocity = initialVelocity + 
			                                                    Mathf.Max(m_MovingPlatforms.activePlatformVelocity.y, 0.0f);
			m_MovingPlatforms.aerialVector = new Vector3(m_MovingPlatforms.activePlatformVelocity.x, 0.0f,
				m_MovingPlatforms.activePlatformVelocity.z);
			if (jumpVelocitySet != null)
			{
				jumpVelocitySet();
			}
		}
		
		/// <summary>
		/// Sets the velocity when falling.
		/// </summary>
		public void SetFallVelocity()
		{
			// If jumped, platform velocity would already have been taken into account on jump.
			if (m_DidJump)
			{
				return;
			}
			m_MovingPlatforms.aerialVector = new Vector3(m_MovingPlatforms.activePlatformVelocity.x, 0.0f,
				m_MovingPlatforms.activePlatformVelocity.z);
		}

		/// <summary>
		/// Initialization on load. This method must be manually called.
		/// </summary>
		/// <param name="transform">Transform of the game object, on which this class will do work</param>
		public virtual void Awake(Transform transform)
		{
			cachedTransform = transform;
			normalizedVerticalSpeed = 0.0f;
			m_CharacterInput = cachedTransform.GetComponent<CharacterInput>();
			m_CharacterBrain = cachedTransform.GetComponent<CharacterBrain>();
			characterController = cachedTransform.GetComponent<OpenCharacterController>();
			characterController.collision += OnCollision;

			if (m_TerminalVelocity > 0.0f)
			{
				m_TerminalVelocity = -m_TerminalVelocity;
			}

			m_Gravity = UnityPhysics.gravity.y;

			// Calculate the minimum surface normal for moving platforms
			var vector = Quaternion.Euler(k_MaxPlatformAngle, 0.0f, 0.0f) * Vector3.up;
			m_MovingPlatforms.minNormalY = vector.y;
		}

		/// <summary>
		/// Subscribe to Cinemachine's camera change event. This method must be manually called.
		/// </summary>
		public virtual void OnEnable()
		{
			if (m_MovingPlatforms.cinemachineBrain != null)
			{
				m_MovingPlatforms.cinemachineBrain.m_CameraActivatedEvent.RemoveListener(OnCameraActivated);
				m_MovingPlatforms.cinemachineBrain.m_CameraActivatedEvent.AddListener(OnCameraActivated);
				FindLiveCamera();
			}
		}

		/// <summary>
		/// Unsubscribe from Cinemachine's camera change event. This method must be manually called.
		/// </summary>
		public virtual void OnDisable()
		{
			if (m_MovingPlatforms.cinemachineBrain != null)
			{
				m_MovingPlatforms.cinemachineBrain.m_CameraActivatedEvent.RemoveListener(OnCameraActivated);
			}
		}

		// Handles interactions with collideable objects in the world
		// Applies force to Rigidbodies. Also checks if the character lands on the top of a moving platform.
		void OnCollision(OpenCharacterController.CollisionInfo collisionInfo)
		{
			OpenCharacterController controller = collisionInfo.controller;

			CheckMovingPlatformCollision(collisionInfo.moveDirection, collisionInfo.normal,
				collisionInfo.collider.transform);

			if (controller == null || controller.collisionFlags == CollisionFlags.Below || controller.collisionFlags == CollisionFlags.Above)
			{
				return;
			}
			
			Rigidbody body = collisionInfo.rigidbody;
			if (body == null)
			{
				return;
			}

			body.AddForceAtPosition(m_CachedGroundVelocity * k_VelocityToForceScale, collisionInfo.point, ForceMode.Impulse);
		}

		// Calculates the current predicted fall distance based on the predicted landing position
		// 		return: The predicted fall distance
		float GetPredictedFallDistance()
		{
			RaycastHit hitInfo;
			if (UnityPhysics.Raycast(new Ray(footWorldPosition, Vector3.down), out hitInfo,
				m_GroundingDistance, collisionLayerMask))
			{
				return hitInfo.distance;
			}
			
			UpdatePredictedLandingPosition();
			return m_PredictedLandingPosition == null
				? float.MaxValue
				: footWorldPosition.y - ((Vector3) m_PredictedLandingPosition).y;
		}	

		/// Updates the predicted landing position by stepping through the fall trajectory
		void UpdatePredictedLandingPosition()
		{
			var currentPosition = footWorldPosition;
			var moveVector = m_CachedGroundVelocity;
			var currentAirTime = 0.0f;
			for (var i = 0; i < k_TrajectorySteps; i++)
			{
				moveVector.y = Mathf.Clamp(m_Gravity * fallGravityMultiplier * currentAirTime,  m_TerminalVelocity, 
				                           Mathf.Infinity);
				currentPosition += moveVector * k_TrajectoryPredictionTimeStep;
				currentAirTime += k_TrajectoryPredictionTimeStep;
#if UNITY_EDITOR
				m_JumpSteps[i] = currentPosition;
#endif
				if (IsGroundCollision(currentPosition))
				{
#if UNITY_EDITOR
					// for gizmos
					m_JumpStepCount = i;
#endif
					m_PredictedLandingPosition = currentPosition;
					return;
				}
			}
#if UNITY_EDITOR
			m_JumpStepCount = k_TrajectorySteps;
#endif
			m_PredictedLandingPosition = null;
		}

		// Checks if the given position would collide with the ground collision layer.
		// 		position: Position to check
		// 		return: True if a ground collision would occur at the given position
		bool IsGroundCollision(Vector3 position)
		{
			// move sphere but to match bottom of character's capsule collider
			var colliderCount = UnityPhysics.OverlapSphereNonAlloc(position + new Vector3(0.0f, radius, 0.0f),
																   radius, m_TrajectoryPredictionColliders,
																   collisionLayerMask);
			return colliderCount > 0.0f;
		}

		// Handles Jumping and Falling
		void AerialMovement(float deltaTime)
		{
			// Calculates how long character has been in air and adjusts their vertical velocity accordingly
			airTime += deltaTime;
			CalculateGravity(deltaTime);
			var minVelocity = m_IsGrounding ? -Mathf.Infinity : m_TerminalVelocity;
			if (m_CurrentVerticalVelocity >= 0.0f)
			{
				m_CurrentVerticalVelocity = Mathf.Clamp(m_InitialJumpVelocity + m_Gravity * airTime, 
														minVelocity, Mathf.Infinity);
			}
			
			var previousFallTime = fallTime;

			// Checks if the character is falling
			if (m_CurrentVerticalVelocity < 0.0f)
			{
				m_CurrentVerticalVelocity = Mathf.Clamp(m_Gravity * fallTime, minVelocity, Mathf.Infinity);
				fallTime += deltaTime;
				if (isGrounded)
				{
					m_InitialJumpVelocity = 0.0f;
					m_VerticalVector = Vector3.zero;
					m_MovingPlatforms.aerialVector = Vector3.zero;

					//Play the moment that the character lands and only at that moment
					if (Math.Abs(airTime - deltaTime) > Mathf.Epsilon)
					{
						if (landed != null)
						{
							landed();
						}
						m_DidJump = false;
						m_IsGrounding = false;
					}

					fallTime = 0.0f;
					airTime = 0.0f;
					return;
				}
			}

			// Checks for the movement that the character has started to fall
			if (Mathf.Approximately(previousFallTime, 0.0f) && fallTime > Mathf.Epsilon)
			{
				var predictedFallDistance = GetPredictedFallDistance();
				if (predictedFallDistance > m_GroundingDistance)
				{
					if (startedFalling != null)
					{
						startedFalling(predictedFallDistance);
					}
				}
				else
				{
					m_IsGrounding = true;
				}
			}
			m_VerticalVector = new Vector3(0.0f, m_CurrentVerticalVelocity * deltaTime, 0.0f);
		}

		// Calculates the current gravity modified based on current vertical velocity
		void CalculateGravity(float deltaTime)
		{
			float gravityFactor;
			if (m_CurrentVerticalVelocity < 0.0f)
			{
				gravityFactor = fallGravityMultiplier;
				if (!m_DidJump && m_IsGrounding) // if a short fall was triggered increase gravity to quickly ground
				{
					gravityFactor *= m_GravityScales.grounding;
					// We don't want to slowly lerp gravity for grounding so set it here.
					m_Gravity = gravityFactor * UnityPhysics.gravity.y;
					return;
				}
				if (m_InitialJumpVelocity < Mathf.Epsilon)
				{
					normalizedVerticalSpeed = 0.0f;
				}
				else
				{
					normalizedVerticalSpeed = Mathf.Clamp(m_CurrentVerticalVelocity / 
					                                      (m_InitialJumpVelocity * gravityFactor), -1.0f, 1.0f);
				}
			}
			else
			{
				gravityFactor = jumpGravityMultiplier;
				if (m_CharacterInput != null && !m_CharacterInput.hasJumpInput) // if no input apply min jump modifier
				{
					gravityFactor *= shortJumpGravityMultiplier;
				}
				normalizedVerticalSpeed = m_InitialJumpVelocity > 0.0f ? m_CurrentVerticalVelocity / m_InitialJumpVelocity 
																	   : 0.0f;
			}

			var newGravity = gravityFactor * UnityPhysics.gravity.y;
			m_Gravity = Mathf.Lerp(m_Gravity, newGravity, deltaTime * m_MaxGravityDelta);
		}

		// Moves the character by 'movement' world units
		// 		movement: The value to move the character by in world units
		void MoveCharacter(Vector3 movement)
		{
			var collisionFlags = characterController.Move(movement);
			if ((collisionFlags & CollisionFlags.CollidedAbove) == CollisionFlags.CollidedAbove)
			{
				m_CurrentVerticalVelocity = 0f;
				m_InitialJumpVelocity = 0f;
			}
		}
		
		// Check if character collided with a potential moving platform
		void CheckMovingPlatformCollision(Vector3 hitDirection, Vector3 hitNormal, Transform hitTransform)
		{
			// Did character move down and hit an up-facing normal?
			if (hitDirection.y < 0.0f && hitNormal.y >= m_MovingPlatforms.minNormalY)
			{
				m_MovingPlatforms.activePlatform = hitTransform;
			}
		}

		// Handles moving platform movement
		void UpdateMovingPlatform(float deltaTime)
		{
			if (m_MovingPlatforms.activePlatform == null || m_MovingPlatforms.recordedPlatform == null)
			{
				m_MovingPlatforms.activePlatformVelocity = Vector3.zero;
				m_MovingPlatforms.activePlatformRotation = Quaternion.identity;
				m_MovingPlatforms.activePlatformMoveVector = Vector3.zero;
				UpdateMovingPlatformCamera(deltaTime);
				return;
			}

			var usePlatform = m_MovingPlatforms.activePlatform;
			
			// If the character just landed on the platform then process the previous platform for 1 more frame. This
			// handles the case where a platform consists of more than 1 collider (transform) and the character is
			// caught between 2 of the colliders.
			if (m_MovingPlatforms.activePlatform != m_MovingPlatforms.recordedPlatform)
			{
				usePlatform = m_MovingPlatforms.recordedPlatform;
			}
			
			// Position
			var newGlobalPlatformPoint = usePlatform.TransformPoint(m_MovingPlatforms.activePlatformLocalPoint);
			var moveDistance = (newGlobalPlatformPoint - m_MovingPlatforms.activePlatformGlobalPoint);
			if (m_MovingPlatforms.didPlatformMove)
			{
				if (!m_MovingPlatforms.preventSliding)
				{
					m_MovingPlatforms.activePlatformMoveVector = moveDistance;
				}
				else
				{
					characterController.Move(moveDistance);
				}
			}
			m_MovingPlatforms.activePlatformVelocity = moveDistance / deltaTime;
			
			// Rotation
			var newGlobalPlatformRotation = usePlatform.rotation * m_MovingPlatforms.activePlatformLocalRotation;
			var rotationDiff = newGlobalPlatformRotation * Quaternion.Inverse(m_MovingPlatforms.activePlatformGlobalRotation);
			// Prevent rotation of the local up vector
			rotationDiff = Quaternion.FromToRotation(rotationDiff * cachedTransform.up, cachedTransform.up) * rotationDiff;
			cachedTransform.rotation = rotationDiff * cachedTransform.rotation;
			m_MovingPlatforms.activePlatformRotation = rotationDiff;
			
			UpdateMovingPlatformCamera(deltaTime);
			
			m_MovingPlatforms.activePlatform = null;
		}
		
		// Rotates the camera based on the moving platform rotation (e.g. the first person or strafe camera).
		void UpdateMovingPlatformCamera(float deltaTime)
		{
			if (m_CharacterInput == null)
			{
				return;
			}
			m_CharacterInput.movingPlatformLookInput = Vector2.zero;
			if (!m_MovingPlatforms.canRotateCameras || !m_CharacterBrain.MovingPlatformCanRotateCamera() || 
			    m_MovingPlatforms.activePlatformRotation == Quaternion.identity)
			{
				return;
			}
			if (m_MovingPlatforms.cinemachineBrain == null)
			{
				FindLiveCamera();
			}
			if (m_MovingPlatforms.cameraMaxRotationSpeed == null)
			{
				return;
			}
			
			var rotation = m_MovingPlatforms.activePlatformRotation;
			// Clamp and wrap angle to range -180 to 180
			var angle = -rotation.eulerAngles.y.Wrap180();
			var x = angle / (m_MovingPlatforms.cameraMaxRotationSpeed.Value * deltaTime);
			m_CharacterInput.movingPlatformLookInput = new Vector2(x, 0.0f);
		}

		// Update moving platform data after the character moved
		void PostUpdateMovingPlatform(Transform previousActivePlatform)
		{
			// Is character not on a platform, but was at the start of the movement?
			if (m_MovingPlatforms.activePlatform == null && previousActivePlatform != null)
			{
				// Check if a potential platform is still below the character
				RaycastHit hitInfo;
				var distance = k_TestPlatformDistance;
				if (characterController.CheckCollisionBelow(distance,
					out hitInfo, cachedTransform.position, Vector3.zero, true,
					characterController.IsLocalHuman, characterController.IsLocalHuman))
				{
					CheckMovingPlatformCollision(new Vector3(0.0f, -distance, 0.0f), hitInfo.normal, hitInfo.transform);
				}
			}

			m_MovingPlatforms.recordedPlatform = m_MovingPlatforms.activePlatform;
			if (m_MovingPlatforms.activePlatform == null)
			{
				return;
			}
			
			// Position
			m_MovingPlatforms.activePlatformGlobalPoint = cachedTransform.position;
			m_MovingPlatforms.activePlatformLastPosition = m_MovingPlatforms.activePlatform.position;
			m_MovingPlatforms.activePlatformLocalPoint = m_MovingPlatforms.activePlatform.InverseTransformPoint(cachedTransform.position);
			// Rotation
			m_MovingPlatforms.activePlatformGlobalRotation = cachedTransform.rotation;
			m_MovingPlatforms.activePlatformLocalRotation = Quaternion.Inverse(m_MovingPlatforms.activePlatform.rotation) * cachedTransform.rotation;
		}

		// Called when the Cinemachine brain's active camera changes
		void OnCameraActivated(ICinemachineCamera newCamera, ICinemachineCamera oldCamera)
		{			
			if (m_MovingPlatforms.canRotateCameras)
			{
				m_MovingPlatforms.cameraMaxRotationSpeed = GetLiveCameraMaxRotationSpeed(newCamera);
			}
		}
		
		// Find the live camera and get the max rotation speed of its horizontal axis, used by the moving platforms.
		void FindLiveCamera()
		{
			m_MovingPlatforms.cameraMaxRotationSpeed = null;
			if (!m_MovingPlatforms.canRotateCameras)
			{
				return;
			}
			if (m_MovingPlatforms.cinemachineBrain == null && CinemachineCore.Instance.BrainCount > 0)
			{
				m_MovingPlatforms.cinemachineBrain = CinemachineCore.Instance.GetActiveBrain(0);
				if (m_MovingPlatforms.cinemachineBrain != null)
				{
					m_MovingPlatforms.cinemachineBrain.m_CameraActivatedEvent.RemoveListener(OnCameraActivated);
					m_MovingPlatforms.cinemachineBrain.m_CameraActivatedEvent.AddListener(OnCameraActivated);
				}
			}
			if (m_MovingPlatforms.cinemachineBrain == null)
			{
				return;
			}
			var stateCamera = m_MovingPlatforms.cinemachineBrain.ActiveVirtualCamera as CinemachineStateDrivenCamera;
			if (stateCamera == null)
			{
				return;
			}
			m_MovingPlatforms.cameraMaxRotationSpeed = GetLiveCameraMaxRotationSpeed(stateCamera.LiveChild);
		}

		// Get the live camera's max rotation speed of its horizontal axis, used by the moving platforms.
		float? GetLiveCameraMaxRotationSpeed(ICinemachineCamera liveCamera)
		{
			var virtualCamera = liveCamera as CinemachineVirtualCamera;
			if (virtualCamera != null)
			{
				// POV camera used for first person mode
				var pov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
				if (pov == null)
				{
					return null;
				}
				return pov.m_HorizontalAxis.m_MaxSpeed;
			}

			// Free look camera used for strafe mode
			var freeLook = liveCamera as CinemachineFreeLook;
			if (freeLook == null)
			{
				return null;
			}
			return freeLook.m_XAxis.m_MaxSpeed;
		}

#if UNITY_EDITOR
		/// <summary>
		/// Draws gizmos. Must be manually called
		/// </summary>
		public virtual void OnDrawGizmosSelected()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (!isGrounded)
			{
				UpdatePredictedLandingPosition();
			}
			
			Gizmos.color = Color.green;
			for (var index = 0; index < m_JumpStepCount - 1; index++)
			{
				Gizmos.DrawLine(m_JumpSteps[index], m_JumpSteps[index + 1]);
			}

			if (m_PredictedLandingPosition != null)
			{
				var land = (Vector3)m_PredictedLandingPosition;
				Gizmos.DrawLine(land - new Vector3(0.5f, 0), land + new Vector3(0.5f, 0));
				Gizmos.DrawLine(land - new Vector3(0, 0, 0.5f), land + new Vector3(0, 0,0.5f));
			}
		}
#endif
	}
}