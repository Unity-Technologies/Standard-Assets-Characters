using System;
using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	public abstract class BaseCharacterPhysics : MonoBehaviour, ICharacterPhysics
	{
		private float gravity;

		/// <summary>
		/// The maximum speed that the character can move downwards
		/// </summary>
		[SerializeField]
		protected float terminalVelocity = 10f;

		protected bool hasMovedBeenCalled;
		
		public bool isGrounded { get; private set; }
		public abstract bool startedSlide { get; }
		public Action landed { get; set; }
		public Action jumpVelocitySet { get; set; }
		public Action<float> startedFalling { get; set; }
		public float airTime { get; private set; }
		public float fallTime { get; private set; }

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
		
		/// <summary>
		/// The current vertical vector
		/// </summary>
		private Vector3 verticalVector = Vector3.zero;

		/// <inheritdoc />
		public void Move(Vector3 moveVector, float deltaTime)
		{
			hasMovedBeenCalled = true;
			AerialMovement(deltaTime);
			MoveCharacter(moveVector + verticalVector);
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
			gravity = UnityEngine.Physics.gravity.y;

			if (terminalVelocity > 0)
			{
				terminalVelocity = -terminalVelocity;
			}
		}
		
		/// <summary>
		/// Handle falling physics
		/// </summary>
		private void FixedUpdate()
		{
			if (!hasMovedBeenCalled)
			{
				AerialMovement(Time.fixedDeltaTime);
				MoveCharacter(verticalVector);
			}

			hasMovedBeenCalled = false;
		}
		
		/// <summary>
		/// Handles Jumping and Falling
		/// </summary>
		private void AerialMovement(float deltaTime)
		{
			isGrounded = CheckGrounded();
			
			airTime += deltaTime;
			currentVerticalVelocity = Mathf.Clamp(initialJumpVelocity + gravity * airTime, terminalVelocity, Mathf.Infinity);
			float previousFallTime = fallTime;

			if (currentVerticalVelocity < 0)
			{
				fallTime += deltaTime;
			}
			
			if (currentVerticalVelocity < 0f && isGrounded)
			{
				initialJumpVelocity = 0f;
				verticalVector = Vector3.zero;
				
				//Play the moment that the character lands and only at that moment
				if (Math.Abs(airTime - deltaTime) > Mathf.Epsilon)
				{
					if (landed != null)
					{
						landed();
					}
				}

				fallTime = 0f;
				airTime = 0f;
				return;
			}
			
			if (previousFallTime < Mathf.Epsilon && fallTime > Mathf.Epsilon)
			{
				if (startedFalling != null)
				{
					startedFalling(GetPredicitedFallDistance());
				}
			}
			
			verticalVector = new Vector3(0, currentVerticalVelocity * deltaTime, 0);
		}

		public abstract float GetPredicitedFallDistance();
		
		protected abstract bool CheckGrounded();

		protected abstract void MoveCharacter(Vector3 movement);
	}
}