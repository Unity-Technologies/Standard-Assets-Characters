using System;
using StandardAssets.Characters.CharacterInput;
using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	[RequireComponent(typeof(ICharacterInput))]
	public abstract class BaseCharacterPhysics : MonoBehaviour, ICharacterPhysics
	{
		private float gravity;

		/// <summary>
		/// The maximum speed that the character can move downwards
		/// </summary>
		[SerializeField]
		protected float terminalVelocity = 10f;

		[SerializeField, Range(1, 10)]
		protected float fallGravityMultiplier = 2.5f;

		[SerializeField, Range(1, 10)]
		protected float minJumpHeightMultiplier = 2f;

		protected bool hasMovedBeenCalled;
		
		public bool isGrounded { get; private set; }
		public abstract bool startedSlide { get; }
		public abstract float radius { get; }
		public Action landed { get; set; }
		public Action jumpVelocitySet { get; set; }
		public Action<float> startedFalling { get; set; }
		public float airTime { get; private set; }
		public float fallTime { get; private set; }

		protected abstract Vector3 footWorldPosition { get; }
		protected abstract LayerMask collisionLayerMask { get; }
		
		private const int k_TranjectorySteps = 60;
		private readonly Collider[] trajectoryPredicitonColliders = new Collider[1];
		
		protected Vector3? predictedLandingPosition;
#if UNITY_EDITOR
		private readonly Vector3[] jumpSteps = new Vector3[k_TranjectorySteps];
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
				
				return currentVerticalVelocity / initialJumpVelocity;
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

		protected Vector3 cachedMoveVector;
		
		/// <summary>
		/// The current vertical vector
		/// </summary>
		private Vector3 verticalVector = Vector3.zero;

		private ICharacterInput characterInput;

		/// <inheritdoc />
		public void Move(Vector3 moveVector, float deltaTime)
		{
			hasMovedBeenCalled = true;
			isGrounded = CheckGrounded();
			AerialMovement(deltaTime);
			MoveCharacter(moveVector + verticalVector);
			cachedMoveVector = moveVector;
		}
		
		/// <summary>
		/// Calculates the current predicted fall distance based on the predicted landing position
		/// </summary>
		/// <returns>The predicted fall distance</returns>
		public float GetPredicitedFallDistance()
		{
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
			gravity = UnityEngine.Physics.gravity.y;

			if (terminalVelocity > 0)
			{
				terminalVelocity = -terminalVelocity;
			}
		}
		
		protected void UpdatePredictedLandingPosition(float deltaTime)
		{
			Vector3 currentPosition = footWorldPosition;
			Vector3 currentVelocity = cachedMoveVector;
			float currentAirTime = airTime;
			for (int i = 0; i < k_TranjectorySteps; i++)
			{
				currentVelocity.y = Mathf.Clamp(initialJumpVelocity + CalculateGravity() * currentAirTime,
												terminalVelocity, Mathf.Infinity) * deltaTime;
				currentPosition += currentVelocity;
				currentAirTime += deltaTime;
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
			jumpStepCount = k_TranjectorySteps;
#endif
			predictedLandingPosition = null;
		}

		private bool IsGroundCollision(Vector3 position)
		{
			// move sphere but to match bottom of character's capsule collider
			int colliderCount = UnityEngine.Physics.OverlapSphereNonAlloc(position + new Vector3(0, radius, 0),
																		  radius, trajectoryPredicitonColliders,
																		  collisionLayerMask);
			return colliderCount > 0;
		}
		
		/// <summary>
		/// Handle falling physics
		/// </summary>
		private void FixedUpdate()
		{
			isGrounded = CheckGrounded();
			if (!hasMovedBeenCalled)
			{
				AerialMovement(Time.fixedDeltaTime);
				MoveCharacter(verticalVector);
			}
			hasMovedBeenCalled = false;

			// TODO currently this is not required to be called every update. It is only required as fall is started.
			// If (When?) a better judgement of landing is required it would need to be here.
			if (!isGrounded)
			{
				UpdatePredictedLandingPosition(Time.fixedDeltaTime);
			}
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
					startedFalling(GetPredicitedFallDistance());
				}
			}
			
			verticalVector = new Vector3(0, currentVerticalVelocity * deltaTime, 0);
		}

		private float CalculateGravity()
		{
			if (currentVerticalVelocity < 0)
			{
				return gravity * fallGravityMultiplier;
			}

			return characterInput.isJumping ? gravity : gravity * minJumpHeightMultiplier;
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