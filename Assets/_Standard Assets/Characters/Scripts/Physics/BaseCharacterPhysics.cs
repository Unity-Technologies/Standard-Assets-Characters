using System;
using StandardAssets.Characters.CharacterInput;
using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	[RequireComponent(typeof(ICharacterInput))]
	public abstract class BaseCharacterPhysics : MonoBehaviour, ICharacterPhysics
	{
		/// <summary>
		/// The maximum speed that the character can move downwards
		/// </summary>
		[SerializeField]
		protected float terminalVelocity = 10f;

		[SerializeField]
		protected float jumpGravityMultiplier = 1f;
		
		[SerializeField, Range(1, 10)]
		protected float fallGravityMultiplier = 2.5f;

		[SerializeField, Range(1, 10)]
		protected float minJumpHeightMultiplier = 2f;

		public bool isGrounded { get; private set; }
		public abstract bool startedSlide { get; }
		public Action landed { get; set; }
		public Action jumpVelocitySet { get; set; }
		public Action<float> startedFalling { get; set; }
		public float airTime { get; private set; }
		public float fallTime { get; private set; }

		protected abstract float radius { get; }
		protected abstract Vector3 footWorldPosition { get; }
		protected abstract LayerMask collisionLayerMask { get; }
		
		private const int k_TrajectorySteps = 60;
		private const float k_TrajectoryPredictionTimeStep = 0.016f;
		private readonly Collider[] trajectoryPredictionColliders = new Collider[1];
		
		protected Vector3? predictedLandingPosition;
#if UNITY_EDITOR
		private readonly Vector3[] jumpSteps = new Vector3[k_TrajectorySteps];
		private int jumpStepCount;
#endif

		public float normalizedVerticalSpeed
		{
			get
			{
				if (initialJumpVelocity < Mathf.Epsilon)
				{
					return 0f;
				}
				
				return Mathf.Clamp(currentVerticalVelocity / (initialJumpVelocity * fallGravityMultiplier), -1f, 1f);
			}
		}
		
		/// <summary>
		/// The initial jump velocity
		/// </summary>
		protected float initialJumpVelocity;

		/// <summary>
		/// The current vertical velocity
		/// </summary>
		/// <returns></returns>
		protected float currentVerticalVelocity;

		/// <summary>
		/// The last used ground (vertical velocity excluded ie 0) velocity
		/// </summary>
		private Vector3 cachedGroundVelocity;
		
		/// <summary>
		/// The current vertical vector
		/// </summary>
		private Vector3 verticalVector = Vector3.zero;

		private ICharacterInput characterInput;

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
			return predictedLandingPosition == null ? float.MaxValue : 
				footWorldPosition.y - ((Vector3)predictedLandingPosition).y;
		}

		/// <summary>
		/// Tries to jump
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
			characterInput = GetComponent<ICharacterInput>();

			if (terminalVelocity > 0)
			{
				terminalVelocity = -terminalVelocity;
			}
		}

		/// <summary>
		/// Updates the predicted landing position by stepping through the fall trajectory
		/// </summary>
		private void UpdatePredictedLandingPosition()
		{
			Vector3 currentPosition = footWorldPosition;
			Vector3 moveVector = cachedGroundVelocity;
			float currentAirTime = 0;
			for (int i = 0; i < k_TrajectorySteps; i++)
			{
				moveVector.y = Mathf.Clamp(UnityEngine.Physics.gravity.y * fallGravityMultiplier * currentAirTime,
												terminalVelocity, Mathf.Infinity) ;
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
		/// Checks if the given position would collide with the ground collision layer
		/// </summary>
		/// <param name="position">Position to check</param>
		private bool IsGroundCollision(Vector3 position)
		{
			// move sphere but to match bottom of character's capsule collider
			int colliderCount = UnityEngine.Physics.OverlapSphereNonAlloc(position + new Vector3(0, radius, 0),
																		  radius, trajectoryPredictionColliders,
																		  collisionLayerMask);
			return colliderCount > 0;
		}
		
		/// <summary>
		/// Handles Jumping and Falling
		/// </summary>
		private void AerialMovement(float deltaTime)
		{
			airTime += deltaTime;
			currentVerticalVelocity = Mathf.Clamp(initialJumpVelocity + CalculateGravity() * airTime, terminalVelocity, 
				Mathf.Infinity);
			float previousFallTime = fallTime;

			if (currentVerticalVelocity < 0)
			{
				fallTime += deltaTime;
				if (isGrounded)
				{
					initialJumpVelocity = 0f;
					verticalVector = Vector3.zero;
					
					//Play the moment that the character lands and only at that moment
					if (Math.Abs(airTime - deltaTime) > Mathf.Epsilon && landed != null)
					{
						landed();
					}
					
					fallTime = 0f;
					airTime = 0f;
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
			
			verticalVector = new Vector3(0, currentVerticalVelocity * deltaTime, 0);
		}

		/// <summary>
		/// Calculates the current gravity modified based on current vertical velocity
		/// </summary>
		private float CalculateGravity()
		{
			if (currentVerticalVelocity < 0)
			{
				return UnityEngine.Physics.gravity.y * fallGravityMultiplier;
			}

			return characterInput.hasJumpInput ? UnityEngine.Physics.gravity.y * jumpGravityMultiplier : 
				UnityEngine.Physics.gravity.y * minJumpHeightMultiplier * jumpGravityMultiplier;
		}
		
		protected abstract bool CheckGrounded();

		protected abstract void MoveCharacter(Vector3 movement);
		
#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			for (int index = 0; index < jumpStepCount - 1; index++)
			{
				Gizmos.DrawLine(jumpSteps[index], jumpSteps[index+1]);
			}
			Gizmos.color = Color.green;
			if (predictedLandingPosition != null)
			{
				Gizmos.DrawSphere((Vector3)predictedLandingPosition, 0.05f);
			}
		}
#endif
	}
}