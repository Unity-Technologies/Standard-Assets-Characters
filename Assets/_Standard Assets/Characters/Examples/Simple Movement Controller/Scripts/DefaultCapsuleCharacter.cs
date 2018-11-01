using UnityEngine;

namespace StandardAssets.Characters.Examples.SimpleMovementController
{
	[RequireComponent(typeof(CapsuleInput))]
	[RequireComponent(typeof(CharacterController))]
	public class DefaultCapsuleCharacter : MonoBehaviour
	{
		[SerializeField]
		protected float maxSpeed = 5f;

		[SerializeField]
		protected float timeToMaxSpeed = 0.5f;

		[SerializeField]
		protected float turnSpeed = 300f;
		
		private float movementTime, currentSpeed;
		
		private bool previouslyHasInput;
		
		private CapsuleInput characterInput;

		private CharacterController controller;

		private Transform mainCameraTransform;

		private void Awake()
		{
			characterInput = GetComponent<CapsuleInput>();
			controller = GetComponent<CharacterController>();
			mainCameraTransform = Camera.main.transform;
		}

		private void OnEnable()
		{
			//Subscribe
		}

		private void OnDisable()
		{
			//Unsubscribe
		}

		private void Update()
		{
			if (!characterInput.hasMovementInput)
			{
				return;
			}

			Vector3 flatForward = mainCameraTransform.forward;
			flatForward.y = 0f;
			flatForward.Normalize();

			Vector3 localMovementDirection = new Vector3(characterInput.moveInput.x, 0f, characterInput.moveInput.y);
			Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			Quaternion targetRotation = Quaternion.LookRotation(cameraToInputOffset * flatForward);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
		}
		
		/// <summary>
		/// Handles movement on Physics update
		/// </summary>
		private void FixedUpdate()
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

			Vector2 input = characterInput.moveInput;
			if (input.sqrMagnitude > 1)
			{
				input.Normalize();
			}
		
			Vector3 forward = transform.forward * input.magnitude;
			Vector3 sideways = Vector3.zero;
			
			controller.SimpleMove((forward + sideways) * currentSpeed /* Time.fixedDeltaTime*/);

			previouslyHasInput = characterInput.hasMovementInput;
		}

		/// <summary>
		/// Calculates current speed based on acceleration anim curve
		/// </summary>
		private void Accelerate()
		{
			movementTime += Time.fixedDeltaTime;
			movementTime = Mathf.Clamp(movementTime, 0f, timeToMaxSpeed);
			currentSpeed = movementTime / timeToMaxSpeed * maxSpeed;
		}
		
		/// <summary>
		/// Stops the movement
		/// </summary>
		private void Stop()
		{
			currentSpeed = 0f;
		}
	}
}