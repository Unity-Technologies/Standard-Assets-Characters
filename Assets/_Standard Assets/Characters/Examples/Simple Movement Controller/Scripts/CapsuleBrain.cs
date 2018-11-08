using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.Examples.SimpleMovementController
{
	public class CapsuleBrain : CharacterBrain
	{
		[Header("Capsule Brain")]
		[SerializeField]
		protected float maxSpeed = 5f;

		[SerializeField]
		protected float timeToMaxSpeed = 0.5f;
		
		[SerializeField] 
		protected float turnSpeed = 300f;

		[SerializeField]
		protected float jumpSpeed = 5f;
	   
		/// <summary>
		/// The current movement properties
		/// </summary>
		float currentSpeed;

		/// <summary>
		/// The current movement properties
		/// </summary>
		float movementTime;

		/// <summary>
		/// A check to see if input was previous being applied
		/// </summary>
		bool previouslyHasInput;

		/// <summary>
		/// The main camera's transform, used for calculating look direction.
		/// </summary>
		Transform mainCameraTransform;

		CapsuleInput input;

		CapsuleInput characterInput
		{
			get
			{
				if (input == null)
				{
					input = GetComponent<CapsuleInput>();
				}

				return input;
			}
		}

		public override float normalizedForwardSpeed
		{
			get
			{
				return currentSpeed / maxSpeed;
			}
		}

		public override float targetYRotation { get; set; }

		protected override void Awake()
		{
			base.Awake();
			mainCameraTransform = Camera.main.transform;
		}

		void OnEnable()
		{
			characterInput.jumpPressed += OnJumpPressed;
		}
		
		/// <summary>
		/// Unsubscribe
		/// </summary>
		void OnDisable()
		{
			if (characterInput == null)
			{
				return;
			}
			
			characterInput.jumpPressed -= OnJumpPressed;
		}
		
		/// <summary>
		/// Handles camera rotation
		/// </summary>
		protected override void Update()
		{
			base.Update();
			if (!characterInput.hasMovementInput)
			{
				return;
			}
			var targetRotation = CalculateTargetRotation();
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

			targetYRotation = targetRotation.eulerAngles.y;
		}
		
		
		protected virtual Quaternion CalculateTargetRotation()
		{
			var flatForward = mainCameraTransform.forward;
			flatForward.y = 0f;
			flatForward.Normalize();

			var localMovementDirection = new Vector3(characterInput.moveInput.x, 0f, characterInput.moveInput.y);
			var cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			return Quaternion.LookRotation(cameraToInputOffset * flatForward);
		}

		/// <summary>
		/// Handles jumping
		/// </summary>
		void OnJumpPressed()
		{
			if (controllerAdapter.isGrounded)
			{
				controllerAdapter.SetJumpVelocity(jumpSpeed);
			}	
		}

		/// <summary>
		/// Handles movement on Physics update
		/// </summary>
		void FixedUpdate()
		{
			Move();
		}

		/// <summary>
		/// State based movement
		/// </summary>
		void Move()
		{
			if (characterInput.hasMovementInput)
			{
				if (!previouslyHasInput)
				{
					movementTime = 0f;
				}
				Accelerate();
			}
			else
			{
				if (previouslyHasInput)
				{
					movementTime = 0f;
				}

				Stop();
			}

			var input = characterInput.moveInput;
			if (input.sqrMagnitude > 1)
			{
				input.Normalize();
			}
		
			var forward = transform.forward * input.magnitude;
			var sideways = Vector3.zero;
			
			controllerAdapter.Move((forward + sideways) * currentSpeed * Time.fixedDeltaTime, Time.fixedDeltaTime);

			previouslyHasInput = characterInput.hasMovementInput;
		}	

		/// <summary>
		/// Calculates current speed based on acceleration anim curve
		/// </summary>
		void Accelerate()
		{
			movementTime += Time.fixedDeltaTime;
			movementTime = Mathf.Clamp(movementTime, 0f, timeToMaxSpeed);
			currentSpeed = movementTime / timeToMaxSpeed * maxSpeed;
		}
		
		/// <summary>
		/// Stops the movement
		/// </summary>
		void Stop()
		{
			currentSpeed = 0f;
		}
	}
}