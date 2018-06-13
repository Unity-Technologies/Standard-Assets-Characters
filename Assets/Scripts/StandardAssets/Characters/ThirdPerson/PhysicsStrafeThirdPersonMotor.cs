using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(ICharacterPhysics))]
    [RequireComponent(typeof(ICharacterInput))]
	public class PhysicsStrafeThirdPersonMotor : BaseThirdPersonMotor
    {
        /// <summary>
        /// Movement values
        /// </summary>
        [SerializeField]
        private Transform lookForwardTransform;

        [SerializeField]
        private float maxForwardSpeed = 10f, maxLateralSpeed = 10f;
        
        [SerializeField]
        private bool useAcceleration = true;
        
        [SerializeField]
        private float groundAcceleration = 20f;
        
        [SerializeField]
        private float groundDeceleration = 15f;

        [Range(0f, 1f)] 
        [SerializeField]
        private float airborneAccelProportion = 0.5f;

        [Range(0f, 1f)] 
        [SerializeField]
        private float airborneDecelProportion = 0.5f;

        [SerializeField]
        private float jumpSpeed = 15f;

        [Range(0f, 1f)] 
        [SerializeField]
        private float airborneTurnSpeedProportion = 0.5f;

        [SerializeField]
        private float angleSnapBehaviour = 120f;
        
        [SerializeField]
        private float maxTurnSpeed = 10000f;

        [SerializeField]
        private AnimationCurve turnSpeedAsAFunctionOfForwardSpeed = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        /// The input implementation
        /// </summary>
        private ICharacterInput characterInput;

        /// <summary>
        /// The physic implementation
        /// </summary>
        private ICharacterPhysics characterPhysics;

        private float currentForwardSpeed;
        private float currentLateralSpeed;
        
        /// <inheritdoc />
        public override float normalizedLateralSpeed
        {
            get { return -currentLateralSpeed / maxLateralSpeed; }
        }

        /// <inheritdoc />
        public override float normalizedForwardSpeed
        {
            get { return currentForwardSpeed / maxForwardSpeed; }
        }

        public override float fallTime
        {
            get { return characterPhysics.fallTime; }
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets required components
        /// </summary>
        protected override void Awake()
        {
            characterInput = GetComponent<ICharacterInput>();
            characterPhysics = GetComponent<ICharacterPhysics>();
            base.Awake();
        }

        /// <summary>
        /// Subscribe
        /// </summary>
        private void OnEnable()
        {
            characterInput.jumpPressed += OnJumpPressed;
            characterPhysics.landed += OnLanding;
        }

        /// <summary>
        /// Unsubscribe
        /// </summary>
        private void OnDisable()
        {
            if (characterInput != null)
            {
                characterInput.jumpPressed -= OnJumpPressed;
            }

            if (characterPhysics != null)
            {
                characterPhysics.landed -= OnLanding;
            }
        }

        /// <summary>
        /// Handles player landing
        /// </summary>
        private void OnLanding()
        {
            if (landed != null)
            {
                landed();
            }
        }

        /// <summary>
        /// Subscribes to the Jump action on input
        /// </summary>
        private void OnJumpPressed()
        {
            if (!characterPhysics.isGrounded)
            {
                return;
            }

            characterPhysics.SetJumpVelocity(jumpSpeed);
            if (jumpStarted != null)
            {
                jumpStarted();
            }
        }

        /// <summary>
        /// Movement Logic on physics update
        /// </summary>
        private void FixedUpdate()
        {
            SetLookDirection();
            CalculateMovement();
            if (animator == null)
            {
                Move();
            }
            CalculateYRotationSpeed(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Handle movement if the animator is set
        /// </summary>
        private void OnAnimatorMove()
        {
            if (animator != null)
            {
                Move();
            }
        }

        /// <summary>
        /// Sets forward rotation
        /// </summary>
        private void SetLookDirection()
        {
            Vector3 lookForwardY = lookForwardTransform.rotation.eulerAngles;
            lookForwardY.x = 0;
            lookForwardY.z = 0;
            Quaternion targetRotation = Quaternion.Euler(lookForwardY);

            float angleDifference = Mathf.Abs((transform.eulerAngles - targetRotation.eulerAngles).y);

            float actualTurnSpeed =
                characterPhysics.isGrounded ? turnSpeed : turnSpeed * airborneTurnSpeedProportion;
            targetRotation =
                Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * Time.fixedDeltaTime);

            transform.rotation = targetRotation;
        }

        /// <summary>
        /// Calculates the forward movement
        /// </summary>
        private void CalculateMovement()
        {
            Vector2 moveInput = characterInput.moveInput;

            currentForwardSpeed = CalculateSpeed(moveInput.y * maxForwardSpeed, currentForwardSpeed);
            currentLateralSpeed = CalculateSpeed(moveInput.x * maxLateralSpeed, currentLateralSpeed);
        }

        private float CalculateSpeed(float desiredSpeed, float currentSpeed)
        {
            if (useAcceleration)
            {
                float acceleration = characterPhysics.isGrounded
                    ? (characterInput.hasMovementInput ? groundAcceleration : groundDeceleration)
                    : (characterInput.hasMovementInput ? groundAcceleration : groundDeceleration) *
                      airborneDecelProportion;

                return 
                    Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                return desiredSpeed;
            }
        }

        /// <summary>
        /// Moves the character
        /// </summary>
        private void Move()
        {
            Vector3 movement;

            if (animator != null && characterPhysics.isGrounded &&
                animator.deltaPosition.z >= groundAcceleration * Time.fixedDeltaTime)
            {
                movement = animator.deltaPosition;
            }
            else
            {
                Vector3 lateral = (currentLateralSpeed * transform.right * Time.fixedDeltaTime);
                Debug.LogFormat("Lateral Speed = {0}", lateral);
                movement = (currentForwardSpeed * transform.forward * Time.fixedDeltaTime + lateral);
            }

            characterPhysics.Move(movement);
        }
    }
}