using System;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Helpers;
using StandardAssets.Characters.ThirdPerson.Configs;
using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(IThirdPersonInput))]
    public class ThirdPersonBrain : CharacterBrain
    {
        const float k_HeadTurnSnapBackScale = 100f;
        const float k_VelocityGizmoScale = 0.2f, k_VelocityLerp = 3f;

        /// <summary>
        /// Character animation states
        /// </summary>
        public enum AnimatorState
        {
            Locomotion,
            Jump,
            Falling,
            Landing,
            JumpLanding,
        }

        [SerializeField, Tooltip("Properties of the root motion motor")]
        ThirdPersonMotor m_Motor;

        [SerializeField, Tooltip("Mechanism used in rapid direction changes")]
        TurnAroundType m_TurnAroundType;

        [SerializeField, Tooltip("Properties of the blendspace-based turn around")]
        BlendspaceTurnAroundBehaviour m_BlendspaceTurnAroundBehaviour;

        [SerializeField, Tooltip("Properties of the animation-based turn around")]
        AnimationTurnAroundBehaviour m_AnimationTurnAroundBehaviour;

        [SerializeField, Tooltip("Properties of the movement event handler")]
        ThirdPersonMovementEventHandler m_ThirdPersonMovementEventHandler;

        [SerializeField, Tooltip("Configuration settings for the animator")]
        AnimationConfig m_Configuration;

        [SerializeField, Tooltip("Show the debug gizmos?")]
        bool m_ShowDebugGizmos;

        [SerializeField, Tooltip("Properties of the debug gizmos")]
        DebugGizmoSettings m_GizmoSettings;

        [Serializable]
        public struct DebugGizmoSettings
        {
            [SerializeField, Tooltip("Arrow prefab used for each gizmo")]
            GameObject m_ArrowPrefab;

            [SerializeField, Tooltip("Show the body direction gizmos?")]
            bool m_ShowBodyGizmos;

            [SerializeField, Tooltip("Color of body current direction gizmo")]
            Color m_BodyCurrentDirection;

            [SerializeField, Tooltip("Color of body desired direction gizmo")]
            Color m_BodyDesiredDirection;

            [SerializeField, Tooltip("Show the input direction and velocity indicator gizmos?")]
            bool m_ShowFootGizmos;
            
            [SerializeField, Tooltip("Color of input direction gizmo")]
            Color m_InputDirection;

            [SerializeField, Tooltip("Color of velocity indicator gizmo")]
            Color m_VelocityIndicator;

            [SerializeField, Tooltip("Show the head direction gizmos?")]
            bool m_ShowHeadGizmos;

            [SerializeField, Tooltip("Color of head current direction gizmo")]
            Color m_HeadCurrentDirection;

            [SerializeField, Tooltip("Color of head desired direction gizmo")]
            Color m_HeadDesiredDirection;

            /// <summary>
            /// Gets the arrow prefab
            /// </summary>
            public GameObject arrowPrefab
            {
                get { return m_ArrowPrefab; }
            }

            /// <summary>
            /// Gets the show body gizmos bool
            /// </summary>
            public bool showBodyGizmos
            {
                get { return m_ShowBodyGizmos; }
            }

            /// <summary>
            /// Gets the body current direction color
            /// </summary>
            public Color bodyCurrentDirection
            {
                get { return m_BodyCurrentDirection; }
            }

            /// <summary>
            /// Gets the body desired direction color
            /// </summary>
            public Color bodyDesiredDirection
            {
                get { return m_BodyDesiredDirection; }
            }

            /// <summary>
            /// Gets the show foot gizmos bool
            /// </summary>
            public bool showFootGizmos
            {
                get { return m_ShowFootGizmos; }
            }

            /// <summary>
            /// Gets the input direction color
            /// </summary>
            public Color inputDirection
            {
                get { return m_InputDirection; }
            }

            /// <summary>
            /// Gets the velocity indicator color
            /// </summary>
            public Color velocityIndicator
            {
                get { return m_VelocityIndicator; }
            }

            /// <summary>
            /// Gets the show head gizmos bool
            /// </summary>
            public bool showHeadGizmos
            {
                get { return m_ShowHeadGizmos; }
            }

            /// <summary>
            /// Gets the head current direction color
            /// </summary>
            public Color headCurrentDirection
            {
                get { return m_HeadCurrentDirection; }
            }

            /// <summary>
            /// Gets the head desired direction color
            /// </summary>
            public Color headDesiredDirection
            {
                get { return m_HeadDesiredDirection; }
            }
        }

        //Gizmo readonly vectors
        readonly Vector3 m_HeadGizmoPosition = new Vector3(0f, 1.55f, 0f);
        readonly Vector3 m_HeadGizmoScale = new Vector3(0.05f, 0.05f, 0.05f);
        readonly Vector3 m_BodyGizmoPosition = new Vector3(0f, 0.8f, 0.1f);
        readonly Vector3 m_BodyGizmoScale = new Vector3(0.1f, 0.1f, 0.1f);
        readonly Vector3 m_FootGizmoPosition = new Vector3(0f, 0.05f, 0f);
        readonly Vector3 m_FootGizmoScale = new Vector3(0.1f, 0.1f, 0.1f);
        readonly Vector3 m_GizmoOffset = new Vector3(0f, 0.025f, 0f);

        //current scale of the velocity gizmo
        float m_VelocityGizmoScale;

        // Hashes of the animator parameters
        int m_HashForwardSpeed;
        int m_HashLateralSpeed;
        int m_HashTurningSpeed;
        int m_HashVerticalSpeed;
        int m_HashGroundedFootRight;
        int m_HashFall;
        int m_HashStrafe;
        int m_HashSpeedMultiplier;

        // is the character grounded
        bool m_IsGrounded;

        // was the last physics jump taken during a planted right foot
        bool m_LastJumpWasRightRoot;

        // angle of the head for look direction
        float m_HeadAngle;
        float m_TargetHeadAngle;

        // cached default animator speed
        float m_CachedAnimatorSpeed = 1.0f;

        // time of the last physics jump
        float m_TimeOfLastJumpLand;

        // whether locomotion mode is set to strafe
        bool m_IsStrafing;

        bool m_IsTryingStrafe;

        bool m_JustJumped;

        //the camera controller to be used
        ThirdPersonCameraController m_CameraController;
        bool m_IsChangingCamera;

        bool m_TriggeredRapidDirectionChange;
        int m_FramesToWait;

        IThirdPersonInput m_Input;

        GameObject[] m_GizmoObjects;

        GameObject m_BodyCurrentGizmo,
            m_BodyDesiredGizmo,
            m_InputDirectionGizmo,
            m_VelocityGizmo,
            m_HeadCurrentGizmo,
            m_HeadDesiredGizmo;

        Transform m_MainCameraTransform;

        TurnAroundBehaviour[] m_TurnAroundBehaviours;

        float m_RapidStrafeTime, m_RapidStrafeChangeDuration;

        /// <summary>
        /// Gets the <see cref="ThirdPersonMotor"/>
        /// </summary>
        public ThirdPersonMotor thirdPersonMotor
        {
            get { return m_Motor; }
        }

        /// <summary>
        /// Gets the <see cref="TurnAroundType"/>
        /// </summary>
        public TurnAroundType typeOfTurnAround
        {
            get { return m_TurnAroundType; }
        }

        /// <summary>
        /// Gets/sets the <see cref="TurnAroundBehaviour"/>
        /// </summary>
        public TurnAroundBehaviour turnAround { get; private set; }

        /// <summary>
        /// Gets all of the <see cref="TurnAroundBehaviour"/>
        /// </summary>
        public TurnAroundBehaviour[] turnAroundOptions
        {
            get
            {
                if (m_TurnAroundBehaviours == null)
                {
                    m_TurnAroundBehaviours = new TurnAroundBehaviour[]
                    {
                        m_BlendspaceTurnAroundBehaviour,
                        m_AnimationTurnAroundBehaviour
                    };
                }

                return m_TurnAroundBehaviours;
            }
        }

        /// <summary>
        /// Gets the <see cref="ThirdPersonMotor"/>'s normalized forward speed
        /// </summary>
        public override float normalizedForwardSpeed
        {
            get { return m_Motor.normalizedForwardSpeed; }
        }

        /// <summary>
        /// Gets/sets the character target Y rotation
        /// </summary>
        public override float targetYRotation { get; set; }

        /// <summary>
        /// Gets the <see cref="ThirdPersonCameraController"/>
        /// </summary>
        public ThirdPersonCameraController thirdPersonCameraController
        {
            get { return m_CameraController; }
        }

        /// <summary>
        /// Gets the <see cref="IThirdPersonInput"/>
        /// </summary>
        public IThirdPersonInput thirdPersonInput
        {
            get
            {
                if (m_Input == null)
                {
                    m_Input = GetComponent<IThirdPersonInput>();
                }

                return m_Input;
            }
        }

        /// <summary>
        /// Gets the animation state of the character.
        /// </summary>
        /// <value>The current <see cref="AnimatorState"/></value>
        public AnimatorState animatorState { get; private set; }

        /// <summary>
        /// Gets the character animator.
        /// </summary>
        /// <value>The Unity Animator component</value>
        public Animator animator { get; private set; }

        /// <summary>
        /// Gets the animator forward speed.
        /// </summary>
        /// <value>The animator forward speed parameter.</value>
        public float animatorForwardSpeed
        {
            get { return animator.GetFloat(m_HashForwardSpeed); }
        }

        /// <summary>
        /// Gets the animator turning speed.
        /// </summary>
        /// <value>The animator forward speed parameter.</value>
        public float animatorTurningSpeed
        {
            get { return animator.GetFloat(m_HashTurningSpeed); }
        }

        /// <summary>
        /// Gets whether the right foot is planted
        /// </summary>
        /// <value>True is the right foot is planted; false if the left.</value>
        public bool isRightFootPlanted { get; private set; }

        /// <summary>
        /// Gets whether the character in a root motion state.
        /// </summary>
        /// <value>True if the state is in a grounded state; false if aerial.</value>
        public bool isRootMotionState
        {
            get
            {
                return animatorState == AnimatorState.Locomotion ||
                    animatorState == AnimatorState.Landing;
            }
        }

        /// <summary>
        /// The value of the animator lateral speed parameter.
        /// </summary>
        float animatorLateralSpeed
        {
            get { return animator.GetFloat(m_HashLateralSpeed); }
        }

        /// <summary>
        /// Called on the exit of the land animation.
        /// </summary>
        /// <remarks>Should only be called by a land StateMachineBehaviour</remarks>
        public void OnLandAnimationExit()
        {
            if (m_IsGrounded)
            {
                animatorState = AnimatorState.Locomotion;
            }

            animator.speed = m_CachedAnimatorSpeed;
        }

        /// <summary>
        /// Called on the enter of the land animation.
        /// </summary>
        /// <remarks>Should only be called by a land StateMachineBehaviour</remarks>
        /// <param name="adjustAnimationSpeedBasedOnForwardSpeed">Should the animator speed be adjusted during the land animation</param>
        public void OnLandAnimationEnter(bool adjustAnimationSpeedBasedOnForwardSpeed)
        {
            animatorState = AnimatorState.Landing;
            if (adjustAnimationSpeedBasedOnForwardSpeed)
            {
                animator.speed = m_Configuration.landSpeedAsAFactorSpeed.Evaluate(m_Motor.normalizedForwardSpeed);
            }
        }

        /// <summary>
        /// Called on the exit of the physics jump animation.
        /// </summary>
        /// <remarks>Should only be called by a physics jump StateMachineBehaviour</remarks>
        public void OnJumpAnimationExit()
        {
            if (animatorState == AnimatorState.Jump || animatorState == AnimatorState.JumpLanding)
            {
                animatorState = AnimatorState.Locomotion;
            }
        }

        /// <summary>
        /// Called on the enter of the physics jump animation.
        /// </summary>
        /// <remarks>Should only be called by a physics jump StateMachineBehaviour</remarks>
        public void OnJumpAnimationEnter()
        {
            animatorState = AnimatorState.Jump;
        }

        /// <summary>
        /// Called on the enter of the locomotion animation.
        /// </summary>
        /// <remarks>Should only be called by a locomotion StateMachineBehaviour</remarks>
        public void OnLocomotionAnimationEnter()
        {
            if (animatorState == AnimatorState.Falling)
            {
                animatorState = AnimatorState.Locomotion;
            }
            else if (animatorState == AnimatorState.Jump)
            {
                animatorState = AnimatorState.JumpLanding;
            }
        }

        /// <summary>
        /// Called on the enter of the falling animation.
        /// </summary>
        /// <remarks>Should only be called by a falling StateMachineBehaviour</remarks>
        public void OnFallingLoopAnimationEnter()
        {
            animatorState = AnimatorState.Falling;
        }

        /// <summary>
        /// Update the animator forward speed parameter.
        /// </summary>
        /// <param name="newSpeed">New forward speed</param>
        /// <param name="deltaTime">Interpolation delta time</param>
        public void UpdateForwardSpeed(float newSpeed, float deltaTime)
        {
            animator.SetFloat(m_HashForwardSpeed, newSpeed,
                m_Configuration.forwardSpeedInterpolation.GetInterpolationTime(
                    animatorForwardSpeed, newSpeed), deltaTime);
        }

        /// <summary>
        /// Update the animator lateral speed parameter.
        /// </summary>
        /// <param name="newSpeed">New forward speed</param>
        /// <param name="deltaTime">Interpolation delta time</param>
        public void UpdateLateralSpeed(float newSpeed, float deltaTime)
        {
            animator.SetFloat(m_HashLateralSpeed, newSpeed,
                m_Configuration.lateralSpeedInterpolation.GetInterpolationTime(
                    animatorLateralSpeed, newSpeed), deltaTime);
        }

        /// <summary>
        /// Update the animator turning speed parameter.
        /// </summary>
        /// <param name="newSpeed">New turning speed</param>
        /// <param name="deltaTime">Interpolation delta time</param>
        public void UpdateTurningSpeed(float newSpeed, float deltaTime)
        {
            // remap turning speed
            newSpeed = m_Configuration.animationTurningSpeedCurve.Evaluate(Mathf.Abs(newSpeed)) * Mathf.Sign(newSpeed);
            animator.SetFloat(m_HashTurningSpeed, newSpeed,
                m_Configuration.turningSpeedInterpolation.GetInterpolationTime(
                    animatorTurningSpeed, newSpeed), deltaTime);
        }

        /// <summary>
        /// Initializes <see cref="TurnAroundBehaviour"/>s, <see cref="ThirdPersonMotor"/>, <see cref="ThirdPersonMovementEventHandler"/>, cameras, gizmos and the animator.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            m_BlendspaceTurnAroundBehaviour.Init(this);
            m_AnimationTurnAroundBehaviour.Init(this);
            turnAround = GetCurrentTurnaroundBehaviour();
            m_Motor.Init(this);

            InitAnimator();
            m_MainCameraTransform = Camera.main.transform;

            m_ThirdPersonMovementEventHandler.Init(this);
            FindCameraController(true);
            SetupGizmos();
        }

        /// <summary>
        /// Sets up the gizmos when they are enabled (either in Awake or OnValidate)
        /// </summary>
        void SetupGizmos()
        {
            if (Application.isPlaying && m_GizmoObjects == null && m_ShowDebugGizmos)
            {
                m_GizmoObjects = new GameObject[6];

                m_InputDirectionGizmo = SetupGizmoObject(m_GizmoSettings.inputDirection, 0, m_FootGizmoScale, m_FootGizmoPosition);
                m_GizmoObjects[0] = m_InputDirectionGizmo;

                m_VelocityGizmo = SetupGizmoObject(m_GizmoSettings.velocityIndicator, 1, m_FootGizmoScale, m_FootGizmoPosition + m_GizmoOffset);
                m_GizmoObjects[1] = m_VelocityGizmo;

                m_BodyCurrentGizmo = SetupGizmoObject(m_GizmoSettings.bodyCurrentDirection, 10, m_BodyGizmoScale, m_BodyGizmoPosition);
                m_GizmoObjects[2] = m_BodyCurrentGizmo;

                m_BodyDesiredGizmo = SetupGizmoObject(m_GizmoSettings.bodyDesiredDirection, 11, m_BodyGizmoScale, m_BodyGizmoPosition + m_GizmoOffset);
                m_GizmoObjects[3] = m_BodyDesiredGizmo;

                m_HeadCurrentGizmo = SetupGizmoObject(m_GizmoSettings.headCurrentDirection, 20, m_HeadGizmoScale, m_HeadGizmoPosition);
                m_GizmoObjects[4] = m_HeadCurrentGizmo;

                m_HeadDesiredGizmo = SetupGizmoObject(m_GizmoSettings.headDesiredDirection, 21, m_HeadGizmoScale, m_HeadGizmoPosition + m_GizmoOffset);
                m_GizmoObjects[5] = m_HeadDesiredGizmo;
            }

            if (m_GizmoObjects != null && m_GizmoObjects.Length > 0)
            {
                m_InputDirectionGizmo.SetActive(m_ShowDebugGizmos && m_GizmoSettings.showFootGizmos);
                m_VelocityGizmo.SetActive(m_ShowDebugGizmos && m_GizmoSettings.showFootGizmos);
                m_BodyCurrentGizmo.SetActive(m_ShowDebugGizmos && m_GizmoSettings.showBodyGizmos);
                m_BodyDesiredGizmo.SetActive(m_ShowDebugGizmos && m_GizmoSettings.showBodyGizmos);
                m_HeadCurrentGizmo.SetActive(m_ShowDebugGizmos && m_GizmoSettings.showHeadGizmos);
                m_HeadDesiredGizmo.SetActive(m_ShowDebugGizmos && m_GizmoSettings.showHeadGizmos);
            }
        }

        /// <summary>
        /// Helper function for initializing a gizmo GameObject
        /// </summary>
        /// <param name="gizmoColor">Color of the gizmo</param>
        /// <param name="sortingOrder">Sprite sorting order</param>
        /// <param name="localScale">Size of the gizmo</param>
        /// <param name="position">Position of the gizmo</param>
        /// <returns>Set up gizmo GameObject</returns>
        GameObject SetupGizmoObject(Color gizmoColor, int sortingOrder, Vector3 localScale, Vector3 position)
        {
            GameObject gameObj = Instantiate(m_GizmoSettings.arrowPrefab, transform);
            SpriteRenderer spriteRenderer = gameObj.GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.color = gizmoColor;
            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.transform.localScale = localScale;
            gameObj.transform.localPosition = position;
            return gameObj;
        }

        /// <summary>
        /// Updates animator, motor, turn around
        /// </summary>
        protected override void Update()
        {
            base.Update();

            UpdateAnimatorParameters();

            m_Motor.Update();

            if (turnAround != null)
            {
                turnAround.Update();
            }

            targetYRotation = m_Motor.targetYRotation;

            //Just for build testing
            //TODO remove
            if (Input.GetKeyDown(KeyCode.T))
            {
                m_TurnAroundType = m_TurnAroundType == TurnAroundType.Animation ? TurnAroundType.None : m_TurnAroundType + 1;
                turnAround = GetCurrentTurnaroundBehaviour();
            }
        }

        /// <summary>
        /// Updates the motor look direction and the gizmos
        /// </summary>
        void LateUpdate()
        {
            m_Motor.SetLookDirection();
            UpdateGizmos();
        }

        /// <summary>
        /// Updates the gizmos
        /// </summary>
        void UpdateGizmos()
        {
            if (!m_ShowDebugGizmos)
            {
                return;
            }

            m_HeadCurrentGizmo.transform.localRotation = Quaternion.Euler(0f, m_HeadAngle, 0f);
            m_HeadDesiredGizmo.transform.localRotation = Quaternion.Euler(0f, m_TargetHeadAngle, 0f);
            m_BodyDesiredGizmo.transform.rotation = Quaternion.Euler(0f, targetYRotation, 0f);
            m_InputDirectionGizmo.SetActive(m_ShowDebugGizmos && m_GizmoSettings.showFootGizmos && m_Input.hasMovementInput);

            if (m_Input.hasMovementInput)
            {
                float inputAngle = Vector2.SignedAngle(new Vector2(0f, 1f), m_Input.moveInput);
                m_InputDirectionGizmo.transform.rotation =
                    Quaternion.Euler(0, m_MainCameraTransform.eulerAngles.y - inputAngle, 0f);
                m_VelocityGizmoScale = Mathf.Lerp(m_VelocityGizmoScale, planarSpeed, Time.deltaTime * k_VelocityLerp);
            }
            else
            {
                m_VelocityGizmoScale = planarSpeed;
            }

            m_VelocityGizmo.transform.localScale = Vector3.one * m_VelocityGizmoScale * k_VelocityGizmoScale;
            m_VelocityGizmo.transform.rotation = Quaternion.Euler(
                0f, 180f + transform.eulerAngles.y + Vector3.SignedAngle(transform.forward, planarDisplacement, transform.up),
                0f);
        }

        /// <summary>
        /// Sets the Animator parameters.
        /// </summary>
        void UpdateAnimatorParameters()
        {
            UpdateTurningSpeed(m_Motor.normalizedTurningSpeed, Time.deltaTime);

            bool fullyGrounded = m_IsGrounded && animatorState != AnimatorState.Landing;

            // only update during landing if there is input to inhibit a jarring stop post land animation.
            bool landingWithInput = animatorState == AnimatorState.Landing &&
                (CheckHasSpeed(m_Motor.normalizedForwardSpeed) ||
                    CheckHasSpeed(m_Motor.normalizedLateralSpeed));

            if (fullyGrounded || landingWithInput
                || animatorState == AnimatorState.Falling
            ) // update during falling as landing animation depends on forward speed.
            {
                UpdateAnimationMovementSpeeds(Time.deltaTime);
                UpdateFoot();
            }
            else if (!m_JustJumped)
            {
                animator.SetFloat(m_HashVerticalSpeed, m_Motor.normalizedVerticalSpeed);
            }
            else
            {
                m_JustJumped = false;
            }
        }

        /// <summary>
        /// Checks if <see cref="ThirdPersonCameraController"/> has been assigned - otherwise looks for it in the scene
        /// </summary>
        void FindCameraController(bool autoDisable)
        {
            if (m_CameraController == null)
            {
                ThirdPersonCameraController[] cameraControllers =
                    FindObjectsOfType<ThirdPersonCameraController>();

                int length = cameraControllers.Length;
                if (length != 1)
                {
                    string errorMessage = "No ThirdPersonCameraAnimationManagers in scene! Disabling Brain";
                    if (length > 1)
                    {
                        errorMessage = "Too many ThirdPersonCameraAnimationManagers in scene! Disabling Brain";
                    }

                    if (autoDisable)
                    {
                        Debug.LogError(errorMessage);
                        gameObject.SetActive(false);
                    }

                    return;
                }

                m_CameraController = cameraControllers[0];
            }

            m_CameraController.SetThirdPersonBrain(this);
        }

        /// <summary>
        /// Gets the current turn around
        /// </summary>
        /// <returns>Current <see cref="TurnAroundBehaviour"/></returns>
        TurnAroundBehaviour GetCurrentTurnaroundBehaviour()
        {
            switch (m_TurnAroundType)
            {
                case TurnAroundType.None:
                    return null;
                case TurnAroundType.Blendspace:
                    return m_BlendspaceTurnAroundBehaviour;
                case TurnAroundType.Animation:
                    return m_AnimationTurnAroundBehaviour;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Subscribes to various events
        /// </summary>
        void OnEnable()
        {
            controllerAdapter.jumpVelocitySet += m_ThirdPersonMovementEventHandler.Jumped;
            controllerAdapter.landed += m_ThirdPersonMovementEventHandler.Landed;
            controllerAdapter.landed += OnLanding;

            m_Motor.jumpStarted += OnJumpStarted;
            m_Motor.fallStarted += OnFallStarted;
            m_Motor.Subscribe();

            if (thirdPersonInput != null)
            {
                ThirdPersonInput userInput = thirdPersonInput as ThirdPersonInput;
                if (userInput != null)
                {
                    userInput.jumpPressed += m_Motor.OnJumpPressed;
                    userInput.sprintStarted += m_Motor.OnSprintStarted;
                    userInput.sprintEnded += m_Motor.OnSprintEnded;
                    userInput.strafeStarted += OnStrafeStarted;
                    userInput.strafeEnded += OnStrafeEnded;
                    userInput.recentreCamera += RecenterCamera;
                }
            }

            m_ThirdPersonMovementEventHandler.Subscribe();
        }

        /// <summary>
        /// Unsubscribes from various events
        /// </summary>
        void OnDisable()
        {
            if (controllerAdapter != null)
            {
                controllerAdapter.jumpVelocitySet -= m_ThirdPersonMovementEventHandler.Jumped;
                controllerAdapter.landed -= m_ThirdPersonMovementEventHandler.Landed;
                controllerAdapter.landed -= OnLanding;
            }

            if (m_Motor != null)
            {
                if (thirdPersonInput != null)
                {
                    ThirdPersonInput userInput = thirdPersonInput as ThirdPersonInput;
                    if (userInput != null)
                    {
                        userInput.jumpPressed -= m_Motor.OnJumpPressed;
                        userInput.sprintStarted -= m_Motor.OnSprintStarted;
                        userInput.sprintEnded -= m_Motor.OnSprintEnded;
                        userInput.strafeStarted -= OnStrafeStarted;
                        userInput.strafeEnded -= OnStrafeEnded;
                        userInput.recentreCamera -= RecenterCamera;
                    }
                }

                m_Motor.jumpStarted -= OnJumpStarted;
                m_Motor.fallStarted -= OnFallStarted;
                m_Motor.Unsubscribe();
            }

            m_ThirdPersonMovementEventHandler.Unsubscribe();
        }

        /// <summary>
        /// Handles motor movement
        /// </summary>
        void OnAnimatorMove()
        {
            m_Motor.OnAnimatorMove();
        }

        /// <summary>
        /// Handles head turn
        /// </summary>
        /// <param name="layerIndex">Animation layer index as required by the Unity message</param>
        void OnAnimatorIK(int layerIndex)
        {
            if (!m_Configuration.enableHeadLookAt)
            {
                return;
            }

            if (m_Motor.currentAerialMovementState != ThirdPersonAerialMovementState.Grounded &&
                !m_Configuration.lookAtWhileAerial)
            {
                return;
            }

            if (m_Motor.currentGroundMovementState == ThirdPersonGroundMovementState.TurningAround &&
                !m_Configuration.lookAtWhileTurnaround)
            {
                return;
            }

            animator.SetLookAtWeight(m_Configuration.lookAtWeight);
            m_TargetHeadAngle = Mathf.Clamp((m_Motor.targetYRotation - gameObject.transform.eulerAngles.y).Wrap180(),
                -m_Configuration.lookAtMaxRotation, m_Configuration.lookAtMaxRotation);

            float headTurn = Time.deltaTime * m_Configuration.lookAtRotationSpeed;

            if (m_Motor.currentGroundMovementState == ThirdPersonGroundMovementState.TurningAround)
            {
                if (Mathf.Abs(m_TargetHeadAngle) < Mathf.Abs(m_HeadAngle))
                {
                    headTurn *= k_HeadTurnSnapBackScale;
                }
                else
                {
                    headTurn *= m_Motor.currentTurnAroundBehaviour.headTurnScale;
                }
            }

            if (Mathf.Approximately(m_Input.lookInput.sqrMagnitude, 0.0f))
            {
                headTurn *= m_Configuration.noLookInputHeadLookAtScale;
            }

            m_HeadAngle = Mathf.LerpAngle(m_HeadAngle, m_TargetHeadAngle, headTurn);

            Vector3 lookAtPos = animator.transform.position +
                Quaternion.AngleAxis(m_HeadAngle, Vector3.up) * animator.transform.forward * 100f;
            animator.SetLookAtPosition(lookAtPos);
        }

        /// <summary>
        /// Sets up the animator hashes and gets the required components.
        /// </summary>
        void InitAnimator()
        {
            m_HashForwardSpeed = Animator.StringToHash(AnimationControllerInfo.k_ForwardSpeedParameter);
            m_HashLateralSpeed = Animator.StringToHash(AnimationControllerInfo.k_LateralSpeedParameter);
            m_HashTurningSpeed = Animator.StringToHash(AnimationControllerInfo.k_TurningSpeedParameter);
            m_HashVerticalSpeed = Animator.StringToHash(AnimationControllerInfo.k_VerticalSpeedParameter);
            m_HashGroundedFootRight = Animator.StringToHash(AnimationControllerInfo.k_GroundedFootRightParameter);
            m_HashStrafe = Animator.StringToHash(AnimationControllerInfo.k_StrafeParameter);
            m_HashFall = Animator.StringToHash(AnimationControllerInfo.k_FallParameter);
            m_HashSpeedMultiplier = Animator.StringToHash(AnimationControllerInfo.k_SpeedMultiplier);
            animator = GetComponent<Animator>();
            m_CachedAnimatorSpeed = animator.speed;
        }

        /// <summary>
        /// Fires when the <see cref="m_Motor"/> enters the fall state.
        /// </summary>
        void OnFallStarted(float predictedFallDistance)
        {
            m_IsGrounded = false;
            animator.SetTrigger(m_HashFall);
        }

        /// <summary>
        /// Logic for dealing with animation on landing
        /// Fires when the <see cref="m_Motor"/> enters a rapid turn
        /// </summary>
        void OnLanding()
        {
            m_IsGrounded = true;

            switch (animatorState)
            {
                // if coming from a physics jump handle animation transition
                case AnimatorState.Jump:
                    bool rightFoot = animator.GetBool(m_HashGroundedFootRight);
                    float duration = m_Configuration.jumpEndTransitionAsAFactorOfSpeed.Evaluate(
                        Mathf.Abs(animator.GetFloat(AnimationControllerInfo.k_ForwardSpeedParameter)));
                    string locomotion = m_IsStrafing
                        ? AnimationControllerInfo.k_StrafeLocomotionState
                        : AnimationControllerInfo.k_LocomotionState;
                    animator.CrossFadeInFixedTime(locomotion, duration, 0, rightFoot
                        ? m_Configuration
                            .rightFootJumpLandAnimationOffset
                        : m_Configuration
                            .leftFootJumpLandAnimationOffset);
                    m_TimeOfLastJumpLand = Time.time;
                    break;
                case AnimatorState.Falling:

                    // strafe mode does not have a landing animation so transition directly to locomotion
                    if (m_IsStrafing)
                    {
                        animator.CrossFade(AnimationControllerInfo.k_StrafeLocomotionState,
                            m_Configuration.landAnimationBlendDuration);
                    }
                    else
                    {
                        if (m_Motor.normalizedForwardSpeed > m_Configuration.forwardSpeedRequiredToRoll
                        ) // moving fast enough to roll
                        {
                            if (m_Motor.fallTime > m_Configuration.fallTimeRequiredToRoll) // play roll
                            {
                                animator.CrossFade(AnimationControllerInfo.k_RollLandState,
                                    m_Configuration.rollAnimationBlendDuration);
                            }
                            else // has not fallen for long enough to roll
                            {
                                animator.CrossFade(AnimationControllerInfo.k_LocomotionState,
                                    m_Configuration.landAnimationBlendDuration);
                            }
                        }
                        else // play land 
                        {
                            animator.CrossFade(AnimationControllerInfo.k_LandState,
                                m_Configuration.landAnimationBlendDuration);
                        }
                    }

                    break;
            }

            SelectGroundedCamera();
        }

        /// <summary>
        /// Updates animator's forward and lateral speeds. If <see cref="AnimationConfig.enableStrafeRapidDirectionChangeSmoothing"/>
        /// is enabled special smoothing logic is performed when a strafe rapid direction change is detected.
        /// </summary>
        void UpdateAnimationMovementSpeeds(float deltaTime)
        {
            // if in strafe move and moving enough perform strafe rapid direction change logic
            if (m_Configuration.enableStrafeRapidDirectionChangeSmoothingLogic &&
                m_Motor.movementMode == ThirdPersonMotorMovementMode.Strafe &&
                (Mathf.Abs(animatorLateralSpeed) >= 0.5f || Mathf.Abs(animatorForwardSpeed) >= 0.5f))
            {
                if (m_TriggeredRapidDirectionChange)
                {
                    m_RapidStrafeTime += deltaTime;
                    float progress = m_Configuration.strafeRapidChangeSpeedCurve.Evaluate(
                        m_RapidStrafeTime / m_RapidStrafeChangeDuration);
                    float current = Mathf.Clamp(1.0f - (2.0f * progress), -1.0f, 1.0f);
                    animator.SetFloat(m_HashSpeedMultiplier, current);

                    CheckForStrafeRapidDirectionChangeComplete(deltaTime);
                    return;
                }

                // check if a rapid direction change has occured.
                float angle = (Vector2.Angle(m_Input.moveInput, new Vector2(animatorLateralSpeed, animatorForwardSpeed)));
                if (angle >= m_Configuration.strafeRapidChangeAngleThreshold)
                {
                    m_TriggeredRapidDirectionChange = !CheckForStrafeRapidDirectionChangeComplete(deltaTime);
                    m_RapidStrafeTime = 0.0f;
                    return;
                }
            }

            // not rapid strafe direction change, update as normal
            UpdateLateralSpeed(m_Motor.normalizedLateralSpeed, deltaTime);
            UpdateForwardSpeed(m_Motor.normalizedForwardSpeed, deltaTime);
        }

        /// <summary>
        /// Checks if the strafe rapid direction change (e.g. left to right and vice versa) is complete
        /// </summary>
        /// <param name="deltaTime">Time difference per Update frame</param>
        /// <returns>true if complete, false otherwise</returns>
        bool CheckForStrafeRapidDirectionChangeComplete(float deltaTime)
        {
            if (IsNormalizedTimeCloseToZeroOrHalf(deltaTime, out m_RapidStrafeChangeDuration))
            {
                animator.SetFloat(m_HashLateralSpeed, -animatorLateralSpeed);
                animator.SetFloat(m_HashForwardSpeed, -animatorForwardSpeed);
                m_TriggeredRapidDirectionChange = false;
                animator.SetFloat(m_HashSpeedMultiplier, 1.0f);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles camera recentering
        /// </summary>
        void RecenterCamera()
        {
            thirdPersonCameraController.RecenterCamera();
        }

        /// <summary>
        /// Sets the animator strafe parameter to true.
        /// </summary>
        void OnStrafeStarted()
        {
            m_IsChangingCamera = true;
            m_IsTryingStrafe = true;
            if (controllerAdapter.isGrounded)
            {
                SelectGroundedCamera();
            }
        }

        /// <summary>
        /// Sets the animator strafe parameter to false.
        /// </summary>
        void OnStrafeEnded()
        {
            m_IsChangingCamera = true;
            m_IsTryingStrafe = false;
            if (controllerAdapter.isGrounded)
            {
                SelectGroundedCamera();
            }
        }

        /// <summary>
        /// Helper for selecting correct camera on grounding
        /// </summary>
        void SelectGroundedCamera()
        {
            if (!m_IsChangingCamera)
            {
                return;
            }

            m_IsStrafing = m_IsTryingStrafe;
            animator.SetBool(m_HashStrafe, m_IsStrafing);
            if (m_IsStrafing)
            {
                m_Motor.StartStrafe();
                m_CameraController.SetStrafeCamera();
            }
            else
            {
                m_Motor.EndStrafe();
                m_CameraController.SetExplorationCamera();
            }

            m_IsChangingCamera = false;
        }

        /// <summary>
        /// Logic for dealing with animation on jumping
        /// Fires when the <see cref="m_Motor"/> enters a jump.
        /// </summary>
        void OnJumpStarted()
        {
            if (!m_IsGrounded)
            {
                return;
            }

            m_IsGrounded = false;

            float movementMagnitude = new Vector2(animatorLateralSpeed, animatorForwardSpeed).magnitude;
            bool rightFoot = animator.GetBool(m_HashGroundedFootRight);

            float duration = m_Motor.movementMode == ThirdPersonMotorMovementMode.Exploration
                ? m_Configuration.jumpTransitionAsAFactorOfSpeed.Evaluate(movementMagnitude)
                : m_Configuration.strafeJumpTransitionAsAFactorOfSpeed.Evaluate(movementMagnitude);

            // keep track of the last jump so legs can be alternated if necessary. ie a skip.
            if (m_TimeOfLastJumpLand + m_Configuration.skipJumpWindow >= Time.time)
            {
                rightFoot = !m_LastJumpWasRightRoot;
            }

            animator.SetFloat(m_HashVerticalSpeed, 1.0f);

            string jumpState;
            if (m_Motor.movementMode == ThirdPersonMotorMovementMode.Exploration)
            {
                jumpState = rightFoot
                    ? AnimationControllerInfo.k_RightFootJumpState
                    : AnimationControllerInfo.k_LeftFootJumpState;
            }
            else
            {
                jumpState = rightFoot
                    ? AnimationControllerInfo.k_RightFootStrafeJump
                    : AnimationControllerInfo.k_LeftFootStrafeJump;
            }

            animator.CrossFade(jumpState, duration);
            m_LastJumpWasRightRoot = rightFoot;

            m_JustJumped = true;
        }

        /// <summary>
        /// Checks if the normalized time is close to zero or half
        /// </summary>
        /// <param name="margin">Margin for deciding whether the time is close</param>
        /// <param name="timeUntilZeroOrHalf">how long until zero or half</param>
        /// <returns>true if close</returns>
        bool IsNormalizedTimeCloseToZeroOrHalf(float margin, out float timeUntilZeroOrHalf)
        {
            float value = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f;

            timeUntilZeroOrHalf = (value >= 0.5 ? 1.0f : 0.5f) - value;

            return (value > 1.0f - margin || value < margin ||
                (value > 0.5f - margin && value < 0.5f + margin));
        }

        /// <summary>
        /// Uses the normalized progress of the animation to determine the grounded foot.
        /// </summary>
        void UpdateFoot()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            var animationNormalizedProgress = stateInfo.normalizedTime.GetFraction();

            if ((animationNormalizedProgress + m_Configuration.groundedFootThresholdOffsetValue).Wrap1() >
                (m_Configuration.groundedFootThresholdValue + m_Configuration.groundedFootThresholdOffsetValue).Wrap1())
            {
                SetGroundedFootRight(!m_Configuration.invertFoot);
                return;
            }

            SetGroundedFootRight(m_Configuration.invertFoot);
        }

        /// <summary>
        /// Sets the grounded foot of the animator. This is used to play the appropriate footed animations.
        /// </summary>
        void SetGroundedFootRight(bool value)
        {
            animator.SetBool(m_HashGroundedFootRight, value);
            isRightFootPlanted = value;
        }

        /// <summary>
        /// Checks if character is moving
        /// </summary>
        /// <param name="speed">Speed value</param>
        /// <returns></returns>
        static bool CheckHasSpeed(float speed)
        {
            return Mathf.Abs(speed) > 0.0f;
        }

#if UNITY_EDITOR
        /// <summary>
        /// On reset of component
        /// </summary>
        void Reset()
        {
            //Design pattern for fetching required scene references
            FindCameraController(false);
        }

        /// <summary>
        /// On change of component
        /// </summary>
        void OnValidate()
        {
            turnAround = GetCurrentTurnaroundBehaviour();

            //Design pattern for fetching required scene references
            FindCameraController(false);

            SetupGizmos();
        }
#endif
    }

    /// <summary>
    /// Handles the third person movement event triggers and event IDs.
    /// As well as collider movement detections <see cref="ColliderMovementDetection"/>
    /// </summary>
    [Serializable]
    public class ThirdPersonMovementEventHandler : MovementEventHandler
    {
        /// <summary>
        /// The movement detection colliders attached to the feet of the Third Person Character
        /// </summary>
        [SerializeField]
        ColliderMovementDetection m_LeftFootDetection;

        [SerializeField]
        ColliderMovementDetection m_RightFootDetection;

        [SerializeField]
        float m_MaximumSpeed = 10f;

        ThirdPersonBrain m_ThirdPersonBrain;

        /// <summary>
        /// Gives the <see cref="ThirdPersonMovementEventHandler"/> context of the <see cref="ThirdPersonBrain"/>
        /// </summary>
        /// <param name="brainToUse">The <see cref="ThirdPersonBrain"/> that called Init</param>
        public void Init(ThirdPersonBrain brainToUse)
        {
            base.Init(brainToUse);
            m_ThirdPersonBrain = brainToUse;
        }

        /// <summary>
        /// Subscribe to the movement detection events
        /// </summary>
        public void Subscribe()
        {
            if (m_LeftFootDetection != null)
            {
                m_LeftFootDetection.detection += HandleLeftFoot;
            }

            if (m_RightFootDetection != null)
            {
                m_RightFootDetection.detection += HandleRightFoot;
            }
        }

        /// <summary>
        /// Unsubscribe to the movement detection events
        /// </summary>
        public void Unsubscribe()
        {
            if (m_LeftFootDetection != null)
            {
                m_LeftFootDetection.detection -= HandleLeftFoot;
            }

            if (m_RightFootDetection != null)
            {
                m_RightFootDetection.detection -= HandleRightFoot;
            }
        }

        /// <summary>
        /// Plays the Jumping movement events
        /// </summary>
        public void Jumped()
        {
            PlayJumping(new MovementEventData(brain.transform));
        }

        /// <summary>
        /// Plays the landing movement events
        /// </summary>
        public void Landed()
        {
            PlayLanding(new MovementEventData(brain.transform));
        }

        /// <summary>
        /// Plays the left foot movement events
        /// </summary>
        /// <param name="movementEventData">the data need to play the event</param>
        /// <param name="physicMaterial">physic material of the footstep</param>
        void HandleLeftFoot(MovementEventData movementEventData, PhysicMaterial physicMaterial)
        {
            SetPhysicMaterial(physicMaterial);
            movementEventData.normalizedSpeed = Mathf.Clamp01(m_ThirdPersonBrain.planarSpeed / m_MaximumSpeed);
            PlayLeftFoot(movementEventData);
        }

        /// <summary>
        /// Plays the right foot movement events
        /// </summary>
        /// <param name="movementEventData">the data need to play the event</param>
        /// <param name="physicMaterial">physic material of the footstep</param>
        void HandleRightFoot(MovementEventData movementEventData, PhysicMaterial physicMaterial)
        {
            SetPhysicMaterial(physicMaterial);
            movementEventData.normalizedSpeed = Mathf.Clamp01(m_ThirdPersonBrain.planarSpeed / m_MaximumSpeed);
            PlayRightFoot(movementEventData);
        }
    }
}
