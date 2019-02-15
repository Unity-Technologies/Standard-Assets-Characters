using System;
using System.Collections.Generic;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Helpers;
using StandardAssets.Characters.ThirdPerson.Configs;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// IThirdPersonMotor implementation that uses, primarily, root motion from the animator to move the character.
	/// </summary>
	[Serializable]
	public class ThirdPersonMotor
	{
		[SerializeField, Tooltip("Configuration with all the movement settings")]
		MotorConfig m_Configuration;
		
#if UNITY_EDITOR
		public MotorConfig configuration
		{
			get { return m_Configuration; }
		}
#endif

		/// <summary>
		/// Fired on jump.
		/// </summary>
		public event Action jumpStarted;

		/// <summary>
		/// Fired when the character starts falling.
		/// </summary>
		public event Action<float> fallStarted;

		// Track distance above the ground at these frame intervals (to prevent checking every frame)
		const int k_TrackGroundFrameIntervals = 5;

		// List of objects that disabled rapid turn. To allow multiple objects to disable it temporarily.
		readonly List<object> m_ObjectsThatDisabledRapidTurn = new List<object>();

		// The input implementation
		IThirdPersonInput m_CharacterInput;

		// The controller controllerAdapter implementation
		ControllerAdapter m_ControllerAdapter;

		// The movement state as a turn around was triggered.
		ThirdPersonGroundMovementState m_PreTurnMovementState;

		// The current ground movement state.
		ThirdPersonGroundMovementState m_MovementState = ThirdPersonGroundMovementState.Walking;

		// The current aerial state.
		ThirdPersonAerialMovementState m_AerialState = ThirdPersonAerialMovementState.Grounded;

		// Sliding average used to smooth forward velocity so jump velocity is more consistent.
		SlidingAverage m_AverageForwardVelocity;

		// Sliding average used to smooth input during exploration.
		SlidingAverage m_ExplorationAverageForwardInput;

		// Sliding average used to smooth forward input during strafe.
		SlidingAverage m_StrafeAverageForwardInput;

		// Sliding average used to smooth lateral input during strafe.
		SlidingAverage m_StrafeAverageLateralInput;

		// Time the character has been in a turn around state. Used to ignore forward input for a moment as a turn 
		// around starts.
		float m_TurnaroundMovementTime;

		// The time of the character's last idle (no input).
		float m_LastIdleTime;

		// Has a jump been queued?
		bool m_JumpQueued;

		// Reference to the Unity Animator.
		Animator m_Animator;

		// The current fall direction.
		Vector3 m_FallDirection;

		// Reference to the ThirdPersonBrain's transform.
		Transform m_Transform;

		// Reference to the ThirdPersonBrain's GameObject.
		GameObject m_GameObject;

		// Reference to the ThirdPersonBrain.
		ThirdPersonBrain m_ThirdPersonBrain;

		// A sized queue of previous inputs used to determine if a rapid turn has been triggered.
		SizedQueue<Vector2> m_PreviousInputs;

		// Reference to the main camera.
		Camera m_MainCamera;

		// Current ground speed for non root motion ground movement.
		float m_GroundSpeed;
		
		// on start strafe control initial look
		bool m_IsInitialStrafeLook;
		float m_InitialStrafeLookCount;
		Quaternion m_RotationOnStrafeStart;

		// Gets whether to track height above the ground.
		bool m_TrackGroundHeight;

		// Current GroundMovementConfig to use for non root motion ground movement.
		GroundMovementConfig m_CurrentGroundMovementConfig;

		/// <summary>
		/// Gets whether <see cref="m_CurrentGroundMovementConfig"/> is set to use root motion.
		/// </summary>
		public bool useRootMotion
		{
			get { return m_CurrentGroundMovementConfig.useRootMotion; }
		}

		/// <summary>
		/// Gets the normalized turning speed
		/// </summary>
		/// <value>A value normalized using turning settings from <see cref="m_Configuration"/> </value>
		public float normalizedTurningSpeed { get; private set; }
		
		/// <summary>
		/// Gets the normalized lateral speed
		/// </summary>
		/// <value>A value normalized using lateral settings from <see cref="m_Configuration"/> </value>
		public float normalizedLateralSpeed { get; private set; }
		
		/// <summary>
		/// Gets the normalized forward speed
		/// </summary>
		/// <value>A value normalized using forward settings from <see cref="m_Configuration"/> </value>
		public float normalizedForwardSpeed { get; private set; }

		/// <summary>
		/// Gets the character's current fall time.
		/// </summary>
		/// <value>The time, in seconds, the character has been in a falling state.</value>
		public float fallTime { get { return m_ControllerAdapter.fallTime; } }

		/// <summary>
		/// Gets the desired target y rotation.
		/// </summary>
		/// <value>The target y rotation of the character, in degrees.</value>
		public float targetYRotation { get; private set; }

		/// <summary>
		/// Gets the velocity that was cached as the character exited a root motion state.
		/// </summary>
		/// <value>An moving average of the root motion velocity.</value>
		public float cachedForwardVelocity { get; private set; }
		
		/// <summary>
		/// Gets the character's current movement mode.
		/// </summary>
		/// <value>Either Exploration or Strafe.</value>
		public ThirdPersonMotorMovementMode movementMode { get; private set; }

		/// <summary>
		/// Gets the current <see cref="TurnAroundBehaviour"/>.
		/// </summary>
		public TurnAroundBehaviour currentTurnAroundBehaviour { get { return m_ThirdPersonBrain.turnAround; } }
	
		/// <summary>
		/// Gets the vertical speed.
		/// </summary>
		/// <value>Range = -1 (falling) to 1 (jumping).</value>
		/// <remarks>Returns <see cref="ControllerAdapter"/>'s <see cref="ControllerAdapter.normalizedVerticalSpeed"/>.</remarks>
		public float normalizedVerticalSpeed { get { return m_ControllerAdapter.normalizedVerticalSpeed; } }
		
		/// <summary>
		/// Gets whether the character is in a sprint state.
		/// </summary>
		/// <value>True if in a sprint state; false otherwise.</value>
		public bool sprint { get; private set; }
		
		/// <summary>
		/// Gets the current <see cref="ThirdPersonGroundMovementState"/>.
		/// </summary>
		public ThirdPersonGroundMovementState currentGroundMovementState { get { return m_MovementState; } }
		
		/// <summary>
		/// Gets the current <see cref="ThirdPersonAerialMovementState"/>.
		/// </summary>
		public ThirdPersonAerialMovementState currentAerialMovementState { get { return m_AerialState; } }

		/// <summary>
		/// Whether the character is grounded
		/// </summary>
		/// <value>True if <see cref="m_AerialState"/> is grounded</value>
		bool isGrounded { get { return m_AerialState == ThirdPersonAerialMovementState.Grounded; } }	

		// Is rapid turn disabled? (Enable/Disable it via EnableRapidTurn / DisableRapidTurn).
		bool disableRapidTurn { get { return m_ObjectsThatDisabledRapidTurn.Count > 0; } }

		/// <summary>
		/// Sets the current <see cref="GroundMovementConfig"/> to be used.
		/// </summary>
		/// <param name="config">Config to use.</param>
		public void SetMovementConfig(GroundMovementConfig config)
		{
			m_CurrentGroundMovementConfig = !m_Configuration.alwaysUseDefaultConfig && config != null
				? config : m_Configuration.defaultGroundMovementConfig;
		}
		
		/// <summary>
		/// Moves the character based on movement and animator state.
		/// </summary>
		/// <remarks>Called by the Animator if root motion is enabled otherwise by <see cref="ThirdPersonBrain"/>'s Update.</remarks>
		public void OnMove()
		{
			if (m_MovementState == ThirdPersonGroundMovementState.TurningAround)
			{
				m_ControllerAdapter.Move(m_ThirdPersonBrain.turnAround.GetMovement(), Time.deltaTime);
				return;
			}

			if (m_ThirdPersonBrain.isGroundedState)
			{
				Vector3 groundMovementVector = GetMovementVector();
				groundMovementVector.y = 0.0f;
				
				m_ControllerAdapter.Move(groundMovementVector, Time.deltaTime);
				
				//Update the average movement speed
				var direction = movementMode == ThirdPersonMotorMovementMode.Exploration
					? m_Transform.forward
					: CalculateLocalInputDirection();              
				float movementVelocity = groundMovementVector.GetMagnitudeOnAxis(direction)/Time.deltaTime;
				if (movementVelocity > 0)
				{
					m_AverageForwardVelocity.Add(movementVelocity, HandleNegative.Absolute);
				}

				if (!m_CharacterInput.hasMovementInput)
				{
					m_LastIdleTime = Time.time;
				}
			}
			else //aerial
			{
				if (normalizedVerticalSpeed <= 0 || m_AerialState != ThirdPersonAerialMovementState.Grounded)
				{
					UpdateFallForwardSpeed();
				}

				var movementDirection = movementMode == ThirdPersonMotorMovementMode.Exploration ? m_Transform.forward :
					CalculateLocalInputDirection() ;
				m_FallDirection = Vector3.Lerp(m_FallDirection, movementDirection, m_Configuration.fallDirectionChange);
				m_ControllerAdapter.Move(cachedForwardVelocity * Time.deltaTime * m_FallDirection, Time.deltaTime);
			}
		}

		/// <summary>
		/// Initializes the motor
		/// </summary>
		/// <param name="brain"><see cref="ThirdPersonBrain"/> calling the initialization</param>
		public void Init(ThirdPersonBrain brain)
		{
			m_MainCamera = Camera.main;
			m_GameObject = brain.gameObject;
			m_Transform = brain.transform;
			m_ThirdPersonBrain = brain;
			m_CharacterInput = brain.thirdPersonInput;
			m_ControllerAdapter = brain.controllerAdapter;
			m_Animator = m_GameObject.GetComponent<Animator>();
			m_AverageForwardVelocity = new SlidingAverage(m_Configuration.jumpGroundVelocityWindowSize);
			m_ExplorationAverageForwardInput = new SlidingAverage(m_Configuration.forwardInputWindowSize);
			m_StrafeAverageForwardInput = new SlidingAverage(m_Configuration.strafeInputWindowSize);
			m_StrafeAverageLateralInput = new SlidingAverage(m_Configuration.strafeInputWindowSize);
			m_PreviousInputs = new SizedQueue<Vector2>(m_Configuration.bufferSizeInput);
			movementMode = ThirdPersonMotorMovementMode.Exploration;
			m_CurrentGroundMovementConfig = m_Configuration.defaultGroundMovementConfig;
			
			EndStrafe();
		}

		/// <summary>
		/// Subscribe to physics, camera and input events
		/// </summary>
		public void Subscribe()
		{
			m_ControllerAdapter.landed += OnLanding;
			m_ControllerAdapter.startedFalling += OnStartedFalling;
			
			//Turnaround subscription for runtime support
			foreach (TurnAroundBehaviour turnaroundBehaviour in m_ThirdPersonBrain.turnAroundOptions)
			{
				turnaroundBehaviour.turnaroundComplete += TurnaroundComplete;
			}
		}

		/// <summary>
		/// Toggles sprint
		/// </summary>
		public void ToggleSprint()
		{
			sprint = !sprint;
		}
		
		/// <summary>
		/// Ends sprint
		/// </summary>
		public void StopSprint()
		{
			sprint = false;
		}

		/// <summary>
		/// Unsubscribe from events
		/// </summary>
		public void Unsubscribe()
		{
			//Physics subscriptions
			if (m_ControllerAdapter != null)
			{
				m_ControllerAdapter.landed -= OnLanding;
				m_ControllerAdapter.startedFalling -= OnStartedFalling;
			}

			//Turnaround un-subscription for runtime support
			foreach (TurnAroundBehaviour turnaroundBehaviour in m_ThirdPersonBrain.turnAroundOptions)
			{
				turnaroundBehaviour.turnaroundComplete -= TurnaroundComplete;
			}
		}

		/// <summary>
		/// Performs movement logic
		/// </summary>
		public void Update()
		{
			if (m_Configuration.autoToggleSprint && sprint && !m_CharacterInput.hasMovementInput)
			{
				StopSprint();
			}
			
			if (m_MovementState == ThirdPersonGroundMovementState.TurningAround)
			{
				CalculateForwardMovement();
			}
			else
			{
				switch (movementMode)
				{
					case ThirdPersonMotorMovementMode.Exploration:
						CalculateForwardMovement();
						break;
					case ThirdPersonMotorMovementMode.Strafe:
						CalculateStrafeMovement();
						break;
				}
			}
			
			m_PreviousInputs.Add(m_CharacterInput.moveInput);
			
			if (m_JumpQueued)
			{
				TryJump(out m_JumpQueued);
			}
			if (m_TrackGroundHeight)
			{
				UpdateTrackGroundHeight();
			}
		}

		/// <summary>
		/// Sets the character's target direction by checking the <see cref="ThirdPersonMotorMovementMode"/> and passing the work on to helper functions
		/// </summary>
		public void SetLookDirection()
		{
			if (m_MovementState == ThirdPersonGroundMovementState.TurningAround)
			{
				return;
			}
			switch (movementMode)
			{
				case ThirdPersonMotorMovementMode.Exploration:
					SetExplorationLookDirection();
					break;
				case ThirdPersonMotorMovementMode.Strafe:
					SetStrafeLookDirection();
					break;
			}

			if (Mathf.Approximately(m_CharacterInput.lookInput.magnitude, 0.0f))
			{
				normalizedTurningSpeed = Mathf.Lerp(normalizedTurningSpeed, 0.0f, 
				                                    Time.deltaTime * m_Configuration.noLookInputTurnSpeedDeceleration);
			}
		}

		/// <summary>
		/// Enable rapid turn. Usually used after it has been temporarily disabled.
		/// </summary>
		/// <param name="disabledByObject">The object that disabled it previously via DisableRapidTurn.</param>
		public void EnableRapidTurn(object disabledByObject)
		{
			if (m_ObjectsThatDisabledRapidTurn.Contains(disabledByObject))
			{
				m_PreviousInputs.Clear();
				m_ObjectsThatDisabledRapidTurn.Remove(disabledByObject);
			}
		}

		/// <summary>
		/// Disable rapid turn. Usually used to disable it temporarily.
		/// </summary>
		/// <param name="disabledByObject">The object that called this method. Use the same object when calling
		/// EnableRapidTurn. This helps identify various objects that temporarily disables rapid turn.</param>
		public void DisableRapidTurn(object disabledByObject)
		{
			if (!m_ObjectsThatDisabledRapidTurn.Contains(disabledByObject))
			{
				m_ObjectsThatDisabledRapidTurn.Add(disabledByObject);
			}
		}

		/// <summary>
		/// Queues a jump.
		/// </summary>
		public void OnJumpPressed()
		{
			TryJump(out m_JumpQueued);
		}

		/// <summary>
		/// Changes movement mode to <see cref="ThirdPersonMotorMovementMode.Strafe"/>
		/// </summary>
		public void StartStrafe()
		{
			if (movementMode == ThirdPersonMotorMovementMode.Strafe)
			{
				return;
			}
			
			movementMode = ThirdPersonMotorMovementMode.Strafe;
			m_IsInitialStrafeLook = true;
			m_InitialStrafeLookCount = m_Configuration.turnForwardOnStartStrafeDuration;
			m_RotationOnStrafeStart = m_Transform.rotation;
		}
		
		/// <summary>
		/// Changes movement mode to <see cref="ThirdPersonMotorMovementMode.Exploration"/>
		/// </summary>
		public void EndStrafe()
		{
			movementMode = ThirdPersonMotorMovementMode.Exploration;
		}
		
		// Calculates and returns the movement vector of the character.
		Vector3 GetMovementVector()
		{
			if (useRootMotion)
			{
				return m_Animator.deltaPosition * m_CurrentGroundMovementConfig.rootMotionScale;
			}

			var inputMagnitude = m_CharacterInput.moveInput.magnitude;
			var maxSpeed = m_CurrentGroundMovementConfig.maxSpeed.Evaluate(inputMagnitude);
			if (sprint)
			{
				maxSpeed *= m_CurrentGroundMovementConfig.sprintScale;
			}
			if (Mathf.Approximately(m_ExplorationAverageForwardInput.average, 0.0f) &&
				Mathf.Approximately(m_StrafeAverageForwardInput.average, 0.0f) &&
				Mathf.Approximately(m_StrafeAverageLateralInput.average, 0.0f))
			{
				m_GroundSpeed = 0.0f;
			}
			else
			{
				m_GroundSpeed = Mathf.Lerp(m_GroundSpeed, maxSpeed, m_CurrentGroundMovementConfig.speedDelta);
			}
			return CalculateLocalInputDirection() * inputMagnitude * m_GroundSpeed * Time.deltaTime;
		}


		// Track height above ground when the physics character is in the air, but the animation has not yet changed to
		// the fall animation.
		void UpdateTrackGroundHeight()
		{
			if (m_AerialState == ThirdPersonAerialMovementState.Grounded && !m_ControllerAdapter.isGrounded)
			{
				if (Time.frameCount % k_TrackGroundFrameIntervals == 0)
				{
					float distance;
					if (!m_ControllerAdapter.IsPredictedFallShort(out distance))
					{
						OnStartedFalling(distance);
					}
				}
			}
			else
			{
				m_TrackGroundHeight = false;
			}
		}

		// Sets the aerial state to ThirdPersonAerialMovementState.Grounded and clears 'm_AverageForwardVelocity' if no input.
		void OnLanding()
		{
			m_AerialState = ThirdPersonAerialMovementState.Grounded;

			if (!m_CharacterInput.hasMovementInput)
			{
				m_AverageForwardVelocity.Clear();
			}
			m_PreviousInputs.Clear();
		}

		// Sets the aerial state to ThirdPersonAerialMovementState.Falling and fires the event.
		// 		NOTE: This subscribes to ControllerAdapter.startedFalling
		void OnStartedFalling(float predictedFallDistance)
		{
			// check if far enough from ground to enter fall state
			if (m_ControllerAdapter.IsPredictedFallShort())
			{
				m_TrackGroundHeight = true;
				return;
			}
			m_TrackGroundHeight = false;
			
			if (m_AerialState == ThirdPersonAerialMovementState.Grounded)
			{
				cachedForwardVelocity = m_AverageForwardVelocity.average;
			}
			
			m_ControllerAdapter.SetFallVelocity();
			
			m_AerialState = ThirdPersonAerialMovementState.Falling;
			m_FallDirection = CalculateLocalInputDirection();
			
			if (fallStarted != null)
			{
				fallStarted(predictedFallDistance);
			}
		}

		// Sets the character's look direction during strafe
		void SetStrafeLookDirection()
		{
			Quaternion targetRotation = CalculateTargetRotation(Vector3.forward);
			targetYRotation = targetRotation.eulerAngles.y;
			Quaternion newRotation;

			if (m_IsInitialStrafeLook)
			{
				newRotation = Quaternion.Lerp(m_RotationOnStrafeStart, targetRotation, 
					1.0f - m_InitialStrafeLookCount / m_Configuration.turnForwardOnStartStrafeDuration);
				m_InitialStrafeLookCount -= Time.deltaTime;
				if (m_InitialStrafeLookCount <= 0.0f)
				{
					m_IsInitialStrafeLook = false;
				}
			}
			else
			{
				newRotation = Quaternion.Slerp(m_Transform.rotation, targetRotation,
								m_Configuration.turningYSpeed * Time.deltaTime);
			}
			
			SetTurningSpeed(m_Transform.rotation, newRotation);
			m_Transform.rotation = newRotation;
		}

		// Sets the character's look direction during exploration
		void SetExplorationLookDirection()
		{
			if (!m_CharacterInput.hasMovementInput)
			{
				normalizedTurningSpeed = 0;
				targetYRotation = m_Transform.eulerAngles.y;
				return;
			}

			Quaternion targetRotation = CalculateTargetRotation(
				new Vector3(m_CharacterInput.moveInput.x, 0.0f, m_CharacterInput.moveInput.y));
			targetYRotation = targetRotation.eulerAngles.y;

			if (isGrounded && CheckForAndHandleRapidTurn(targetRotation))
			{
				return;
			}

			float turnSpeed = isGrounded
				? m_Configuration.turningYSpeed
				: m_Configuration.jumpTurningYSpeed;

			Quaternion newRotation = Quaternion.Slerp(m_Transform.rotation, targetRotation, 
			                                                  turnSpeed * Time.deltaTime);

			SetTurningSpeed(m_Transform.rotation, newRotation);

			m_Transform.rotation = newRotation;
		}

		// Calculates the character's forward normalized speed for exploration
		void CalculateForwardMovement()
		{
			if (m_MovementState == ThirdPersonGroundMovementState.TurningAround &&
			    m_TurnaroundMovementTime < m_Configuration.ignoreInputTimeRapidTurn)
			{
				m_TurnaroundMovementTime += Time.deltaTime;
				return; 
			}
			
			normalizedLateralSpeed = 0;

			var inputVector = m_CharacterInput.moveInput;
			if (inputVector.magnitude > 1)
			{
				inputVector.Normalize();
			}

			var input = inputVector.magnitude;
			if (sprint && m_CharacterInput.hasMovementInput)
			{
				input *= m_CurrentGroundMovementConfig.sprintScale;
			}
			m_ExplorationAverageForwardInput.Add(input);
			
			normalizedForwardSpeed = m_ExplorationAverageForwardInput.average;
		}

		// Calculates the character's normalized forward and lateral speeds for strafe
		void CalculateStrafeMovement()
		{
			m_StrafeAverageForwardInput.Add(m_CharacterInput.moveInput.y);
			float averageForwardInput = m_StrafeAverageForwardInput.average;
			m_StrafeAverageLateralInput.Add(m_CharacterInput.moveInput.x);
			float averageLateralInput = m_StrafeAverageLateralInput.average;
			
			normalizedForwardSpeed = Mathf.Approximately(averageForwardInput, 0f) ? 0f : averageForwardInput;
			normalizedLateralSpeed = Mathf.Approximately(averageLateralInput, 0f)
				? 0f : averageLateralInput;
		}

		// Calculates the local input direction
		// 		return: Input direction relative to main camera
		Vector3 CalculateLocalInputDirection()
		{
			var localMovementDirection = new Vector3(m_CharacterInput.moveInput.x, 0f, m_CharacterInput.moveInput.y);
			return Quaternion.AngleAxis(m_MainCamera.transform.eulerAngles.y, Vector3.up) * 
			       localMovementDirection.normalized;
		}

		// Calculate character's target rotation
		// 		localDirection: character's current rotation
		// 		return: Target rotation
		Quaternion CalculateTargetRotation(Vector3 localDirection)
		{
			Vector3 flatForward = m_MainCamera.transform.forward;
			flatForward.y = 0f;
			flatForward.Normalize();
			
			Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			return Quaternion.LookRotation(cameraToInputOffset * flatForward);
		}

		// Sets 'normalizedForwardSpeed' so that a turn will approach the desired rotation.
		// 		currentRotation: Current rotation
		// 		newRotation: Desired rotation
		void SetTurningSpeed(Quaternion currentRotation, Quaternion newRotation)
		{
			float currentY = currentRotation.eulerAngles.y;
			float newY = newRotation.eulerAngles.y;
			float difference = (newY - currentY).Wrap180() / Time.deltaTime;

			normalizedTurningSpeed = Mathf.Lerp(normalizedTurningSpeed,Mathf.Clamp(
													difference / m_Configuration.turningYSpeed *
													m_Configuration.turningSpeedScaleVisual, -1, 1),
													Time.deltaTime * m_Configuration.normalizedTurningSpeedLerpSpeedFactor);
		}

		// Resets m_MovementState to its value prior to the turn around.
		void TurnaroundComplete()
		{
			m_MovementState = m_PreTurnMovementState;
		}

		// Checks and handles rapid turn
		// 		target: character target rotation
		// 		return: true if a rapid turn occurs
		bool CheckForAndHandleRapidTurn(Quaternion target)
		{
			if (m_ThirdPersonBrain.turnAround == null || disableRapidTurn)
			{
				return false;
			}
			
			float angle;
			if (ShouldTurnAround(out angle, target))
			{
				StartTurnAround(angle);
				return true;
			}
			return false;
		}

		// Starts the turn around
		// 		angle: Turn around angle
		void StartTurnAround(float angle)
		{
			m_TurnaroundMovementTime = 0f;
			cachedForwardVelocity = m_AverageForwardVelocity.average;
			m_PreTurnMovementState = m_MovementState;
			m_MovementState = ThirdPersonGroundMovementState.TurningAround;
			m_JumpQueued = false;
			m_ThirdPersonBrain.turnAround.TurnAround(angle);
			m_ThirdPersonBrain.turnAround.turnaroundComplete += OnTurnAroundComplete;
		}

		// Fired when the turn around is finished
		void OnTurnAroundComplete()
		{
			m_ThirdPersonBrain.turnAround.turnaroundComplete -= OnTurnAroundComplete;

			if (!m_CharacterInput.hasMovementInput)
			{
				return;
			}
			Quaternion target = CalculateTargetRotation(
				new Vector3(m_CharacterInput.moveInput.x, 0, m_CharacterInput.moveInput.y));
			var angle = (target.eulerAngles.y - m_Transform.eulerAngles.y).Wrap180();
			if (Mathf.Abs(angle) > m_Configuration.stationaryAngleRapidTurn)
			{
				StartTurnAround(angle);
			}
		}

		// Decides whether a rapid turn should be initiated.
		// 		angle: The angle of the rapid turn. 0 if no rapid turn was detected
		// 		target: Target character direction
		// 		return: True is a rapid turn has been detected
		bool ShouldTurnAround(out float angle, Quaternion target)
		{
			if (normalizedForwardSpeed < m_Configuration.standingTurnThreshold)
			{
				m_PreviousInputs.Clear();
				angle = (target.eulerAngles.y - m_Transform.eulerAngles.y).Wrap180();
				return Mathf.Abs(angle) > m_Configuration.stationaryAngleRapidTurn;
			}

			foreach (Vector2 previousInputsValue in m_PreviousInputs.values)
			{ 
				angle = Mathf.Abs(Vector2.SignedAngle(previousInputsValue, m_CharacterInput.moveInput));
				if (angle > m_Configuration.inputAngleRapidTurn && 
				    Vector2.Distance(previousInputsValue, m_CharacterInput.moveInput) > 1.0f)
				{
					m_PreviousInputs.Clear();
					return true;
				}
			}
			angle = 0;
			return false;
		}
		
		// Attempts a jump. If successful fires the 'jumpStarted' event and sets 'm_AerialState' to ThirdPersonAerialMovementState.Jumping
		// reattempt: Whether a jump should be reattempted
		void TryJump(out bool reattempt)
		{
			if (m_MovementState == ThirdPersonGroundMovementState.TurningAround || 
			    m_ThirdPersonBrain.animatorState == ThirdPersonBrain.AnimatorState.Landing ||
			    m_ThirdPersonBrain.animatorState == ThirdPersonBrain.AnimatorState.JumpLanding)
			{
				reattempt = true;
				return;
			}
			if (!isGrounded || m_ControllerAdapter.startedSlide || !m_ThirdPersonBrain.isGroundedState)
			{
				reattempt = false;
				return;
			}
			
			m_AerialState = ThirdPersonAerialMovementState.Jumping;
			m_FallDirection = CalculateLocalInputDirection();
			
			// If an idle forward jump is detected update the normalized speeds to represent a forward jump and pass 
			// them on to the ThirdPersonBrain.
			if (IsIdleForwardJump())
			{
				cachedForwardVelocity = m_Configuration.standingJumpSpeed;
				if (movementMode == ThirdPersonMotorMovementMode.Exploration)
				{
					normalizedForwardSpeed = 1.0f;
				}
				else
				{
					normalizedLateralSpeed = m_CharacterInput.moveInput.x;
					normalizedForwardSpeed = m_CharacterInput.moveInput.y;
					m_ThirdPersonBrain.UpdateLateralSpeed(normalizedLateralSpeed, 1.0f);
				}
				m_ThirdPersonBrain.UpdateForwardSpeed(normalizedForwardSpeed, 1.0f);
			}
			else
			{
				cachedForwardVelocity = m_AverageForwardVelocity.average;
			}
			
			if (Mathf.Abs(normalizedLateralSpeed) >= 0.8f)
			{
				cachedForwardVelocity *= m_Configuration.lateralStrafeJumpMultiplier;
			}
			
			m_ControllerAdapter.SetJumpVelocity(
				m_Configuration.jumpHeightAsFactorOfForwardSpeed.Evaluate(normalizedForwardSpeed));
			
			if (jumpStarted != null)
			{
				jumpStarted();
			}

			reattempt = false;
		}

		// Helper for deciding jump is idle forward jump.
		bool IsIdleForwardJump()
		{
			return m_CharacterInput.moveInput.magnitude > m_Configuration.standingJumpMinInputThreshold &&
			        m_LastIdleTime + m_Configuration.standingJumpMoveTimeThreshold >= Time.time &&
			        m_Animator.deltaPosition.GetMagnitudeOnAxis(m_Transform.forward) <=
			        m_Configuration.standingJumpMaxMovementThreshold * Time.deltaTime;
		}

		// Updates the character fall forward speed
		void UpdateFallForwardSpeed()
		{
			float maxFallForward = m_Configuration.fallingForwardSpeed;
			float target = maxFallForward * Mathf.Clamp01(m_CharacterInput.moveInput.magnitude);
			float time = cachedForwardVelocity > target
				             ? m_Configuration.fallSpeedDeceleration
				             : m_Configuration.fallSpeedAcceleration;
			cachedForwardVelocity = Mathf.Lerp(cachedForwardVelocity, target, time);
			normalizedForwardSpeed = Mathf.Sign(normalizedForwardSpeed) * cachedForwardVelocity / maxFallForward;
		}
	}
	
	
	/// <summary>
	/// Enum used to describe the third person aerial movement state.
	/// </summary>
	public enum ThirdPersonAerialMovementState
	{
		Grounded,
		Jumping,
		Falling
	}
	
	/// <summary>
	/// Enum used to describe the third person ground movement state.
	/// </summary>
	public enum ThirdPersonGroundMovementState
	{
		Walking,
		Running,
		TurningAround
	}
	
	/// <summary>
	/// Enum used to describe the third person movement mode.
	/// </summary>
	public enum ThirdPersonMotorMovementMode
	{
		Exploration,
		Strafe
	}
}