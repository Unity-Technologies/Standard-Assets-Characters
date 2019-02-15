using System;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Helpers;
using StandardAssets.Characters.ThirdPerson.Configs;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(IThirdPersonInput)), RequireComponent(typeof(Animator))]
    public class ThirdPersonBrain : CharacterBrain
    {
        [SerializeField, Tooltip("Properties of the root motion motor")]
        ThirdPersonMotor m_Motor;

        [SerializeField, Tooltip("Mechanism used in rapid direction changes")]
        TurnAroundType m_TurnAroundType;

        [SerializeField, Tooltip("Transform that acts as the target for the Cinemachine SDC")]
        Transform m_VCamTarget;

        [SerializeField, Tooltip("Properties of the blendspace-based turn around state")]
        BlendspaceTurnAroundBehaviour m_BlendspaceTurnAround;

        [SerializeField, Tooltip("Properties of the animation-based turn around state")]
        AnimationTurnAroundBehaviour m_AnimationTurnAround;

        [SerializeField, Tooltip("Properties of the movement event handler")]
        ThirdPersonMovementEventHandler m_MovementEffects;

        [SerializeField, Tooltip("Configuration settings for the animator")]
        AnimationConfig m_Configuration;

        [SerializeField, Tooltip("Show the debug gizmos?")]
        bool m_ShowDebugGizmos;

        [SerializeField, Tooltip("Properties of the debug gizmos")]
        DebugGizmoSettings m_GizmoSettings;


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


        /// <summary>
        /// Struct for containing DebugGizmo settings.
        /// </summary>
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
            public GameObject arrowPrefab{ get { return m_ArrowPrefab; } }

            /// <summary>
            /// Gets the show body gizmos bool
            /// </summary>
            public bool showBodyGizmos { get { return m_ShowBodyGizmos; } }

            /// <summary>
            /// Gets the body current direction color
            /// </summary>
            public Color bodyCurrentDirection { get { return m_BodyCurrentDirection; } }

            /// <summary>
            /// Gets the body desired direction color
            /// </summary>
            public Color bodyDesiredDirection { get { return m_BodyDesiredDirection; } }

            /// <summary>
            /// Gets the show foot gizmos bool
            /// </summary>
            public bool showFootGizmos { get { return m_ShowFootGizmos; } }

            /// <summary>
            /// Gets the input direction color
            /// </summary>
            public Color inputDirection { get { return m_InputDirection; } }

            /// <summary>
            /// Gets the velocity indicator color
            /// </summary>
            public Color velocityIndicator { get { return m_VelocityIndicator; } }

            /// <summary>
            /// Gets the show head gizmos bool
            /// </summary>
            public bool showHeadGizmos { get { return m_ShowHeadGizmos; } }

            /// <summary>
            /// Gets the head current direction color
            /// </summary>
            public Color headCurrentDirection { get { return m_HeadCurrentDirection; } }

            /// <summary>
            /// Gets the head desired direction color
            /// </summary>
            public Color headDesiredDirection { get { return m_HeadDesiredDirection; } }
        }

        // value used when in a turn around state and a rapid head turn is required.
        const float k_HeadTurnSnapBackScale = 100f;

        // scale used on velocity gizmo
        const float k_VelocityGizmoScale = 0.2f;
        
        // value used to lerp velocity gizmo scale
        const float k_VelocityLerp = 3f;
        
        //Gizmo readonly vectors
        readonly Vector3 m_HeadGizmoPosition = new Vector3(0f, 1.55f, 0f);
        readonly Vector3 m_HeadGizmoScale = new Vector3(0.05f, 0.05f, 0.05f);
        readonly Vector3 m_BodyGizmoPosition = new Vector3(0f, 0.8f, 0.1f);
        readonly Vector3 m_BodyGizmoScale = new Vector3(0.1f, 0.1f, 0.1f);
        readonly Vector3 m_FootGizmoPosition = new Vector3(0f, 0.05f, 0f);
        readonly Vector3 m_FootGizmoScale = new Vector3(0.1f, 0.1f, 0.1f);
        readonly Vector3 m_GizmoOffset = new Vector3(0f, 0.025f, 0f);

        /// <summary>
        /// Event that can be subscribed to for when there is a camera change
        /// </summary>
        public event Action onCameraChange;


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

        // If a strafe has been queued.
        bool m_IsTryingStrafe;

        // If a jump was preformed this frame.
        bool m_JustJumped;

        // Time in seconds since the current grounded foot swapped.
        float m_TimeSinceGroundedFootChange;

        //the camera controller to be used
        bool m_IsChangingCamera;

        // If a rapid direction change was triggered in strafe mode.
        bool m_TriggeredRapidDirectionChange;

        // Reference to the input.
        IThirdPersonInput m_Input;

        // Objects used for drawing helpful gizmos.
        GameObject[] m_GizmoObjects;

        // GameObjects used to draw gizmos.
        GameObject m_BodyCurrentGizmo;
        GameObject m_BodyDesiredGizmo;
        GameObject m_InputDirectionGizmo;
        GameObject m_VelocityGizmo;
        GameObject m_HeadCurrentGizmo;
        GameObject m_HeadDesiredGizmo;

        // Cached reference to the main camera's transform,
        Transform m_MainCameraTransform;

        // Array of the different turn around behaviours.
        TurnAroundBehaviour[] m_TurnAroundBehaviours;

        // The current count and duration of a strafe rapid direction change.
        float m_RapidStrafeTime, m_RapidStrafeChangeDuration;

        /// <summary>
        /// Gets the <see cref="ThirdPersonMotor"/>'s normalized forward speed
        /// </summary>
        public override float normalizedForwardSpeed { get { return m_Motor.normalizedForwardSpeed; } }

        /// <summary>
        /// Gets/sets the character target Y rotation
        /// </summary>
        public override float targetYRotation { get; set; }

        /// <summary>
        /// Gets/sets whether the locomotion mode is set to Strafe
        /// </summary>
        public bool IsStrafing { get; private set; }

        /// <summary>
        /// Gets the <see cref="ThirdPersonMotor"/>
        /// </summary>
        public ThirdPersonMotor thirdPersonMotor { get { return m_Motor; } }

        /// <summary>
        /// Gets the VCam Target Transform/>
        /// </summary>
        public Transform vcamTarget { get { return m_VCamTarget; } }

        /// <summary>
        /// Gets the <see cref="TurnAroundType"/>
        /// </summary>
        public TurnAroundType typeOfTurnAround { get { return m_TurnAroundType; } }

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
                        m_BlendspaceTurnAround,
                        m_AnimationTurnAround
                    };
                }

                return m_TurnAroundBehaviours;
            }
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
        public float animatorForwardSpeed { get { return animator.GetFloat(m_HashForwardSpeed); } }

        /// <summary>
        /// Gets the animator turning speed.
        /// </summary>
        /// <value>The animator forward speed parameter.</value>
        public float animatorTurningSpeed { get { return animator.GetFloat(m_HashTurningSpeed); } }

        /// <summary>
        /// Gets whether the right foot is planted
        /// </summary>
        /// <value>True is the right foot is planted; false if the left.</value>
        public bool isRightFootPlanted { get; private set; }

        /// <summary>
        /// Gets whether the character in a grounded state.
        /// </summary>
        /// <value>True if the state is in a grounded state; false if aerial.</value>
        public bool isGroundedState
        {
            get
            {
                return animatorState == AnimatorState.Locomotion ||
                    animatorState == AnimatorState.Landing;
            }
        }

        // The value of the animator lateral speed parameter.
        float animatorLateralSpeed { get { return animator.GetFloat(m_HashLateralSpeed); } }


        /// <summary>
        /// Initializes <see cref="TurnAroundBehaviour"/>s, <see cref="ThirdPersonMotor"/>, <see cref="ThirdPersonMovementEventHandler"/>, cameras, gizmos and the animator.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            m_BlendspaceTurnAround.Init(this);
            m_AnimationTurnAround.Init(this);
            turnAround = GetCurrentTurnaroundBehaviour();
            m_Motor.Init(this);

            InitAnimator();
            m_MainCameraTransform = Camera.main.transform;

            m_MovementEffects.Init(this);
            SetupGizmos();
        }

