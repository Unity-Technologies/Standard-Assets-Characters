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

		[FormerlySerializedAs("useSimpleCameras")]
		[SerializeField, Tooltip("Set to true if you do not want to use the Camera animation manager")]
		protected bool m_UseSimpleCameras;

		[FormerlySerializedAs("motor")]
		[SerializeField, Tooltip("Properties of the root motion motor")]
		protected ThirdPersonMotor m_Motor;

		[FormerlySerializedAs("turnaroundType")]
		[SerializeField]
		protected TurnaroundType m_TurnaroundType;

		[FormerlySerializedAs("blendspaceTurnAroundBehaviour")]
		[SerializeField]
		protected BlendspaceTurnAroundBehaviour m_BlendspaceTurnAroundBehaviour;

		[FormerlySerializedAs("animationTurnAroundBehaviour")]
		[SerializeField]
		protected AnimationTurnAroundBehaviour m_AnimationTurnAroundBehaviour;

		[FormerlySerializedAs("thirdPersonMovementEventHandler")]
		[SerializeField]
		protected ThirdPersonMovementEventHandler m_ThirdPersonMovementEventHandler;

		[FormerlySerializedAs("configuration")]
		[SerializeField, Tooltip("Configuration settings for the animator")]
		protected AnimationConfig m_Configuration;

		[FormerlySerializedAs("showDebugGizmos")]
		[SerializeField]
		protected bool m_ShowDebugGizmos;

		[FormerlySerializedAs("gizmoSettings")]
		[SerializeField]
		protected DebugGizmoSettings m_GizmoSettings;

		[Serializable]
		public struct DebugGizmoSettings
		{
			public GameObject arrowPrefab;

			public bool showBodyGizmos;

			public Color bodyCurrentDirection,
			             bodyDesiredDirection;

			public bool showFootGizmos;
			public Color inputDirection;
			public Color velocityIndicator;

			public bool showHeadGizmos;

			public Color headCurrentDirection,
			             headDesiredDirection;
		}

		readonly Vector3 m_HeadGizmoPosition = new Vector3(0f, 1.55f, 0f);
		readonly Vector3 m_HeadGizmoScale = new Vector3(0.05f, 0.05f, 0.05f);

		readonly Vector3 m_BodyGizmoPosition = new Vector3(0f, 0.8f, 0.1f);
		readonly Vector3 m_BodyGizmoScale = new Vector3(0.1f, 0.1f, 0.1f);

		readonly Vector3 m_FootGizmoPosition = new Vector3(0f, 0.05f, 0f);
		readonly Vector3 m_FootGizmoScale = new Vector3(0.1f, 0.1f, 0.1f);

		readonly Vector3 m_GizmoOffset = new Vector3(0f, 0.025f, 0f);

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

		bool justJumped;

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

		public ThirdPersonMotor thirdPersonMotor
		{
			get { return m_Motor; }
		}

		public TurnaroundType typeOfTurnaround
		{
			get { return m_TurnaroundType; }
		}

		public TurnAroundBehaviour turnAround { get; private set; }

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

		public override float normalizedForwardSpeed
		{
			get { return m_Motor.normalizedForwardSpeed; }
		}

		public override float targetYRotation { get; set; }

		public ThirdPersonCameraController thirdPersonCameraController
		{
			get { return m_CameraController; }
		}

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

		void SetupGizmos()
		{
			if (Application.isPlaying && m_GizmoObjects == null && m_ShowDebugGizmos)
			{
				m_GizmoObjects = new GameObject[6];

				m_InputDirectionGizmo = Instantiate(m_GizmoSettings.arrowPrefab, transform);
				SpriteRenderer inputSpriteRenderer = m_InputDirectionGizmo.GetComponentInChildren<SpriteRenderer>();
				inputSpriteRenderer.color = m_GizmoSettings.inputDirection;
				inputSpriteRenderer.sortingOrder = 0;
				inputSpriteRenderer.transform.localScale = m_FootGizmoScale;
				m_InputDirectionGizmo.transform.localPosition = m_FootGizmoPosition;
				m_GizmoObjects[0] = m_InputDirectionGizmo;

				m_VelocityGizmo = Instantiate(m_GizmoSettings.arrowPrefab, transform);
				SpriteRenderer velocitySpriteRenderer = m_VelocityGizmo.GetComponentInChildren<SpriteRenderer>();
				velocitySpriteRenderer.color = m_GizmoSettings.velocityIndicator;
				velocitySpriteRenderer.sortingOrder = 1;
				velocitySpriteRenderer.transform.localScale = m_FootGizmoScale;
				m_VelocityGizmo.transform.localPosition = m_FootGizmoPosition + m_GizmoOffset;
				m_GizmoObjects[1] = m_VelocityGizmo;

				m_BodyCurrentGizmo = Instantiate(m_GizmoSettings.arrowPrefab, transform);
				SpriteRenderer bodyCurrentSpriteRenderer = m_BodyCurrentGizmo.GetComponentInChildren<SpriteRenderer>();
				bodyCurrentSpriteRenderer.color = m_GizmoSettings.bodyCurrentDirection;
				bodyCurrentSpriteRenderer.sortingOrder = 10;
				bodyCurrentSpriteRenderer.transform.localScale = m_BodyGizmoScale;
				m_BodyCurrentGizmo.transform.localPosition = m_BodyGizmoPosition;
				m_GizmoObjects[2] = m_BodyCurrentGizmo;

				m_BodyDesiredGizmo = Instantiate(m_GizmoSettings.arrowPrefab, transform);
				SpriteRenderer bodyDesiredSpriteRenderer = m_BodyDesiredGizmo.GetComponentInChildren<SpriteRenderer>();
				bodyDesiredSpriteRenderer.color = m_GizmoSettings.bodyDesiredDirection;
				bodyDesiredSpriteRenderer.sortingOrder = 11;
				bodyDesiredSpriteRenderer.transform.localScale = m_BodyGizmoScale;
				m_BodyDesiredGizmo.transform.localPosition = m_BodyGizmoPosition + m_GizmoOffset;
				m_GizmoObjects[3] = m_BodyDesiredGizmo;

				m_HeadCurrentGizmo = Instantiate(m_GizmoSettings.arrowPrefab, transform);
				SpriteRenderer headCurrentSpriteRenderer = m_HeadCurrentGizmo.GetComponentInChildren<SpriteRenderer>();
				headCurrentSpriteRenderer.color = m_GizmoSettings.headCurrentDirection;
				headCurrentSpriteRenderer.sortingOrder = 20;
				headCurrentSpriteRenderer.transform.localScale = m_HeadGizmoScale;
				m_HeadCurrentGizmo.transform.localPosition = m_HeadGizmoPosition;
				m_GizmoObjects[4] = m_HeadCurrentGizmo;

				m_HeadDesiredGizmo = Instantiate(m_GizmoSettings.arrowPrefab, transform);
				SpriteRenderer headDesiredSpriteRenderer = m_HeadDesiredGizmo.GetComponentInChildren<SpriteRenderer>();
				headDesiredSpriteRenderer.color = m_GizmoSettings.headDesiredDirection;
				headDesiredSpriteRenderer.sortingOrder = 21;
				headDesiredSpriteRenderer.transform.localScale = m_HeadGizmoScale;
				m_HeadDesiredGizmo.transform.localPosition = m_HeadGizmoPosition + m_GizmoOffset;
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
				m_TurnaroundType = m_TurnaroundType == TurnaroundType.Animation ? TurnaroundType.None : m_TurnaroundType + 1;
				turnAround = GetCurrentTurnaroundBehaviour();
			}
		}

		void LateUpdate()
		{
			m_Motor.SetLookDirection();
			UpdateGizmos();
		}

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
			else if (!justJumped)
			{
				animator.SetFloat(m_HashVerticalSpeed, m_Motor.normalizedVerticalSpeed);
			}
			else
			{
				justJumped = false;
			}
		}

		/// <summary>
		/// Checks if <see cref="ThirdPersonCameraController"/> has been assigned - otherwise looks for it in the scene
		/// </summary>
		void FindCameraController(bool autoDisable)
		{
			if (m_UseSimpleCameras)
			{
				return;
			}

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

		TurnAroundBehaviour GetCurrentTurnaroundBehaviour()
		{
			switch (m_TurnaroundType)
			{
				case TurnaroundType.None:
					return null;
				case TurnaroundType.Blendspace:
					return m_BlendspaceTurnAroundBehaviour;
				case TurnaroundType.Animation:
					return m_AnimationTurnAroundBehaviour;
				default:
					return null;
			}
		}

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

		void OnAnimatorMove()
		{
			m_Motor.OnAnimatorMove();
		}

		void OnAnimatorIK(int layerIndex)
		{
			HeadTurn();
		}

		/// <summary>
		/// Handles the head turning.
		/// </summary>
		void HeadTurn()
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
		/// Gets the required components.
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
		/// Fires when the <see cref="m_Motor"/> enters a rapid turn.
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
						if (m_Motor.normalizedForwardSpeed > m_Configuration.forwardSpeedToRoll
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
		/// Updates hr animator's forward and lateral speeds. If <see cref="AnimationConfig.enableStrafeRapidDirectionChangeSmoothing"/>
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
			
			justJumped = true;
		}

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
			//TODO: remove zero index
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
			if (Mathf.Abs(m_Motor.normalizedLateralSpeed) < Mathf.Epsilon)
			{
				animator.SetBool(m_HashGroundedFootRight, value);
				isRightFootPlanted = value;
				return;
			}

			// while strafing a foot is preferred depending on lateral direction
			bool lateralSpeedRight = m_Motor.normalizedLateralSpeed < 0.0f;
			animator.SetBool(m_HashGroundedFootRight, lateralSpeedRight);
			isRightFootPlanted = lateralSpeedRight;
		}

		static bool CheckHasSpeed(float speed)
		{
			return Mathf.Abs(speed) > 0.0f;
		}

#if UNITY_EDITOR
		void Reset()
		{
			//Design pattern for fetching required scene references
			FindCameraController(false);
		}

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
		[FormerlySerializedAs("leftFootDetection")]
		[SerializeField]
		protected ColliderMovementDetection m_LeftFootDetection;

		[FormerlySerializedAs("rightFootDetection")]
		[SerializeField]
		protected ColliderMovementDetection m_RightFootDetection;

		[FormerlySerializedAs("maximumSpeed")]
		[SerializeField]
		protected float m_MaximumSpeed = 10f;

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
		void HandleLeftFoot(MovementEventData movementEventData)
		{
			movementEventData.normalizedSpeed = Mathf.Clamp01(m_ThirdPersonBrain.planarSpeed / m_MaximumSpeed);
			PlayLeftFoot(movementEventData);
		}

		/// <summary>
		/// Plays the right foot movement events
		/// </summary>
		/// <param name="movementEventData">the data need to play the event</param>
		void HandleRightFoot(MovementEventData movementEventData)
		{
			movementEventData.normalizedSpeed = Mathf.Clamp01(m_ThirdPersonBrain.planarSpeed / m_MaximumSpeed);
			PlayRightFoot(movementEventData);
		}
	}
}