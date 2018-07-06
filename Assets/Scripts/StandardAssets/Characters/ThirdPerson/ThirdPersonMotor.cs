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
	[RequireComponent(typeof(Animator))]
	public class ThirdPersonMotor : MonoBehaviour, IThirdPersonMotor
	{
		//Serialized Fields
		[SerializeField]
		protected ThirdPersonMotorProperties motorProperties;
		
		[SerializeField]
		protected Transform cameraTransform;
		
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

		protected Animator animator;

		protected ThirdPersonMotorMovementMode movementMode = ThirdPersonMotorMovementMode.Action;

		protected ThirdPersonGroundMovementState movementState = ThirdPersonGroundMovementState.Walking;
		
		protected AnimationInputProperties currentForwardInputProperties, currentLateralInputProperties;
		
		protected float forwardClampSpeed, targetForwardClampSpeed, lateralClampSpeed, targetLateralClampSpeed;

		protected Vector3 cachedGroundMovementVector;

		protected bool isStrafing
		{
			get { return movementMode == ThirdPersonMotorMovementMode.Strafe; }
		}
		
		//Public Methods
		
		public void FinishedTurn()
		{
			throw new NotImplementedException();
		}
		
		//Unity Messages
		private void OnAnimatorMove()
		{
			if (characterPhysics.isGrounded)
			{
				Vector3 groundMovementVector = animator.deltaPosition * motorProperties.scaleRootMovement;
				groundMovementVector.y = 0;
				characterPhysics.Move(groundMovementVector);
				cachedGroundMovementVector = groundMovementVector;
			}
			else
			{
				characterPhysics.Move(cachedGroundMovementVector);
			}
		}
		
		protected virtual void Awake()
		{
			characterInput = GetComponent<ICharacterInput>();
			characterPhysics = GetComponent<ICharacterPhysics>();
			animator = GetComponent<Animator>();
			
			if (cameraTransform == null)
			{
				cameraTransform = Camera.main.transform;
			}

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
			targetForwardClampSpeed = currentForwardInputProperties.inputUnclamped;
			if (isStrafing)
			{
				targetLateralClampSpeed = currentLateralInputProperties.inputUnclamped;
			}
		}

		/// <summary>
		/// Method called by run input ended
		/// </summary>
		protected virtual void OnRunStarted()
		{
			movementState = ThirdPersonGroundMovementState.Running;
			targetForwardClampSpeed = currentForwardInputProperties.inputClamped;
			if (isStrafing)
			{
				targetLateralClampSpeed = currentLateralInputProperties.inputClamped;
			}
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
			Vector3 lookForwardY = transform.rotation.eulerAngles;
			
			lookForwardY.x = 0;
			lookForwardY.z = 0;
			lookForwardY.y -= characterInput.lookInput.x * Time.deltaTime * motorProperties.scaleStrafeLook;
			
			Quaternion targetRotation = Quaternion.Euler(lookForwardY);

			targetRotation =
				Quaternion.RotateTowards(transform.rotation, targetRotation, motorProperties.turningLerp * Time.deltaTime);

			transform.rotation = targetRotation;
		}

		protected virtual void SetLookDirection()
		{
			Quaternion targetRotation = CalculateTargetRotation();
			
			targetRotation =
				Quaternion.RotateTowards(transform.rotation, targetRotation, motorProperties.turningLerp * Time.deltaTime);

			transform.rotation = targetRotation;
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

			if (isStrafing)
			{
				lateralClampSpeed = DecelerateClampSpeed(lateralClampSpeed, targetLateralClampSpeed,
				                                         currentLateralInputProperties.inputDecay);
			}
		}
		
		protected virtual Quaternion CalculateTargetRotation()
		{
			Vector3 flatForward = cameraTransform.forward;
			flatForward.y = 0f;
			flatForward.Normalize();

			Vector3 localMovementDirection =
				new Vector3(characterInput.moveInput.x, 0f, characterInput.moveInput.y);

			Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			return Quaternion.LookRotation(cameraToInputOffset * flatForward);
		}
		
	}
}