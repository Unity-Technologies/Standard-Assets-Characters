using System;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Common;
using UnityEngine;
using UnityPhysics = UnityEngine.Physics;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Abstract implementation of <see cref="ICharacterPhysics"/> that requires an <see cref="ICharacterInput"/> and
	/// a <see cref="INormalizedForwardSpeedContainer"/>.
	/// </summary>
	[RequireComponent(typeof(ICharacterInput))]
	[RequireComponent(typeof(INormalizedForwardSpeedContainer))]
	public abstract class BaseCharacterPhysics : MonoBehaviour, ICharacterPhysics
	{
		/// <summary>
		/// Used as a clamp for downward velocity.
		/// </summary>
		[SerializeField, Tooltip("The maximum speed that the character can move downwards")]
		protected float terminalVelocity = 10f;

		[SerializeField, Tooltip("Gravity scale applied during a jump")]
		protected AnimationCurve jumpGravityMultiplierAsAFactorOfForwardSpeed =
			AnimationCurve.Constant(1.0f, 0.0f, 1.0f);

		[SerializeField, Tooltip("Gravity scale applied during a fall")]
		protected AnimationCurve fallGravityMultiplierAsAFactorOfForwardSpeed =
			AnimationCurve.Constant(1.0f, 0.0f, 1.0f);

		[SerializeField, Tooltip("Gravity scale applied during a jump without jump button held")]
		protected AnimationCurve minJumpHeightMultiplierAsAFactorOfForwardSpeed =
			AnimationCurve.Constant(1.0f, 0.0f, 1.0f);

		[SerializeField, Tooltip("The speed at which gravity is allowed to change")]
		protected float gravityChangeSpeed = 10f;

		/// <inheritdoc/>
		public bool isGrounded { get; private set; }

		/// <inheritdoc/>
		public abstract bool startedSlide { get; }

		/// <inheritdoc/>
		public event Action landed;

		/// <inheritdoc/>
		public event Action jumpVelocitySet;

		/// <inheritdoc/>
		public event Action<float> startedFalling;

		/// <inheritdoc/>
		public float airTime { get; private set; }

		/// <inheritdoc/>
		public float fallTime { get; private set; }

		/// <summary>
		/// Gets the radius of the character.
		/// </summary>
		/// <value>The radius used for predicting the landing position.</value>
		protected abstract float radius { get; }

		/// <summary>
		/// Gets the character's world foot position.
		/// </summary>
		/// <value>A world position at the bottom of the character</value>
		protected abstract Vector3 footWorldPosition { get; }

		/// <summary>
		/// Gets the collision layer mask used for physics grounding,
		/// </summary>
		protected abstract LayerMask collisionLayerMask { get; }

		// the number of time sets used for trajectory prediction.
		private const int k_TrajectorySteps = 60;

		// the time step used between physics trajectory steps.
		private const float k_TrajectoryPredictionTimeStep = 0.016f;

		// colliders used in physics checks for landing prediction
		private readonly Collider[] trajectoryPredictionColliders = new Collider[1];

		// the current value of gravity
		private float gravity;

		// the source of the normalized forward speed.
		private INormalizedForwardSpeedContainer normalizedForwardSpeedContainer;

		/// <summary>
		/// The predicted landing position of the character. Null if a position could not be predicted.
		/// </summary>
		protected Vector3? predictedLandingPosition;
#if UNITY_EDITOR
		private readonly Vector3[] jumpSteps = new Vector3[k_TrajectorySteps];
		private int jumpStepCount;
#endif

		/// <inheritdoc/>
		public float normalizedVerticalSpeed
		{
			get;
			private set;
		}

		/// <summary>
		/// The initial jump velocity.
		/// </summary>
		/// <value>Velocity used to initiate a jump.</value>
		protected float initialJumpVelocity;

		/// <summary>
		/// The current vertical velocity.
		/// </summary>
		/// <value>Calculated using <see cref="initialJumpVelocity"/>, <see cref="airTime"/> and
		/// <see cref="CalculateGravity"/></value>
		protected float currentVerticalVelocity;

		/// <summary>
		/// The last used ground (vertical velocity excluded ie 0) velocity.
		/// </summary>
		/// <value>Velocity based on the moveVector used by <see cref="Move"/>.</value>
		private Vector3 cachedGroundVelocity;

		/// <summary>
		/// The current vertical vector.
		/// </summary>
		/// <value><see cref="Vector3.zero"/> with a y based on <see cref="currentVerticalVelocity"/>.</value>
		private Vector3 verticalVector = Vector3.zero;

		private ICharacterInput characterInput;

		/// <summary>
		/// Gets the current jump gravity multiplier as a factor of normalized forward speed.
		/// </summary>
		private float jumpGravityMultiplier
		{
			get
			{
				return jumpGravityMultiplierAsAFactorOfForwardSpeed.Evaluate(
					normalizedForwardSpeedContainer.normalizedForwardSpeed);
			}
		}
		
		/// <summary>
		/// Gets the current minimum jump height gravity multiplier as a factor of normalized forward speed.
		/// </summary>
		private float minJumpHeightMultiplier
		{
			get
			{
				return minJumpHeightMultiplierAsAFactorOfForwardSpeed.Evaluate(
					normalizedForwardSpeedContainer.normalizedForwardSpeed);
			}
		}
		
		/// <summary>
		/// Gets the current fall gravity multiplier as a factor of normalized forward speed.
		/// </summary>
		private float fallGravityMultiplier
		{
			get
			{
				return fallGravityMultiplierAsAFactorOfForwardSpeed.Evaluate(
					normalizedForwardSpeedContainer.normalizedForwardSpeed);
			}
		}

		/// <inheritdoc />
		public void Move(Vector3 moveVector, float deltaTime)
		{
			isGrounded = CheckGrounded();
			AerialMovement(deltaTime);
			MoveCharacter(moveVector + verticalVector);
			cachedGroundVelocity = moveVector / deltaTime;
		}

		/// <summary>
		/// Calculates the current predicted fall distance based on the predicted landing position
		/// </summary>
		/// <returns>The predicted fall distance</returns>
		public float GetPredictedFallDistance()
		{
			UpdatePredictedLandingPosition();
			return predictedLandingPosition == null
				? float.MaxValue
				: footWorldPosition.y - ((Vector3) predictedLandingPosition).y;
		}

		/// <summary>
		/// Tries to jump.
		/// </summary>
		/// <param name="initialVelocity"></param>
		public void SetJumpVelocity(float initialVelocity)
		{
			initialJumpVelocity = initialVelocity;
			if (jumpVelocitySet != null)
			{
				jumpVelocitySet();
			}
		}

		protected virtual void Awake()
		{
			normalizedVerticalSpeed = 0.0f;
			characterInput = GetComponent<ICharacterInput>();
			normalizedForwardSpeedContainer = GetComponent<INormalizedForwardSpeedContainer>();

			if (terminalVelocity > 0.0f)
			{
				terminalVelocity = -terminalVelocity;
			}

			gravity = UnityPhysics.gravity.y;
		}

		/// <summary>
		/// Updates the predicted landing position by stepping through the fall trajectory
		/// </summary>
		private void UpdatePredictedLandingPosition()
		{
			Vector3 currentPosition = footWorldPosition;
			Vector3 moveVector = cachedGroundVelocity;
			float currentAirTime = 0.0f;
			for (int i = 0; i < k_TrajectorySteps; i++)
			{
				moveVector.y = Mathf.Clamp(gravity * fallGravityMultiplier * currentAirTime,  terminalVelocity, 
				                           Mathf.Infinity);
				currentPosition += moveVector * k_TrajectoryPredictionTimeStep;
				currentAirTime += k_TrajectoryPredictionTimeStep;
#if UNITY_EDITOR
				jumpSteps[i] = currentPosition;
#endif
				if (IsGroundCollision(currentPosition))
				{
#if UNITY_EDITOR
					// for gizmos
					jumpStepCount = i;
#endif
					predictedLandingPosition = currentPosition;
					return;
				}
			}
#if UNITY_EDITOR
			jumpStepCount = k_TrajectorySteps;
#endif
			predictedLandingPosition = null;
		}

		/// <summary>
		/// Checks if the given position would collide with the ground collision layer.
		/// </summary>
		/// <param name="position">Position to check</param>
		/// <returns>True if a ground collision would occur at the given position.</returns>
		private bool IsGroundCollision(Vector3 position)
		{
			// move sphere but to match bottom of character's capsule collider
			int colliderCount = UnityPhysics.OverlapSphereNonAlloc(position + new Vector3(0.0f, radius, 0.0f),
																   radius, trajectoryPredictionColliders,
																   collisionLayerMask);
			return colliderCount > 0.0f;
		}

		/// <summary>
		/// Handles Jumping and Falling
		/// </summary>
		private void AerialMovement(float deltaTime)
		{
			airTime += deltaTime;
			CalculateGravity(deltaTime);
			if (currentVerticalVelocity >= 0.0f)
			{
				currentVerticalVelocity = Mathf.Clamp(initialJumpVelocity + gravity * airTime, terminalVelocity,
													  Mathf.Infinity);
			}
			
			float previousFallTime = fallTime;

			if (currentVerticalVelocity < 0.0f)
			{
				currentVerticalVelocity = Mathf.Clamp(gravity * fallTime, terminalVelocity, Mathf.Infinity);
				fallTime += deltaTime;
				if (isGrounded)
				{
					initialJumpVelocity = 0.0f;
					verticalVector = Vector3.zero;

					//Play the moment that the character lands and only at that moment
					if (Math.Abs(airTime - deltaTime) > Mathf.Epsilon && landed != null)
					{
						landed();
					}

					fallTime = 0.0f;
					airTime = 0.0f;
					return;
				}
			}

			if (Mathf.Approximately(previousFallTime, 0.0f) && fallTime > Mathf.Epsilon)
			{
				if (startedFalling != null)
				{
					startedFalling(GetPredictedFallDistance());
				}
			}
			verticalVector = new Vector3(0.0f, currentVerticalVelocity * deltaTime, 0.0f);
		}

		/// <summary>
		/// Calculates the current gravity modified based on current vertical velocity
		/// </summary>
		private void CalculateGravity(float deltaTime)
		{
			float gravityFactor;
			if (currentVerticalVelocity < 0.0f)
			{
				gravityFactor = fallGravityMultiplier;
				if (initialJumpVelocity < Mathf.Epsilon)
				{
					normalizedVerticalSpeed = 0.0f;
				}
				else
				{
					normalizedVerticalSpeed = Mathf.Clamp(currentVerticalVelocity / 
					                                      (initialJumpVelocity * gravityFactor), -1f, 1f);
				}
			}
			else
			{
				gravityFactor = jumpGravityMultiplier;
				if (!characterInput.hasJumpInput) // if no input apply min jump modifier
				{
					gravityFactor *= minJumpHeightMultiplier;
				}
				normalizedVerticalSpeed = currentVerticalVelocity / initialJumpVelocity;
			}

			float newGravity = gravityFactor * UnityPhysics.gravity.y;
			gravity = Mathf.Lerp(gravity, newGravity, deltaTime * gravityChangeSpeed);
		}

		/// <returns>True if the character is grounded; false otherwise.</returns>
		protected abstract bool CheckGrounded();

		/// <summary>
		/// Moves the character by <paramref name="movement"/> world units.
		/// </summary>
		/// <param name="movement">The value to move the character by in world units.</param>
		protected abstract void MoveCharacter(Vector3 movement);

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			for (int index = 0; index < jumpStepCount - 1; index++)
			{
				Gizmos.DrawLine(jumpSteps[index], jumpSteps[index + 1]);
			}

			Gizmos.color = Color.green;
			if (predictedLandingPosition != null)
			{
				Gizmos.DrawSphere((Vector3) predictedLandingPosition, 0.05f);
			}
		}
#endif
	}
}