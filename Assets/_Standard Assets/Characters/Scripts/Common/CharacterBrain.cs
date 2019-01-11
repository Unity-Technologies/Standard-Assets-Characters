using System;
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
	}

	/// <summary>
	/// Wrapper for the OpenCharacterController
	/// </summary>
	[Serializable]
	public class ControllerAdapter
	{
		// the number of time sets used for trajectory prediction
		const int k_TrajectorySteps = 60;

		// the time step used between physics trajectory steps
		const float k_TrajectoryPredictionTimeStep = 0.016f;
		
		// the scale that is used to convert character's current velocity to force applied to rigidbody
		const float k_VelocityToForceScale = 0.1f;
		
		[SerializeField, Tooltip("Maximum speed that the character can move downwards")]
		float m_TerminalVelocity = 10f;

		[SerializeField, Tooltip("Curve that defines the gravity scale to be applied during the rising motion of a jump, relative to the character's forward speed")]
		AnimationCurve m_JumpGravityScale = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);

		[SerializeField, Tooltip("Curve that defines the gravity scale applied during the rising motion of a jump if the jump button is no longer being held, relative to the character's forward speed")]
		AnimationCurve m_ShortJumpGravityScale = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);

		[SerializeField, Tooltip("Curve that defines the gravity scale to be applied while falling, relative to the character's forward speed")]
		AnimationCurve m_FallGravityScale = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);
		
		[SerializeField, Tooltip("Gravity scale applied when falling less that the Min Fall Distance (set below)")]
		float m_GroundingGravityScale = 5.0f; 

		[SerializeField, Tooltip("A fall less than this will trigger 'grounding'. GroundingGravityScale is applied to quickly ground the character.")]
		float m_GroundingDistance = 0.5f;

		[SerializeField, Tooltip("How quickly the Gravity that affects the character is allowed to change, when it is" +
		                         " being dynamically modified by Jumping or Falling Gravity modifiers")]
		float m_MaxGravityDelta = 10f;		

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
		bool m_Grounding;

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
		float jumpGravityMultiplier { get { return m_JumpGravityScale.Evaluate(m_CharacterBrain.normalizedForwardSpeed); } }
		
		// Gets the current short jump gravity multiplier
		float shortJumpGravityMultiplier { get { return m_ShortJumpGravityScale.Evaluate( m_CharacterBrain.normalizedForwardSpeed); } }
		
		// Gets the current fall gravity multiplier
		float fallGravityMultiplier { get { return m_FallGravityScale.Evaluate(m_CharacterBrain.normalizedForwardSpeed); } }

		/// <summary>
		/// Moves the character
		/// </summary>
		/// <param name="moveVector">Movement vector</param>
		/// <param name="deltaTime">Time since last call</param>
		public void Move(Vector3 moveVector, float deltaTime)
		{
			isGrounded = characterController.isGrounded;
			AerialMovement(deltaTime);
			MoveCharacter(moveVector + m_VerticalVector);
			m_CachedGroundVelocity = moveVector / deltaTime;
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
			m_CurrentVerticalVelocity = m_InitialJumpVelocity = initialVelocity;
			if (jumpVelocitySet != null)
			{
				jumpVelocitySet();
			}
		}

		/// <summary>
		/// Initialization on load. Must be manually called
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
		}

		// Handles interactions with collideable objects in the world
		// Applies force to Rigidbodies
		void OnCollision(OpenCharacterController.CollisionInfo collisionInfo)
		{
			OpenCharacterController controller = collisionInfo.controller;

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
			var minVelocity = m_Grounding ? -Mathf.Infinity : m_TerminalVelocity;
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

					//Play the moment that the character lands and only at that moment
					if (Math.Abs(airTime - deltaTime) > Mathf.Epsilon)
					{
						if (landed != null)
						{
							landed();
						}
						m_DidJump = false;
						m_Grounding = false;
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
					m_Grounding = true;
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
				if (!m_DidJump && m_Grounding) // if a short fall was triggered increase gravity to quickly ground
				{
					gravityFactor *= m_GroundingGravityScale;
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