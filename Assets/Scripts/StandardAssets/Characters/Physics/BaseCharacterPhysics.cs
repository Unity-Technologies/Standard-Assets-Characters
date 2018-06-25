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
		private float terminalVelocity = 10f;
		
		public bool isGrounded { get; private set; }
		public Action landed { get; set; }
		public Action jumpVelocitySet { get; set; }
		public Action<float> startedFalling { get; set; }
		public float airTime { get; private set; }
		public float fallTime { get; private set; }
		
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
		public void Move(Vector3 moveVector)
		{
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
			AerialMovement();
		}
		
		/// <summary>
		/// Handles Jumping and Falling
		/// </summary>
		private void AerialMovement()
		{
			isGrounded = CheckGrounded();
			
			airTime += Time.fixedDeltaTime;
			currentVerticalVelocity = Mathf.Clamp(initialJumpVelocity + gravity * airTime, terminalVelocity, Mathf.Infinity);
			float previousFallTime = fallTime;

			if (currentVerticalVelocity < 0)
			{
				fallTime += Time.fixedDeltaTime;
			}
			
			if (currentVerticalVelocity < 0f && isGrounded)
			{
				initialJumpVelocity = 0f;
				verticalVector = Vector3.zero;
				
				//Play the moment that the character lands and only at that moment
				if (Math.Abs(airTime - Time.fixedDeltaTime) > Mathf.Epsilon)
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
			
			verticalVector = new Vector3(0, currentVerticalVelocity * Time.fixedDeltaTime, 0);
		}

		protected abstract float GetPredicitedFallDistance();
		
		protected abstract bool CheckGrounded();

		protected abstract void MoveCharacter(Vector3 movement);
	}
}