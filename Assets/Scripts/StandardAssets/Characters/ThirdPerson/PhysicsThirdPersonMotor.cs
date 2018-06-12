using System;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace StandardAssets.Characters.ThirdPerson
{
    /// <summary>
    /// The main third person controller
    /// </summary>
    [RequireComponent(typeof(ICharacterPhysics))]
    [RequireComponent(typeof(ICharacterInput))]
    public class PhysicsThirdPersonMotor : BaseThirdPersonMotor
    {
        /// <summary>
        /// Movement values
        /// </summary>
        [SerializeField]
        private Transform cameraTransform;

        [SerializeField]
        private float maxForwardSpeed = 10f;
        
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
        
        /// <inheritdoc />
        public override float normalizedLateralSpeed
        {
            get { return 0f; }
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
            SetForward();
            CalculateForwardMovement();
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
        private void SetForward()
        {
            if (!characterInput.hasMovementInput)
            {
                return;
            }

            Vector3 flatForward = cameraTransform.forward;
            flatForward.y = 0f;
            flatForward.Normalize();

            Vector3 localMovementDirection =
                new Vector3(characterInput.moveInput.x, 0f, characterInput.moveInput.y);

            Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
            cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

            Quaternion targetRotation = Quaternion.LookRotation(cameraToInputOffset * flatForward);

            float angleDifference = Mathf.Abs((transform.eulerAngles - targetRotation.eulerAngles).y);

            float calculatedTurnSpeed = 0;
            if (angleDifference > angleSnapBehaviour)
            {
                calculatedTurnSpeed = turnSpeed + (maxTurnSpeed - turnSpeed) *
                                      turnSpeedAsAFunctionOfForwardSpeed.Evaluate(
                                          Mathf.Abs(normalizedForwardSpeed));
            }
            else
            {
                calculatedTurnSpeed = turnSpeed;
            }

            float actualTurnSpeed =
                characterPhysics.isGrounded ? calculatedTurnSpeed : calculatedTurnSpeed * airborneTurnSpeedProportion;
            targetRotation =
                Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * Time.fixedDeltaTime);

            transform.rotation = targetRotation;
        }

        /// <summary>
        /// Calculates the forward movement
        /// </summary>
        private void CalculateForwardMovement()
        {
            Vector2 moveInput = characterInput.moveInput;
            if (moveInput.sqrMagnitude > 1f)
            {
                moveInput.Normalize();
            }

            float desiredSpeed = moveInput.magnitude * maxForwardSpeed;

            if (useAcceleration)
            {
                float acceleration = characterPhysics.isGrounded
                    ? (characterInput.hasMovementInput ? groundAcceleration : groundDeceleration)
                    : (characterInput.hasMovementInput ? groundAcceleration : groundDeceleration) *
                      airborneDecelProportion;

                currentForwardSpeed =
                    Mathf.MoveTowards(currentForwardSpeed, desiredSpeed, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentForwardSpeed = desiredSpeed;
            }
        }

        /// <summary>
        /// Moves the character
        /// </summary>
        private void Move()
        {
            Vector3 movement;

            if (animator != null && characterPhysics.isGrounded &&
                animator.deltaPosition.z >= groundAcceleration * Time.deltaTime)
            {
                movement = animator.deltaPosition;
            }
            else
            {
                movement = currentForwardSpeed * transform.forward * Time.fixedDeltaTime;
            }

            characterPhysics.Move(movement);
        }
    }
}