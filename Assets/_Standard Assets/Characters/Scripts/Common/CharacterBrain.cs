using System;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Serialization;
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
		ControllerAdapter m_CharacterControllerAdapter;

		Vector3 m_LastPosition;
		
		public ControllerAdapter controllerAdapter
		{
			get { return m_CharacterControllerAdapter; }
		}
		
		/// <summary>
		/// Gets/sets the planar speed (i.e. ignoring the displacement) of the CharacterBrain
		/// </summary>
		public float planarSpeed { get; private set; }
		public abstract float normalizedForwardSpeed { get;}
		public abstract float targetYRotation { get; set; }
		
		protected Vector3 planarDisplacement { get; private set; }

		/// <summary>
		/// Get controller adapters and input on Awake
		/// </summary>
		protected virtual void Awake()
		{
			m_LastPosition = transform.position;

			if (m_CharacterControllerAdapter != null)
			{
				m_CharacterControllerAdapter.Awake(transform);
			}
			else
			{
				gameObject.SetActive(false);
			}
		}

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
		/// <summary>
		/// Used as a clamp for downward velocity.
		/// </summary>
		[FormerlySerializedAs("terminalVelocity")]
		[SerializeField, Tooltip("Maximum speed that the character can move downwards")]
		float m_TerminalVelocity = 10f;

		[FormerlySerializedAs("jumpGravityMultiplierAsAFactorOfForwardSpeed")]
		[SerializeField, Tooltip("Gravity scale applied during a jump")]
		AnimationCurve m_JumpGravityMultiplierAsAFactorOfForwardSpeed =
			AnimationCurve.Constant(0.0f, 1.0f, 1.0f);

		[FormerlySerializedAs("fallGravityMultiplierAsAFactorOfForwardSpeed")]
		[SerializeField, Tooltip("Gravity scale applied during a fall")]
		AnimationCurve m_FallGravityMultiplierAsAFactorOfForwardSpeed =
			AnimationCurve.Constant(0.0f, 1.0f, 1.0f);

		[FormerlySerializedAs("minJumpHeightMultiplierAsAFactorOfForwardSpeed")]
		[SerializeField, Tooltip("Gravity scale applied during a jump without jump button held")]
		AnimationCurve m_MinJumpHeightMultiplierAsAFactorOfForwardSpeed =
			AnimationCurve.Constant(0.0f, 1.0f, 1.0f);

		[FormerlySerializedAs("gravityChangeSpeed")]
		[SerializeField, Tooltip("Speed at which gravity is allowed to change")]
		float m_GravityChangeSpeed = 10f;
		
		[FormerlySerializedAs("minFallDistance")]
		[SerializeField, Tooltip("Minimum fall distance required to trigger the fall state")]
		float m_MinFallDistance = 1.1f;
		
		[FormerlySerializedAs("groundingGravityMultiplier")]
		[SerializeField, Tooltip("Gravity multiplier applied when falling less that the Min Fall Distance (set above)")]
		float m_GroundingGravityMultiplier = 2.0f; 

		public bool isGrounded { get; private set; }
		
		public OpenCharacterController characterController { get; private set; }

		public bool startedSlide
		{
			get { return characterController.startedSlide; }
		}

		public event Action landed;

		public event Action jumpVelocitySet;

		public event Action<float> startedFalling;

		public float fallTime { get; private set; }
		
		float airTime { get; set; }

		/// <summary>
		/// Gets the radius of the character.
		/// </summary>
		/// <value>The radius used for predicting the landing position.</value>
		float radius
		{
			get { return characterController.scaledRadius + characterController.GetSkinWidth(); }
		}

		/// <summary>
		/// Gets the character's world foot position.
		/// </summary>
		/// <value>A world position at the bottom of the character</value>
		Vector3 footWorldPosition
		{
			get { return characterController.GetFootWorldPosition(); }
		}

		/// <summary>
		/// Gets the collision layer mask used for physics grounding,
		/// </summary>
		LayerMask collisionLayerMask
		{
			get { return characterController.GetCollisionLayerMask(); }
		}

		// the number of time sets used for trajectory prediction.
		const int k_TrajectorySteps = 60;

		// the time step used between physics trajectory steps.
		const float k_TrajectoryPredictionTimeStep = 0.016f;

		// colliders used in physics checks for landing prediction
		readonly Collider[] m_TrajectoryPredictionColliders = new Collider[1];

		// the current value of gravity
		float m_Gravity;

		// the source of the normalized forward speed.
		CharacterBrain m_CharacterBrain;

		/// <summary>
		/// The predicted landing position of the character. Null if a position could not be predicted.
		/// </summary>
		protected Vector3? m_PredictedLandingPosition;
