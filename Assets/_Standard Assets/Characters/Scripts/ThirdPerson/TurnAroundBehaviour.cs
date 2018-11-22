using System;
using StandardAssets.Characters.Helpers;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
    /// <summary>
    /// Class to handle rapid turns
    /// </summary>
    public abstract class TurnAroundBehaviour
    {
        /// <summary>
        /// Value multiplicatively applied to the head look at turn angle
        /// </summary>
        public abstract float headTurnScale { get; }

        /// <summary>
        /// Event fired on completion of turnaround
        /// </summary>
        public event Action turnaroundComplete;

        /// <summary>
        /// Whether this object is currently handling a turnaround motion
        /// </summary>
        protected bool isTurningAround { get; private set; }

        /// <summary>
        /// Initializes the turn around behaviour.
        /// </summary>
        public abstract void Init(ThirdPersonBrain brain);

        /// <summary>
        /// Handles the update logic of a turn around.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Gets the movement of the character
        /// </summary>
        /// <returns>Movement to apply to the character</returns>
        public abstract Vector3 GetMovement();

        /// <summary>
        /// Starts a turnaround
        /// </summary>
        /// <param name="angle">Target y rotation in degrees</param>
        public void TurnAround(float angle)
        {
            if (isTurningAround)
            {
                return;
            }

            isTurningAround = true;
            StartTurningAround(angle);
        }

        /// <summary>
        /// Called on completion of turnaround. Fires <see cref="turnaroundComplete"/>  event.
        /// </summary>
        protected void EndTurnAround()
        {
            isTurningAround = false;
            FinishedTurning();
            if (turnaroundComplete != null)
            {
                turnaroundComplete();
            }
        }

        /// <summary>
        /// Method fired on completion of a turn around.
        /// </summary>
        protected abstract void FinishedTurning();

        /// <summary>
        ///Method fired on start of a turn around.
        /// </summary>
        protected abstract void StartTurningAround(float angle);
    }

    /// <summary>
    /// Enum used to describe a turnaround type.
    /// </summary>
    public enum TurnAroundType
    {
        None,
        Blendspace,
        Animation
    }

    /// <summary>
    /// Animation extension of TurnaroundBehaviour. Rotates the character to the target angle while playing an animation.
    /// </summary>
    /// <remarks>This turnaround type should be used to improve fidelity at the cost of responsiveness.</remarks>
    [Serializable]
    public class AnimationTurnAroundBehaviour : TurnAroundBehaviour
    {
        /// <summary>
        /// State of the animation turn around.
        /// </summary>
        enum State
        {
            Inactive,
            WaitingForTransition,
            Transitioning,
            TurningAnimation,
            TransitioningOut
        }

        /// <summary>
        /// Model to store data per animation turnaround
        /// </summary>
        [Serializable]
        protected class AnimationInfo
        {
            string m_Name;

            [SerializeField, Tooltip("Animation play speed")]
            float m_Speed = 1.0f;
            
            [SerializeField, Tooltip("Head look at angle scale during animation")]
            float m_HeadTurnScale = 1.0f;

            /// <summary>
            /// Gets the animation state name.
            /// </summary>
            public string name { get { return m_Name; } }

            /// <summary>
            /// Gets the animation play speed.
            /// </summary>
            public float speed { get { return m_Speed; } }

            /// <summary>
            /// Gets the head look at angle scale during animation.
            /// </summary>
            public float headTurnScale { get { return m_HeadTurnScale; } }            


            /// <summary>
            /// Initialized <see cref="m_Name"/>.
            /// </summary>
            public AnimationInfo(string name)
            {
                m_Name = name;
            }
        }

        [SerializeField, Tooltip("Configuration for run 180 left turn animation")]
        AnimationInfo m_RunLeftTurn = new AnimationInfo(AnimationControllerInfo.k_Run180TurnLeftState);
        
        [SerializeField, Tooltip("Configuration for run 180 right turn animation")]
        AnimationInfo m_RunRightTurn = new AnimationInfo(AnimationControllerInfo.k_Run180TurnRightState);
        
        [SerializeField, Tooltip("Configuration for sprint 180 left turn animation")]
        AnimationInfo m_SprintLeftTurn = new AnimationInfo(AnimationControllerInfo.k_Run180TurnLeftState);
        
        [SerializeField, Tooltip("Configuration for sprint 180 right turn animation")]
        AnimationInfo m_SprintRightTurn = new AnimationInfo(AnimationControllerInfo.k_Run180TurnRightState);
        
        [SerializeField, Tooltip("Configuration for idle 180 left turn animation")]
        AnimationInfo m_IdleLeftTurn = new AnimationInfo(AnimationControllerInfo.k_Idle180TurnLeftState);
        
        [SerializeField, Tooltip("Configuration for idle 180 right turn animation")]
        AnimationInfo m_IdleRightTurn = new AnimationInfo(AnimationControllerInfo.k_Idle180TurnRightState);

        [SerializeField, Tooltip("Curve used to evaluate rotation throughout turn around")]
        AnimationCurve m_RotationMap = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

        [SerializeField, Tooltip("Value (normalized forward speed) used to determine if a run turn should be used instead of an idle turn")]
        float m_RunTurnThreshold = 0.1f;

        [SerializeField, Tooltip("Duration of the cross fade into turn animation")]
        float m_CrossfadeDuration = 0.125f;

        float m_TargetAngle; // target y rotation angle in degrees
        float m_CachedAnimatorSpeed; // speed of the animator prior to starting an animation turnaround
        float m_CacheForwardSpeed; // forwards speed of the motor prior to starting an animation turnaround
        Quaternion m_StartRotation; // rotation of the character as turnaround is started
        AnimationInfo m_CurrentAnimationInfo; // currently selected animation info
        ThirdPersonBrain m_ThirdPersonBrain;
        Transform m_Transform; // character's transform
        State m_State; // state used to determine where to retrieve animator normalized time from

        /// <inheritdoc />
        public override float headTurnScale
        {
            get { return m_CurrentAnimationInfo == null ? 1.0f : m_CurrentAnimationInfo.headTurnScale; }
        }

        // Cached reference of the animator
        Animator animator { get { return m_ThirdPersonBrain.animator; } }


        /// <inheritdoc/>
        public override void Init(ThirdPersonBrain brain)
        {
            m_ThirdPersonBrain = brain;
            m_Transform = brain.transform;
        }

        /// <summary>
        /// Rotates the character toward <see cref="m_TargetAngle"/> using the animation's normalized progress/>
        /// </summary>
        public override void Update()
        {
            if (!isTurningAround)
            {
                return;
            }

            // check if next or current state normalized time is appropriate.
            var currentStateNormalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            var normalizedTime = 0.0f;
            switch (m_State)
            {
                case State.WaitingForTransition: // wait a a frame for transition
                    m_State = State.Transitioning;
                    break;
                case State.Transitioning: // transitioning into animation use next state time until transition is complete.
                    if (!animator.IsInTransition(0))
                    {
                        m_State = State.TurningAnimation;
                        normalizedTime = currentStateNormalizedTime;
                    }
                    else
                    {
                        // transitioning, use next state's normalized time.
                        normalizedTime = animator.GetNextAnimatorStateInfo(0).normalizedTime;
                    }

                    break;
                case State.TurningAnimation: // playing turn use current state until turn is complete
                    normalizedTime = currentStateNormalizedTime;
                    if (normalizedTime >= 1.0f)
                    {
                        m_State = State.TransitioningOut;
                        return;
                    }

                    break;
                case State.TransitioningOut: // transition out of turn don't rotate just wait for transition end
                    if (!animator.IsInTransition(0))
                    {
                        m_State = State.Inactive;
                        animator.speed = m_CachedAnimatorSpeed;
                        EndTurnAround();
                    }

                    return; // don't rotate character
            }

            m_ThirdPersonBrain.UpdateForwardSpeed(m_CacheForwardSpeed, float.MaxValue);

            var rotationProgress = m_RotationMap.Evaluate(normalizedTime);
            m_Transform.rotation = Quaternion.AngleAxis(rotationProgress * m_TargetAngle, Vector3.up) * m_StartRotation;
        }

        /// <inheritdoc />
        public override Vector3 GetMovement()
        {
            if (m_CurrentAnimationInfo == m_IdleLeftTurn || m_CurrentAnimationInfo == m_IdleRightTurn)
            {
                return Vector3.zero;
            }

            return animator.deltaPosition;
        }

        /// <inheritdoc/>
        protected override void FinishedTurning() { }

        /// <summary>
        /// Using the target angle and <see cref="ThirdPersonBrain.isRightFootPlanted"/> selects the
        /// appropriate animation to cross fade into.
        /// </summary>
        /// <param name="angle">The target angle in degrees.</param>
        protected override void StartTurningAround(float angle)
        {
            m_TargetAngle = angle.Wrap180();
            m_CurrentAnimationInfo = GetCurrent(m_ThirdPersonBrain.animatorForwardSpeed, angle > 0.0f,
                !m_ThirdPersonBrain.isRightFootPlanted);

            m_StartRotation = m_Transform.rotation;
            animator.CrossFade(m_CurrentAnimationInfo.name, m_CrossfadeDuration, 0, 0.0f);

            m_CachedAnimatorSpeed = animator.speed;
            animator.speed = m_CurrentAnimationInfo.speed;

            m_CacheForwardSpeed = m_ThirdPersonBrain.animatorForwardSpeed;

            m_State = State.WaitingForTransition;
        }

        // Determines which animation should be played
        //     forwardSpeed: character's normalized forward speed
        //     turningClockwise: Is the character turning clockwise
        //     leftFootPlanted: Is the character's left foot currently planted
        //     return: The determined AnimationInfo
        AnimationInfo GetCurrent(float forwardSpeed, bool turningClockwise, bool leftFootPlanted)
        {
            // idle turn
            if (forwardSpeed < m_RunTurnThreshold)
            {
                return turningClockwise ? m_IdleRightTurn : m_IdleLeftTurn;
            }

            // < 180 turn
            if (m_TargetAngle < 170.0f || m_TargetAngle > 190.0f)
            {
                return CurrentRun(forwardSpeed, turningClockwise);
            }

            // 180 turns should be based on the grounded foot
            m_TargetAngle = Mathf.Abs(m_TargetAngle);
            if (!leftFootPlanted)
            {
                m_TargetAngle *= -1.0f;
            }

            return CurrentRun(forwardSpeed, leftFootPlanted);
        }

        // Determines if the run or sprint AnimationInfo should be selected
        //     forwardSpeed: Character's normalized forward speed
        //     turningClockwise: Is the character turning clockwise
        //     return: The determined AnimationInfo
        AnimationInfo CurrentRun(float forwardSpeed, bool turningClockwise)
        {
            if (turningClockwise)
            {
                return forwardSpeed <= 1.0f ? m_RunRightTurn : m_SprintRightTurn;
            }

            return forwardSpeed <= 1.0f ? m_RunLeftTurn : m_SprintLeftTurn;
        }
    }


    /// <summary>
    /// Blendspace extension of TurnaroundBehaviour. Rotates the character to the target angle using blendspace.
    /// </summary>
    [Serializable]
    public class BlendspaceTurnAroundBehaviour : TurnAroundBehaviour
    {
        [SerializeField, Tooltip("Duration in seconds of the turn around")]
        float m_TimeToTurn = 0.2f;

        [SerializeField, Tooltip("Curve used to evaluate rotation throughout turn around")]
        AnimationCurve m_RotationMap = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

        [SerializeField, Tooltip("Curve used to evaluate forward speed throughout turn around")]
        AnimationCurve m_ForwardSpeedMap = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

        [SerializeField, Tooltip("Method to apply forward speed during turn around")]
        BlendspaceCalculation m_ForwardSpeedType = BlendspaceCalculation.Multiplicative;

        [SerializeField, Tooltip("An turning angle larger than this will be a large (180 degree) turn, otherwise it is a small (90 degree) turn")]
        float m_TurnAngleThreshold = 150.0f;

        [SerializeField, Tooltip("Curve used to evaluate movement throughout a large (180 dgree) turn around")]
        AnimationCurve m_LargeTurnMap = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

        [SerializeField, Tooltip("Curve used to evaluate movement throughout a small (90 degree) turn around")]
        AnimationCurve m_SmallTurnMap = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

        [SerializeField, Tooltip("Amount to scale the dynamic head look at angle during a turn around")]
        float m_HeadTurnScale = 1.0f;

        // Has the current turn been classified as a small turn.
        bool m_IsSmallTurn;

        // The counter of the current turn.
        float m_TurningTime;

        // The motor's current forward speed.
        float m_CurrentForwardSpeed;

        // The motor's current turning speed.
        float m_CurrentTurningSpeed;

        // The turn's target angle
        float m_TargetAngle;

        // The character's rotation at the start of the turn around
        Vector3 m_StartRotation;

        // The vector passed to the motor to move the character
        Vector3 m_MovementVector;

        // Cached reference of the ThirdPersonBrain
        ThirdPersonBrain m_ThirdPersonBrain;

        // Cached reference of the ThirdPersonBrain's transform
        Transform m_Transform;

        /// <inheritdoc/>
        public override float headTurnScale { get { return m_HeadTurnScale; } }

        // The curve used to evaluate movement throughout a turnaround.
        AnimationCurve turnMovementOverTime { get { return m_IsSmallTurn ? m_SmallTurnMap : m_LargeTurnMap; } }


        /// <inheritdoc/>
        public override void Init(ThirdPersonBrain brain)
        {
            m_Transform = brain.transform;
            m_ThirdPersonBrain = brain;
        }

        /// <summary>
        /// Evaluates the turn and rotates the character.
        /// </summary>
        public override void Update()
        {
            if (isTurningAround)
            {
                EvaluateTurn();
                m_TurningTime += Time.deltaTime;
                if (m_TurningTime >= m_TimeToTurn)
                {
                    EndTurnAround();
                }
            }
        }

        /// <inheritdoc/>
        public override Vector3 GetMovement()
        {
            var normalizedTime = m_TurningTime / m_TimeToTurn;
            return m_MovementVector * turnMovementOverTime.Evaluate(normalizedTime) * Time.deltaTime;
        }

        /// <summary>
        /// Updates the <see cref="ThirdPersonBrain"/> with a new forward and turning speed.
        /// </summary>
        protected override void FinishedTurning()
        {
            m_TurningTime = m_TimeToTurn;
            EvaluateTurn();
            m_ThirdPersonBrain.UpdateForwardSpeed(m_CurrentForwardSpeed, Time.deltaTime);
            m_ThirdPersonBrain.UpdateTurningSpeed(m_CurrentTurningSpeed, Time.deltaTime);
        }

        /// <summary>
        /// Starts the blendspace turnaround.
        /// </summary>
        /// <param name="angle">The target angle.</param>
        protected override void StartTurningAround(float angle)
        {
            m_IsSmallTurn = Mathf.Abs(angle) < m_TurnAngleThreshold;
            m_TargetAngle = angle.Wrap180();
            m_TurningTime = 0.0f;
            m_CurrentForwardSpeed = m_ThirdPersonBrain.animatorForwardSpeed;
            m_CurrentTurningSpeed = m_ThirdPersonBrain.animatorTurningSpeed;

            m_StartRotation = m_Transform.eulerAngles;

            var motor = m_ThirdPersonBrain.thirdPersonMotor;
            if (motor != null)
            {
                var rotatedVector = Quaternion.AngleAxis(angle, Vector3.up) * m_Transform.forward;
                m_MovementVector = rotatedVector * motor.cachedForwardVelocity;
                m_MovementVector.y = 0.0f;
            }
        }

        // Evaluates the current rotation to be applied to the character.
        void EvaluateTurn()
        {
            var normalizedTime = m_TurningTime / m_TimeToTurn;

            var forwardSpeedValue = m_ForwardSpeedMap.Evaluate(normalizedTime);

            if (m_ForwardSpeedType == BlendspaceCalculation.Multiplicative)
            {
                forwardSpeedValue = forwardSpeedValue * m_CurrentForwardSpeed;
            }
            else
            {
                forwardSpeedValue = forwardSpeedValue + m_CurrentForwardSpeed;
            }

            m_ThirdPersonBrain.UpdateForwardSpeed(forwardSpeedValue, Time.deltaTime);

            var newRotation =
                m_StartRotation + new Vector3(0.0f, m_RotationMap.Evaluate(normalizedTime) * m_TargetAngle, 0.0f);
            m_Transform.rotation = Quaternion.Euler(newRotation);
        }

        /// <summary>
        /// Enum describing a mathematics operation.
        /// </summary>
        protected enum BlendspaceCalculation
        {
            Additive,
            Multiplicative
        }
    }
}