#if UNITY_EDITOR
        // On change of component
        void OnValidate()
        {
            turnAround = GetCurrentTurnaroundBehaviour();
            SetupGizmos();
        }
#endif

        // Subscribes to various events
	    protected override void OnEnable()
	    {
		    base.OnEnable();
            controllerAdapter.jumpVelocitySet += m_MovementEffects.Jumped;
            controllerAdapter.landed += m_MovementEffects.Landed;
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
                    userInput.sprintStarted += m_Motor.ToggleSprint;
                    userInput.sprintEnded += m_Motor.StopSprint;
                    userInput.strafeStarted += OnStrafeStarted;
                    userInput.strafeEnded += OnStrafeEnded;
                }
            }

            m_MovementEffects.Subscribe();
        }

        // Unsubscribes from various events
	    protected override void OnDisable()
	    {
		    base.OnDisable();
            if (controllerAdapter != null)
            {
                controllerAdapter.jumpVelocitySet -= m_MovementEffects.Jumped;
                controllerAdapter.landed -= m_MovementEffects.Landed;
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
                        userInput.sprintStarted -= m_Motor.ToggleSprint;
                        userInput.sprintEnded -= m_Motor.StopSprint;
                        userInput.strafeStarted -= OnStrafeStarted;
                        userInput.strafeEnded -= OnStrafeEnded;
                    }
                }

                m_Motor.jumpStarted -= OnJumpStarted;
                m_Motor.fallStarted -= OnFallStarted;
                m_Motor.Unsubscribe();
            }

            m_MovementEffects.Unsubscribe();
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
        }

        // Calls the motor's move if not using root motion.
        void FixedUpdate()
        {
            if (!m_Motor.useRootMotion)
            {
                m_Motor.OnMove();           
            }
        }

        // Updates the motor look direction and the gizmos
        void LateUpdate()
        {
            m_Motor.SetLookDirection();
            UpdateGizmos();
        }        

        // Handles motor movement
        void OnAnimatorMove()
        {
            if (m_Motor.useRootMotion)
            {
                m_Motor.OnMove();
            }
        }

        // Handles head turn
        //      layerIndex: Animation layer index as required by the Unity message
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
        public void OnLocomotionAnimationEnter(GroundMovementConfig movementConfig)
        {
            if (animatorState == AnimatorState.Falling)
            {
                animatorState = AnimatorState.Locomotion;
            }
            else if (animatorState == AnimatorState.Jump)
            {
                animatorState = AnimatorState.JumpLanding;
            }
            m_Motor.SetMovementConfig(movementConfig);
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

        // Sets up the gizmos when they are enabled (either in Awake or OnValidate)
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

        // Helper function for initializing a gizmo GameObject
        //      gizmoColor: Color of the gizmo
        //      sortingOrder: Sprite sorting order
        //      localScale: Size of the gizmo
        //      position: Position of the gizmo
        //      return: Set up gizmo GameObject
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

        // Updates the gizmos
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

        // Sets the Animator parameters.
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

        // Gets the current turn around
        //      return: Current TurnAroundBehaviour
        TurnAroundBehaviour GetCurrentTurnaroundBehaviour()
        {
            switch (m_TurnAroundType)
            {
                case TurnAroundType.None:
                    return null;
                case TurnAroundType.Blendspace:
                    return m_BlendspaceTurnAround;
                case TurnAroundType.Animation:
                    return m_AnimationTurnAround;
                default:
                    return null;
            }
        }

        // Sets up the animator hashes and gets the required components.
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

        // Fires when the 'm_Motor' enters the fall state
        void OnFallStarted(float predictedFallDistance)
        {
            m_IsGrounded = false;
            animator.SetTrigger(m_HashFall);
        }

        // Logic for dealing with animation on landing. Fires when the 'controllerAdapter' enters a land
        void OnLanding()
        {
            m_IsGrounded = true;
            animator.ResetTrigger(m_HashFall);
            
            switch (animatorState)
            {
                // if coming from a physics jump handle animation transition
                case AnimatorState.Jump:
                    bool rightFoot = animator.GetBool(m_HashGroundedFootRight);
                    float duration = m_Configuration.jumpEndTransitionAsAFactorOfSpeed.Evaluate(
                        Mathf.Abs(animator.GetFloat(AnimationControllerInfo.k_ForwardSpeedParameter)));
                    string locomotion = IsStrafing
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
                    if (IsStrafing)
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

        // Updates animator's forward and lateral speeds. If 'AnimationConfig.enableStrafeRapidDirectionChangeSmoothing'
        // is enabled special smoothing logic is performed when a strafe rapid direction change is detected.
        void UpdateAnimationMovementSpeeds(float deltaTime)
        {
            // if in strafe move and moving enough perform strafe rapid direction change logic
            if (m_Configuration.enableStrafeRapidDirectionChangeSmoothing &&
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

        // Checks if the strafe rapid direction change (e.g. left to right and vice versa) is complete
        //      deltaTime: Time difference per Update frame
        //      return: true if complete, false otherwise
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

        // Enter a state that attempts to enter the strafe state. Fired by an input event.
        void OnStrafeStarted()
        {
            m_IsChangingCamera = true;
            m_IsTryingStrafe = true;
            if (controllerAdapter.isGrounded)
            {
                SelectGroundedCamera();
            }
        }

        // Enter a state that attempts to end the strafe state. Fired by an input event.
        void OnStrafeEnded()
        {
            m_IsChangingCamera = true;
            m_IsTryingStrafe = false;
            if (controllerAdapter.isGrounded)
            {
                SelectGroundedCamera();
            }
        }

        // Helper for selecting correct camera on grounding
        void SelectGroundedCamera()
        {
            if (!m_IsChangingCamera)
            {
                return;
            }

            IsStrafing = m_IsTryingStrafe;
            animator.SetBool(m_HashStrafe, IsStrafing);
            if (IsStrafing)
            {
                m_Motor.StartStrafe();
            }
            else
            {
                m_Motor.EndStrafe();
            }
            
            // Call any registered events for when the camera has changed
            if(onCameraChange != null)
            {
                onCameraChange();
            }

            m_IsChangingCamera = false;
        }

        // Logic for dealing with animation on jumping. Fires when the 'm_Motor' enters a jump.
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

            var timeSincePlantedFootChange = 
                m_Configuration.footPositionJumpIncRemap.Evaluate(m_TimeSinceGroundedFootChange * 2.0f);
            var extraDuration = timeSincePlantedFootChange * m_Configuration.jumpBlendTimeInc;
            
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

            animator.CrossFade(jumpState, duration + extraDuration);
            m_LastJumpWasRightRoot = rightFoot;

            m_JustJumped = true;
        }

        // Checks if the normalized time is close to zero or half
        //      margin: Margin for deciding whether the time is close
        //      timeUntilZeroOrHalf: how long until zero or half
        //      return: true if close
        bool IsNormalizedTimeCloseToZeroOrHalf(float margin, out float timeUntilZeroOrHalf)
        {
            float value = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f;

            timeUntilZeroOrHalf = (value >= 0.5 ? 1.0f : 0.5f) - value;

            return (value > 1.0f - margin || value < margin ||
                (value > 0.5f - margin && value < 0.5f + margin));
        }

        // Uses the normalized progress of the animation to determine the grounded foot.
        void UpdateFoot()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            var animationNormalizedProgress = stateInfo.normalizedTime.GetFraction();

            if ((animationNormalizedProgress + m_Configuration.groundedFootThresholdOffsetValue).Wrap1() >
                (m_Configuration.groundedFootThresholdValue + m_Configuration.groundedFootThresholdOffsetValue).Wrap1())
            {
                m_TimeSinceGroundedFootChange = animationNormalizedProgress - m_Configuration.groundedFootThresholdOffsetValue;
                SetGroundedFootRight(!m_Configuration.invertFoot);
                return;
            }
            m_TimeSinceGroundedFootChange = (animationNormalizedProgress + m_Configuration.groundedFootThresholdValue) 
                % 1.0f;
            SetGroundedFootRight(m_Configuration.invertFoot);
        }

        // Sets the grounded foot of the animator. This is used to play the appropriate footed animations.
        void SetGroundedFootRight(bool value)
        {
            animator.SetBool(m_HashGroundedFootRight, value);
            isRightFootPlanted = value;
        }

        // Checks if character is moving
        static bool CheckHasSpeed(float speed)
        {
            return Mathf.Abs(speed) > 0.0f;
        }
	    
	    /// <summary>
	    /// Can moving platforms rotate the current camera?
	    /// </summary>
	    public override bool MovingPlatformCanRotateCamera()
	    {
		    return IsStrafing;
	    }
    }


    /// <summary>
    /// Handles the third person movement event triggers and event IDs.
    /// As well as collider movement detections <see cref="ColliderMovementDetection"/>
    /// </summary>
    [Serializable]
    public class ThirdPersonMovementEventHandler : MovementEventHandler
    {
        [SerializeField, Tooltip("Reference to the left foot collider movement detection")]
        ColliderMovementDetection m_LeftFootDetection;

        [SerializeField, Tooltip("Reference to the right foot collider movement detection")]
        ColliderMovementDetection m_RightFootDetection;

        [SerializeField, Tooltip("The maximum speed used to normalized planar speed")]
        float m_MaximumSpeed = 10f;

        // Cached reference to the ThirdPersonBrain
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

        // Plays the left foot movement events
        //      movementEventData: the data need to play the event
        //      physicMaterial: physic material of the footstep
        void HandleLeftFoot(MovementEventData movementEventData, PhysicMaterial physicMaterial)
        {
            SetPhysicMaterial(physicMaterial);
            movementEventData.normalizedSpeed = Mathf.Clamp01(m_ThirdPersonBrain.planarSpeed / m_MaximumSpeed);
            PlayLeftFoot(movementEventData);
        }

        // Plays the right foot movement events
        //      movementEventData: the data need to play the event
        //      physicMaterial: physic material of the footstep
        void HandleRightFoot(MovementEventData movementEventData, PhysicMaterial physicMaterial)
        {
            SetPhysicMaterial(physicMaterial);
            movementEventData.normalizedSpeed = Mathf.Clamp01(m_ThirdPersonBrain.planarSpeed / m_MaximumSpeed);
            PlayRightFoot(movementEventData);
        }
    }
}