#if UNITY_EDITOR
		readonly Vector3[] m_JumpSteps = new Vector3[k_TrajectorySteps];
		int m_JumpStepCount;
#endif
		
		/// <summary>
		/// Reference to the transform of the game object, on which this class will do work.
		/// </summary>
		public Transform cachedTransform { get; private set; }

		public float normalizedVerticalSpeed
		{
			get;
			private set;
		}

		/// <summary>
		/// The initial jump velocity.
		/// </summary>
		/// <value>Velocity used to initiate a jump.</value>
		protected float m_InitialJumpVelocity;

		/// <summary>
		/// The current vertical velocity.
		/// </summary>
		/// <value>Calculated using <see cref="m_InitialJumpVelocity"/>, <see cref="airTime"/> and
		/// <see cref="CalculateGravity"/></value>
		protected float m_CurrentVerticalVelocity;

		/// <summary>
		/// The last used ground (vertical velocity excluded ie 0) velocity.
		/// </summary>
		/// <value>Velocity based on the moveVector used by <see cref="Move"/>.</value>
		Vector3 m_CachedGroundVelocity;

		/// <summary>
		/// The current vertical vector.
		/// </summary>
		/// <value><see cref="Vector3.zero"/> with a y based on <see cref="m_CurrentVerticalVelocity"/>.</value>
		Vector3 m_VerticalVector = Vector3.zero;

		CharacterInput m_CharacterInput;

		bool m_ShortFall,
			m_DidJump;

		/// <summary>
		/// Gets the current jump gravity multiplier as a factor of normalized forward speed.
		/// </summary>
		float jumpGravityMultiplier
		{
			get
			{
				return m_JumpGravityMultiplierAsAFactorOfForwardSpeed.Evaluate(
					m_CharacterBrain.normalizedForwardSpeed);
			}
		}
		
		/// <summary>
		/// Gets the current minimum jump height gravity multiplier as a factor of normalized forward speed.
		/// </summary>
		float minJumpHeightMultiplier
		{
			get
			{
				return m_MinJumpHeightMultiplierAsAFactorOfForwardSpeed.Evaluate(
					m_CharacterBrain.normalizedForwardSpeed);
			}
		}
		
		/// <summary>
		/// Gets the current fall gravity multiplier as a factor of normalized forward speed.
		/// </summary>
		float fallGravityMultiplier
		{
			get
			{
				return m_FallGravityMultiplierAsAFactorOfForwardSpeed.Evaluate(
					m_CharacterBrain.normalizedForwardSpeed);
			}
		}

		public void Move(Vector3 moveVector, float deltaTime)
		{
			isGrounded = characterController.isGrounded;
			AerialMovement(deltaTime);
			MoveCharacter(moveVector + m_VerticalVector);
			m_CachedGroundVelocity = moveVector / deltaTime;
		}

		/// <summary>
		/// Calculates the current predicted fall distance based on the predicted landing position
		/// </summary>
		/// <returns>The predicted fall distance</returns>
		public float GetPredictedFallDistance()
		{
			UpdatePredictedLandingPosition();
			return m_PredictedLandingPosition == null
				? float.MaxValue
				: footWorldPosition.y - ((Vector3) m_PredictedLandingPosition).y;
		}
		
		/// <summary>
		/// Calculates whether the current fall is defined as a short fall.
		/// </summary>
		/// <returns>True is the current fall distance is less than <see cref="m_MinFallDistance"/> false otherwise.</returns>
		public bool IsPredictedFallShort(out float distance)
		{
			distance = GetPredictedFallDistance();
			return distance <= m_MinFallDistance;
		}
		
		/// <summary>
		/// Calculates whether the current fall is defined as a short fall.
		/// </summary>
		/// <returns>True is the current fall distance is less than <see cref="m_MinFallDistance"/> false otherwise.</returns>
		public bool IsPredictedFallShort()
		{
			return GetPredictedFallDistance() <= m_MinFallDistance;
		}

		/// <summary>
		/// Tries to jump.
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
		/// Initialization on load. Must be manually called.
		/// </summary>
		/// <param name="transform">Transform of the game object, on which this class will do work.</param>
		public virtual void Awake(Transform transform)
		{
			cachedTransform = transform;
			normalizedVerticalSpeed = 0.0f;
			m_CharacterInput = cachedTransform.GetComponent<CharacterInput>();
			m_CharacterBrain = cachedTransform.GetComponent<CharacterBrain>();
			characterController = cachedTransform.GetComponent<OpenCharacterController>();

			if (m_TerminalVelocity > 0.0f)
			{
				m_TerminalVelocity = -m_TerminalVelocity;
			}

			m_Gravity = UnityPhysics.gravity.y;
		}

		/// <summary>
		/// Updates the predicted landing position by stepping through the fall trajectory
		/// </summary>
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

		/// <summary>
		/// Checks if the given position would collide with the ground collision layer.
		/// </summary>
		/// <param name="position">Position to check</param>
		/// <returns>True if a ground collision would occur at the given position.</returns>
		bool IsGroundCollision(Vector3 position)
		{
			// move sphere but to match bottom of character's capsule collider
			var colliderCount = UnityPhysics.OverlapSphereNonAlloc(position + new Vector3(0.0f, radius, 0.0f),
																   radius, m_TrajectoryPredictionColliders,
																   collisionLayerMask);
			return colliderCount > 0.0f;
		}

		/// <summary>
		/// Handles Jumping and Falling
		/// </summary>
		void AerialMovement(float deltaTime)
		{
			airTime += deltaTime;
			CalculateGravity(deltaTime);
			if (m_CurrentVerticalVelocity >= 0.0f)
			{
				m_CurrentVerticalVelocity = Mathf.Clamp(m_InitialJumpVelocity + m_Gravity * airTime, m_TerminalVelocity,
													  Mathf.Infinity);
			}
			
			var previousFallTime = fallTime;

			if (m_CurrentVerticalVelocity < 0.0f)
			{
				m_CurrentVerticalVelocity = Mathf.Clamp(m_Gravity * fallTime, m_TerminalVelocity, Mathf.Infinity);
				fallTime += deltaTime;
				if (isGrounded)
				{
					m_InitialJumpVelocity = 0.0f;
					m_VerticalVector = Vector3.zero;

					//Play the moment that the character lands and only at that moment
					if (Math.Abs(airTime - deltaTime) > Mathf.Epsilon && landed != null)
					{
						landed();
						m_DidJump = false;
					}

					fallTime = 0.0f;
					airTime = 0.0f;
					return;
				}
			}

			if (Mathf.Approximately(previousFallTime, 0.0f) && fallTime > Mathf.Epsilon)
			{
				var predictedFallDistance = GetPredictedFallDistance();
				m_ShortFall = predictedFallDistance <= m_MinFallDistance;
				if (startedFalling != null)
				{
					startedFalling(predictedFallDistance);
				}
			}
			m_VerticalVector = new Vector3(0.0f, m_CurrentVerticalVelocity * deltaTime, 0.0f);
		}

		/// <summary>
		/// Calculates the current gravity modified based on current vertical velocity
		/// </summary>
		void CalculateGravity(float deltaTime)
		{
			float gravityFactor;
			if (m_CurrentVerticalVelocity < 0.0f)
			{
				gravityFactor = fallGravityMultiplier;
				if (!m_DidJump && m_ShortFall) // if a short fall was triggered increase gravity to quickly ground.
				{
					gravityFactor *= m_GroundingGravityMultiplier;
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
				if (m_CharacterInput  != null && !m_CharacterInput.hasJumpInput) // if no input apply min jump modifier
				{
					gravityFactor *= minJumpHeightMultiplier;
				}
				normalizedVerticalSpeed = m_InitialJumpVelocity > 0.0f ? m_CurrentVerticalVelocity / m_InitialJumpVelocity 
																	   : 0.0f;
			}

			var newGravity = gravityFactor * UnityPhysics.gravity.y;
			m_Gravity = Mathf.Lerp(m_Gravity, newGravity, deltaTime * m_GravityChangeSpeed);
		}

		/// <summary>
		/// Moves the character by <paramref name="movement"/> world units.
		/// </summary>
		/// <param name="movement">The value to move the character by in world units.</param>
		protected void MoveCharacter(Vector3 movement)
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
		/// Draws gizmos. Must be manually called.
		/// </summary>
		public virtual void OnDrawGizmosSelected()
		{
			for (var index = 0; index < m_JumpStepCount - 1; index++)
			{
				Gizmos.DrawLine(m_JumpSteps[index], m_JumpSteps[index + 1]);
			}

			Gizmos.color = Color.green;
			if (m_PredictedLandingPosition != null)
			{
				Gizmos.DrawSphere((Vector3) m_PredictedLandingPosition, 0.05f);
			}
		}
#endif
	}
}