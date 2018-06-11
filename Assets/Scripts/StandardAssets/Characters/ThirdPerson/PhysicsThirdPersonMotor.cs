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
        public Transform cameraTransform;

        public float maxForwardSpeed = 10f;
        public bool useAcceleration = true;
        public float groundAcceleration = 20f;
        public float groundDeceleration = 15f;

        [Range(0f, 1f)] public float airborneAccelProportion = 0.5f;

        [Range(0f, 1f)] public float airborneDecelProportion = 0.5f;

        public float jumpSpeed = 15f;

        [Range(0f, 1f)] public float airborneTurnSpeedProportion = 0.5f;


        public float angleSnapBehaviour = 120f;
        public float maxTurnSpeed = 10000f;

        public AnimationCurve turnSpeedAsAFunctionOfForwardSpeed = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        /// The input implementation
        /// </summary>
        ICharacterInput m_CharacterInput;

        /// <summary>
        /// The physic implementation
        /// </summary>
        ICharacterPhysics m_CharacterPhysics;

        /// <inheritdoc />
        public override float normalizedLateralSpeed
        {
            get { return 0f; }
        }

        /// <inheritdoc />
        public override float normalizedForwardSpeed
        {
            get { return m_CurrentForwardSpeed / maxForwardSpeed; }
        }

        public override float fallTime
        {
            get { return m_CharacterPhysics.fallTime; }
        }

        float m_CurrentForwardSpeed;

        /// <inheritdoc />
        /// <summary>
        /// Gets required components
        /// </summary>
        protected override void Awake()
        {
            m_CharacterInput = GetComponent<ICharacterInput>();
            m_CharacterPhysics = GetComponent<ICharacterPhysics>();
            base.Awake();
        }

        /// <summary>
        /// Subscribe
        /// </summary>
        void OnEnable()
        {
            m_CharacterInput.jumpPressed += OnJumpPressed;
            m_CharacterPhysics.landed += OnLanding;
        }

        /// <summary>
        /// Unsubscribe
        /// </summary>
        void OnDisable()
        {
            if (m_CharacterInput != null)
            {
                m_CharacterInput.jumpPressed -= OnJumpPressed;
            }

            if (m_CharacterPhysics != null)
            {
                m_CharacterPhysics.landed -= OnLanding;
            }
        }

        /// <summary>
        /// Handles player landing
        /// </summary>
        void OnLanding()
        {
            if (landed != null)
            {
                landed();
            }
        }

        /// <summary>
        /// Subscribes to the Jump action on input
        /// </summary>
        void OnJumpPressed()
        {
            if (m_CharacterPhysics.isGrounded)
            {
                m_CharacterPhysics.SetJumpVelocity(jumpSpeed);
                if (jumpStarted != null)
                {
                    jumpStarted();
                }
            }
        }

        /// <summary>
        /// Movement Logic on physics update
        /// </summary>
        void FixedUpdate()
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
        void SetForward()
        {
            if (!m_CharacterInput.hasMovementInput)
            {
                return;
            }

            Vector3 flatForward = cameraTransform.forward;
            flatForward.y = 0f;
            flatForward.Normalize();

            Vector3 localMovementDirection =
                new Vector3(m_CharacterInput.moveInput.x, 0f, m_CharacterInput.moveInput.y);

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
                m_CharacterPhysics.isGrounded ? calculatedTurnSpeed : calculatedTurnSpeed * airborneTurnSpeedProportion;
            targetRotation =
                Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * Time.fixedDeltaTime);

            transform.rotation = targetRotation;
        }

        /// <summary>
        /// Calculates the forward movement
        /// </summary>
        void CalculateForwardMovement()
        {
            Vector2 moveInput = m_CharacterInput.moveInput;
            if (moveInput.sqrMagnitude > 1f)
            {
                moveInput.Normalize();
            }

            float desiredSpeed = moveInput.magnitude * maxForwardSpeed;

            if (useAcceleration)
            {
                float acceleration = m_CharacterPhysics.isGrounded
                    ? (m_CharacterInput.hasMovementInput ? groundAcceleration : groundDeceleration)
                    : (m_CharacterInput.hasMovementInput ? groundAcceleration : groundDeceleration) *
                      airborneDecelProportion;

                m_CurrentForwardSpeed =
                    Mathf.MoveTowards(m_CurrentForwardSpeed, desiredSpeed, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                m_CurrentForwardSpeed = desiredSpeed;
            }
        }

        /// <summary>
        /// Moves the character
        /// </summary>
        void Move()
        {
            Vector3 movement;

            if (animator != null && m_CharacterPhysics.isGrounded &&
                animator.deltaPosition.z >= groundAcceleration * Time.deltaTime)
            {
                movement = animator.deltaPosition;
            }
            else
            {
                movement = m_CurrentForwardSpeed * transform.forward * Time.fixedDeltaTime;
            }

            m_CharacterPhysics.Move(movement);
        }
    }
}