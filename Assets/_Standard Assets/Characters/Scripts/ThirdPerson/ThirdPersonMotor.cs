using System;
using System.Collections.Generic;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Helpers;
using StandardAssets.Characters.ThirdPerson.Configs;
using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// IThirdPersonMotor implementation that uses, primarily, root motion from the animator to move the character.
	/// </summary>
	[Serializable]
	public class ThirdPersonMotor
	{
		/// <summary>
		/// Track distance above the ground at these frame intervals (to prevent checking every frame)
		/// </summary>
		const int k_TrackGroundFrameIntervals = 5;
		
		/// <summary>
		/// Various configuration settings more movement.
		/// </summary>
		[FormerlySerializedAs("configuration")]
		[SerializeField, Tooltip("Reference to the configuration with all the movement settings")]
		protected MotorConfig m_Configuration;

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
		public float fallTime
		{
			get { return m_ControllerAdapter.fallTime; }
		}

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
		/// Is rapid turn disabled? (Enable/Disable it via <see cref="EnableRapidTurn"/>/<see cref="DisableRapidTurn"/>).
		/// </summary>
		bool disableRapidTurn
		{
			get { return m_ObjectsThatDisabledRapidTurn.Count > 0; }
		}

		/// <summary>
		/// Fired on jump.
		/// </summary>
		public event Action jumpStarted;

		/// <summary>
		/// Fired when the character starts falling.
		/// </summary>
		public event Action<float> fallStarted;

		/// <summary>
		/// The input implementation
		/// </summary>
		IThirdPersonInput m_CharacterInput;

		/// <summary>
		/// The controller controllerAdapter implementation
		/// </summary>
		ControllerAdapter m_ControllerAdapter;

		ThirdPersonGroundMovementState m_PreTurnMovementState;
		ThirdPersonGroundMovementState m_MovementState = ThirdPersonGroundMovementState.Walking;
		ThirdPersonAerialMovementState m_AerialState = ThirdPersonAerialMovementState.Grounded;

		SlidingAverage m_AverageForwardVelocity,
		                       m_ExplorationAverageForwardInput,
		                       m_StrafeAverageForwardInput,
		                       m_StrafeAverageLateralInput;

		float m_TurnaroundMovementTime,
		              m_LastIdleTime;
		bool m_JumpQueued;
		Animator m_Animator;
		Vector3 m_FallDirection;
		Transform m_Transform;
		GameObject m_GameObject;
		ThirdPersonBrain m_ThirdPersonBrain;
		SizedQueue<Vector2> m_PreviousInputs;
		Camera m_MainCamera;
		
		// on start strafe control initial look
		bool m_IsInitialStrafeLook;
		float m_InitialStrafeLookCount;
		Quaternion m_RotationOnStrafeStart;

		
		/// <summary>
		/// Gets whether to track height above the ground.
		/// </summary>
		bool m_TrackGroundHeight;

		/// <summary>
		/// List of objects that disabled rapid turn. To allow multiple objects to disable it temporarily.
		/// </summary>
		readonly List<object> m_ObjectsThatDisabledRapidTurn = new List<object>();

		/// <summary>
		/// Gets the current <see cref="TurnAroundBehaviour"/>.
		/// </summary>
		public TurnAroundBehaviour currentTurnAroundBehaviour
		{
			get { return m_ThirdPersonBrain.turnAround; }
		}
	
		/// <summary>
		/// Gets the vertical speed.
		/// </summary>
		/// <value>Range = -1 (falling) to 1 (jumping).</value>
		/// <remarks>Returns <see cref="ControllerAdapter"/>'s <see cref="ControllerAdapter.normalizedVerticalSpeed"/>.</remarks>
		public float normalizedVerticalSpeed
		{
			get { return m_ControllerAdapter.normalizedVerticalSpeed; }
		}
		
		/// <summary>
		/// Gets whether the character is in a sprint state.
		/// </summary>
		/// <value>True if in a sprint state; false otherwise.</value>
		public bool sprint { get; private set; }
		
		/// <summary>
		/// Gets the current <see cref="ThirdPersonGroundMovementState"/>.
		/// </summary>
		public ThirdPersonGroundMovementState currentGroundMovementState
		{
			get { return m_MovementState; }
		}
		
		/// <summary>
		/// Gets the current <see cref="ThirdPersonAerialMovementState"/>.
		/// </summary>
		public ThirdPersonAerialMovementState currentAerialMovementState
		{
			get { return m_AerialState; }
		}

		/// <summary>
		/// Called on the exit of the root motion jump animation.
		/// </summary>
		public void OnJumpAnimationComplete()
		{
			if (m_ControllerAdapter.IsPredictedFallShort())
			{
				OnLanding();
			}
		}

		/// <summary>
		/// Whether the character it grounded
		/// </summary>
		/// <value>True if <see cref="m_AerialState"/> is grounded</value>
		bool IsGrounded
		{
			get { return m_AerialState == ThirdPersonAerialMovementState.Grounded; }
		}

		/// <summary>
		/// Moves the character based on movement and animator state.
		/// </summary>
		/// <remarks>Called by the Animator</remarks>
		public void OnAnimatorMove()
		{
			if (m_MovementState == ThirdPersonGroundMovementState.TurningAround)
			{
				m_ControllerAdapter.Move(m_ThirdPersonBrain.turnAround.GetMovement(), Time.deltaTime);
				return;
			}

			if (m_ThirdPersonBrain.isRootMotionState)
			{
				Vector3 groundMovementVector = m_Animator.deltaPosition * m_Configuration.scaleRootMovement;
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

		public void OnSprintStarted()
		{
			sprint = !sprint;
		}
		
		public void OnSprintEnded()
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
			if (m_Configuration.autoToggleSprintOnNoInput && sprint && !m_CharacterInput.hasMovementInput)
			{
				OnSprintEnded();
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
		/// Track height above ground when the physics character is in the air, but the animation has not yet changed to
		/// the fall animation.
		/// </summary>
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

		/// <summary>
		/// Sets the aerial state to <see cref="ThirdPersonAerialMovementState.Grounded"/> and clears
		/// <see cref="m_AverageForwardVelocity"/> if no input.
		/// </summary>
		void OnLanding()
		{
			m_AerialState = ThirdPersonAerialMovementState.Grounded;

			if (!m_CharacterInput.hasMovementInput)
			{
				m_AverageForwardVelocity.Clear();
			}
		}

		/// <summary>
		/// Sets the aerial state to <see cref="ThirdPersonAerialMovementState.Falling"/> and fires the
		/// <see cref="fallStarted"/> event.
		/// </summary>
		/// <remarks>This subscribes to <see cref="ControllerAdapter.startedFalling"/></remarks>
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
			
			m_AerialState = ThirdPersonAerialMovementState.Falling;
			
			if (fallStarted != null)
			{
				fallStarted(predictedFallDistance);
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
			m_InitialStrafeLookCount = m_Configuration.initialStrafeLookDuration;
			m_RotationOnStrafeStart = m_Transform.rotation;
		}
		
		/// <summary>
		/// Changes movement mode to <see cref="ThirdPersonMotorMovementMode.Exploration"/>
		/// </summary>
		public void EndStrafe()
		{
			movementMode = ThirdPersonMotorMovementMode.Exploration;
		}

		void SetStrafeLookDirection()
		{
			Quaternion targetRotation = CalculateTargetRotation(Vector3.forward);
			targetYRotation = targetRotation.eulerAngles.y;
			Quaternion newRotation;

			if (m_IsInitialStrafeLook)
			{
				newRotation = Quaternion.Lerp(m_RotationOnStrafeStart, targetRotation, 
					1.0f - m_InitialStrafeLookCount / m_Configuration.initialStrafeLookDuration);
				m_InitialStrafeLookCount -= Time.deltaTime;
				if (m_InitialStrafeLookCount <= 0.0f)
				{
					m_IsInitialStrafeLook = false;
				}
			}
			else
			{
				newRotation = Quaternion.RotateTowards(m_Transform.rotation, targetRotation,
								m_Configuration.turningYSpeed * m_Configuration.strafeTurningSpeedScale * Time.deltaTime);
			}
			
			SetTurningSpeed(m_Transform.rotation, newRotation);
			m_Transform.rotation = newRotation;
		}

		void SetExplorationLookDirection()
		{
			if (!m_CharacterInput.hasMovementInput)
			{
				normalizedTurningSpeed = 0;
				targetYRotation = m_Transform.eulerAngles.y;
				return;
			}

			Quaternion targetRotation = CalculateTargetRotation(new Vector3(m_CharacterInput.moveInput.x, 0, m_CharacterInput.moveInput.y));
			targetYRotation = targetRotation.eulerAngles.y;

			if (IsGrounded && CheckForAndHandleRapidTurn(targetRotation))
			{
				return;
			}

			float turnSpeed = IsGrounded
				? m_Configuration.turningYSpeed
				: m_Configuration.jumpTurningYSpeed;

			Quaternion newRotation = Quaternion.RotateTowards(m_Transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

			SetTurningSpeed(m_Transform.rotation, newRotation);

			m_Transform.rotation = newRotation;
		}

		void CalculateForwardMovement()
		{
			if (m_MovementState == ThirdPersonGroundMovementState.TurningAround && m_TurnaroundMovementTime < m_Configuration.ignoreInputTimeRapidTurn)
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
			m_ExplorationAverageForwardInput.Add(inputVector.magnitude + (sprint && m_CharacterInput.hasMovementInput
											  ? m_Configuration.sprintNormalizedForwardSpeedIncrease : 0));
			
			normalizedForwardSpeed = m_ExplorationAverageForwardInput.average;
		}

		void CalculateStrafeMovement()
		{
			m_StrafeAverageForwardInput.Add(m_CharacterInput.moveInput.y);
			float averageForwardInput = m_StrafeAverageForwardInput.average;
			m_StrafeAverageLateralInput.Add(m_CharacterInput.moveInput.x);
			float averageLateralInput = m_StrafeAverageLateralInput.average;
			
			normalizedForwardSpeed =
				Mathf.Clamp((Mathf.Approximately(averageForwardInput, 0f) ? 0f : averageForwardInput),
							-m_Configuration.normalizedBackwardStrafeSpeed, m_Configuration.normalizedForwardStrafeSpeed);
			normalizedLateralSpeed = Mathf.Approximately(averageLateralInput, 0f)
				? 0f : averageLateralInput * m_Configuration.normalizedLateralStrafeSpeed;
		}

		Vector3 CalculateLocalInputDirection()
		{
			var localMovementDirection = new Vector3(m_CharacterInput.moveInput.x, 0f, m_CharacterInput.moveInput.y);
			return Quaternion.AngleAxis(m_MainCamera.transform.eulerAngles.y, Vector3.up) * 
			       localMovementDirection.normalized;
		}

		Quaternion CalculateTargetRotation(Vector3 localDirection)
		{
			Vector3 flatForward = CalculateCharacterBearing();
			
			Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			return Quaternion.LookRotation(cameraToInputOffset * flatForward);
		}

		Vector3 CalculateCharacterBearing()
		{
			Vector3 bearing = m_MainCamera.transform.forward;
			bearing.y = 0f;
			bearing.Normalize();

			return bearing;
		}

		/// <summary>
		/// Sets <see cref="normalizedForwardSpeed"/> so that a turn will approach the desired rotation.
		/// </summary>
		/// <param name="currentRotation">Current rotation.</param>
		/// <param name="newRotation">Desired rotation.</param>
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

		/// <remarks>Subscribes to the <see cref="currentTurnAroundBehaviour"/>'s
		/// <see cref="TurnAroundBehaviour.turnaroundComplete"/> </remarks>
		void TurnaroundComplete()
		{
			m_MovementState = m_PreTurnMovementState;
		}

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

		/// <summary>
		/// Decides whether a rapid turn should be initiated.
		/// </summary>
		/// <param name="angle">The angle of the rapid turn. 0 if no rapid turn was detected.</param>
		/// <param name="target">Target character direction.</param>
		/// <returns>True is a rapid turn has been detected.</returns>
		bool ShouldTurnAround(out float angle, Quaternion target)
		{
			if (normalizedForwardSpeed < m_Configuration.standingTurnaroundSpeedThreshold)
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
		
		/// <summary>
		/// Attempts a jump. If successful fires the <see cref="jumpStarted"/> event and
		/// sets <see cref="m_AerialState"/> to <see cref="ThirdPersonAerialMovementState.Jumping"/>.
		/// </summary>
		/// <param name="reattempt">Whether a jump should be reattempted</param>
		void TryJump(out bool reattempt)
		{
			if (m_MovementState == ThirdPersonGroundMovementState.TurningAround || 
			    m_ThirdPersonBrain.animatorState == ThirdPersonBrain.AnimatorState.Landing)
			{
				reattempt = true;
				return;
			}
			if (!IsGrounded || m_ControllerAdapter.startedSlide || !m_ThirdPersonBrain.isRootMotionState)
			{
				reattempt = false;
				return;
			}
			
			m_AerialState = ThirdPersonAerialMovementState.Jumping;
			m_FallDirection = CalculateLocalInputDirection();
			
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

		bool IsIdleForwardJump()
		{
			return m_CharacterInput.moveInput.magnitude > m_Configuration.standingJumpMinInputThreshold &&
			        m_LastIdleTime + m_Configuration.standingJumpMoveThresholdTime >= Time.time &&
			        m_Animator.deltaPosition.GetMagnitudeOnAxis(m_Transform.forward) <=
			        m_Configuration.standingJumpMaxMovementThreshold * Time.deltaTime;
		}

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