using System;
using System.Timers;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Events;

namespace StandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(ICharacterPhysics))]
	[RequireComponent(typeof(ICharacterInput))]
	public class ThirdPersonMotor : MonoBehaviour, IThirdPersonMotor
	{
		//Serialized Fields
		[SerializeField]
		protected ThirdPersonMotorProperties motorProperties;
		
		[SerializeField]
		protected InputResponse runInput;
		
		[SerializeField]
		protected InputResponse strafeInput;
		
		[SerializeField]
		protected UnityEvent startActionMode, startStrafeMode;
		
		//Properties
		
		public float normalizedTurningSpeed { get; private set; }
		public float normalizedLateralSpeed { get; private set; }
		public float normalizedForwardSpeed { get; private set; }
		public float fallTime { get; private set; }
		public Action jumpStarted { get; set; }
		public Action landed { get; set; }
		public Action<float> fallStarted { get; set; }
		public Action<float> rapidlyTurned { get; set; }
		
		//Protected fields
		
		/// <summary>
		/// The input implementation
		/// </summary>
		protected ICharacterInput characterInput;

		/// <summary>
		/// The physic implementation
		/// </summary>
		protected ICharacterPhysics characterPhysics;

		protected ThirdPersonMotorMovementMode movementMode = ThirdPersonMotorMovementMode.Action;

		protected ThirdPersonGroundMovementState movementState = ThirdPersonGroundMovementState.Walking;
		
		protected AnimationInputProperties currentForwardInputProperties, currentLateralInputProperties;
		
		protected float forwardClampSpeed, targetForwardClampSpeed, lateralClampSpeed, targetLateralClampSpeed;
		
		//Public Methods
		
		public void FinishedTurn()
		{
			throw new NotImplementedException();
		}
		
		//Unity Messages
		
		protected virtual void Awake()
		{
			characterInput = GetComponent<ICharacterInput>();
			characterPhysics = GetComponent<ICharacterPhysics>();

			if (runInput != null)
			{
				runInput.Init();
			}
			
			if (strafeInput != null)
			{
				strafeInput.Init();
			}
			
			OnStrafeEnded();
		}
		
		/// <summary>
		/// Subscribe
		/// </summary>
		protected virtual void OnEnable()
		{
			//Physics subscriptions
			characterPhysics.landed += OnLanding;
			characterPhysics.startedFalling += OnStartedFalling;
			
			//Input subscriptions
			characterInput.jumpPressed += OnJumpPressed;
			if (runInput != null)
			{
				runInput.started += OnRunStarted;
				runInput.ended += OnRunEnded;
			}
			if (strafeInput != null)
			{
				strafeInput.started += OnStrafeStarted;
				strafeInput.ended += OnStrafeEnded;
			}
		}
		
		/// <summary>
		/// Unsubscribe
		/// </summary>
		protected virtual void OnDisable()
		{
			//Physics subscriptions
			if (characterPhysics != null)
			{
				characterPhysics.landed -= OnLanding;
				characterPhysics.startedFalling -= OnStartedFalling;
			}
			
			//Input subscriptions
			if (characterInput != null)
			{
				characterInput.jumpPressed -= OnJumpPressed;
			}
			if (runInput != null)
			{
				runInput.started -= OnRunStarted;
				runInput.ended -= OnRunEnded;
			}
			if (strafeInput != null)
			{
				strafeInput.started -= OnStrafeStarted;
				strafeInput.ended -= OnStrafeEnded;
			}
		}

		protected virtual void Update()
		{
			HandleMovement();
			HandleClampSpeedDeceleration();
		}

		//Protected Methods
		/// <summary>
		/// Handles player landing
		/// </summary>
		protected virtual void OnLanding()
		{
			if (landed != null)
			{
				landed();
			}
		}
		
		/// <summary>
		/// Handles player falling
		/// </summary>
		/// <param name="predictedFallDistance"></param>
		protected virtual void OnStartedFalling(float predictedFallDistance)
		{
			if (fallStarted != null)
			{
				fallStarted(predictedFallDistance);
			}
		}
		
		/// <summary>
		/// Subscribes to the Jump action on input
		/// </summary>
		protected virtual void OnJumpPressed()
		{
			if (!characterPhysics.isGrounded)
			{
				return;
			}

			if (jumpStarted != null)
			{
				jumpStarted();
			}

			characterPhysics.SetJumpVelocity(motorProperties.initialJumpVelocity);
		}
		
		/// <summary>
		/// Method called by run input started
		/// </summary>
		protected virtual void OnRunEnded()
		{
			movementState = ThirdPersonGroundMovementState.Walking;
		}

		/// <summary>
		/// Method called by run input ended
		/// </summary>
		protected virtual void OnRunStarted()
		{
			movementState = ThirdPersonGroundMovementState.Running;
		}
	
		/// <summary>
		/// Method called by strafe input started
		/// </summary>
		protected virtual void OnStrafeStarted()
		{
			if (startStrafeMode != null)
			{
				startStrafeMode.Invoke();
			}

			movementMode = ThirdPersonMotorMovementMode.Strafe;
		}
		
		/// <summary>
		/// Method called by strafe input ended
		/// </summary>
		protected virtual  void OnStrafeEnded()
		{
			if (startActionMode != null)
			{
				startActionMode.Invoke();
			}

			movementMode = ThirdPersonMotorMovementMode.Action;
		}

		/// <summary>
		/// Called by update to handle movement
		/// </summary>
		protected virtual void HandleMovement()
		{
			if (movementState == ThirdPersonGroundMovementState.TurningAround)
			{
				TurningAround();
			}
			
			switch (movementMode)
			{
				case ThirdPersonMotorMovementMode.Action:
					ActionMovement();
					break;
				case ThirdPersonMotorMovementMode.Strafe:
					StrafeMovement();
					break;
			}
		}

		protected virtual void TurningAround()
		{
			throw new NotImplementedException();
		}
		
		protected virtual void ActionMovement()
		{
			SetLookDirection();
			CalculateForwardMovement();
		}

		protected virtual void StrafeMovement()
		{
			SetStrafeLookDirection();
			CalculateStrafeMovement();
		}

		protected virtual void SetStrafeLookDirection()
		{
			throw new NotImplementedException();
		}

		protected virtual void SetLookDirection()
		{
			throw new NotImplementedException();
		}

		protected virtual void CalculateForwardMovement()
		{
			normalizedLateralSpeed = 0;

			if (!characterInput.hasMovementInput)
			{
				EaseOffForwardInput();
				return;
			}

			ApplyForwardInput(1f);
		}

		protected virtual void CalculateStrafeMovement()
		{
			Vector2 moveInput = characterInput.moveInput;

			// we need to ease each axis
			if (Mathf.Abs(moveInput.y) > Mathf.Epsilon)
			{
				ApplyForwardInput(Mathf.Sign(moveInput.y));
			}
			else
			{
				EaseOffForwardInput();
			}

			if (Mathf.Abs(moveInput.x) > Mathf.Epsilon)
			{
				ApplyLateralInput(Mathf.Sign(moveInput.x));
			}
			else
			{
				EaseOffLateralInput();
			}
		}
		
		protected virtual void ApplyForwardInput(float input)
		{
			float forwardVelocity = currentForwardInputProperties.inputGain;
			if (Mathf.Abs(Mathf.Sign(input) - Mathf.Sign(normalizedForwardSpeed)) > 0)
			{
				forwardVelocity = currentForwardInputProperties.inputChangeGain;
			}

			normalizedForwardSpeed =
				Mathf.Clamp(normalizedForwardSpeed + input * forwardVelocity * Time.deltaTime, -forwardClampSpeed,
				            forwardClampSpeed);
		}

		protected virtual void EaseOffForwardInput()
		{
			normalizedForwardSpeed =
				Mathf.Lerp(normalizedForwardSpeed, 0, currentForwardInputProperties.inputDecay * Time.deltaTime);
		}
		
		protected virtual void ApplyLateralInput(float input)
		{
			float lateralVelocity = currentLateralInputProperties.inputGain;
			if (Mathf.Abs(Mathf.Sign(input) - Mathf.Sign(normalizedLateralSpeed)) > 0)
			{
				lateralVelocity = currentLateralInputProperties.inputChangeGain;
			}

			normalizedLateralSpeed =
				Mathf.Clamp(normalizedLateralSpeed + input * lateralVelocity * Time.deltaTime, -forwardClampSpeed,
				            forwardClampSpeed);
		}

		protected virtual void EaseOffLateralInput()
		{
			normalizedLateralSpeed =
				Mathf.Lerp(normalizedLateralSpeed, 0, currentLateralInputProperties.inputDecay * Time.deltaTime);
		}
		
		protected virtual float DecelerateClampSpeed(float currentValue, float targetValue, float gain)
		{
			if (currentValue <= targetValue)
			{
				return targetValue;
			}

			return Mathf.Lerp(currentValue, targetValue, Time.deltaTime * gain);
		}

		protected virtual void HandleClampSpeedDeceleration()
		{
			forwardClampSpeed = DecelerateClampSpeed(forwardClampSpeed, targetForwardClampSpeed,
			                                         currentForwardInputProperties.inputDecay);

			if (movementMode == ThirdPersonMotorMovementMode.Strafe)
			{
				lateralClampSpeed = DecelerateClampSpeed(lateralClampSpeed, targetLateralClampSpeed,
				                                         currentLateralInputProperties.inputDecay);
			}
		}
	}
}