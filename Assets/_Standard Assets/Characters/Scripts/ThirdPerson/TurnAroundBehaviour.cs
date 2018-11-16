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
        bool m_IsTurningAround;

        public bool isTurningAround
        {
            get { return m_IsTurningAround; }
        }

        public abstract void Init(ThirdPersonBrain brain);

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
            if (m_IsTurningAround)
            {
                return;
            }

            m_IsTurningAround = true;
            StartTurningAround(angle);
        }

        /// <summary>
        /// Called on completion of turnaround. Fires <see cref="turnaroundComplete"/>  event.
        /// </summary>
        protected void EndTurnAround()
        {
            m_IsTurningAround = false;
            FinishedTurning();
            if (turnaroundComplete != null)
            {
                turnaroundComplete();
            }
        }

        protected abstract void FinishedTurning();

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
            [Tooltip("Animation state name")]
            public string name;
            [Tooltip("Animation play speed")]
            public float speed = 1.0f;
            [Tooltip("Head look at angle scale during animation")]
            public float headTurnScale = 1.0f;

            public AnimationInfo(string name)
            {
                this.name = name;
            }
        }

        // the data for each animation turnaround
        [SerializeField, Tooltip("Configuration for run 180 left turn animation")]
        AnimationInfo m_RunLeftTurn = new AnimationInfo("RunForwardTurnLeft180");
        
        [SerializeField, Tooltip("Configuration for run 180 right turn animation")]
        AnimationInfo m_RunRightTurn = new AnimationInfo("RunForwardTurnRight180_Mirror");
        
        [SerializeField, Tooltip("Configuration for sprint 180 left turn animation")]
        AnimationInfo m_SprintLeftTurn = new AnimationInfo("RunForwardTurnLeft180");
        
        [SerializeField, Tooltip("Configuration for sprint 180 right turn animation")]
        AnimationInfo m_SprintRightTurn = new AnimationInfo("RunForwardTurnRight180_Mirror");
        
        [SerializeField, Tooltip("Configuration for idle 180 left turn animation")]
        AnimationInfo m_IdleLeftTurn = new AnimationInfo("IdleTurnLeft180");
        
        [SerializeField, Tooltip("Configuration for idle 180 right turn animation")]
        AnimationInfo m_IdleRightTurn = new AnimationInfo("IdleTurnRight180_Mirror");

        [SerializeField, Tooltip("Curve used to determine rotation during animation")]
        AnimationCurve m_RotationCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

        [SerializeField, Tooltip("Value used to determine if a run turn should be used")]
        float m_NormalizedRunSpeedThreshold = 0.1f;

        [SerializeField, Tooltip("Duration of the cross fade into turn animation")]
        float m_CrossfadeDuration = 0.125f;

        float m_TargetAngle, // target y rotation angle in degrees
            m_CachedAnimatorSpeed, // speed of the animator prior to starting an animation turnaround
            m_CacheForwardSpeed; // forwards speed of the motor prior to starting an animation turnaround
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

        Animator animator
        {
            get { return m_ThirdPersonBrain.animator; }
        }

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

            var rotationProgress = m_RotationCurve.Evaluate(normalizedTime);
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

        /// <summary>
        /// Determines which animation should be played
        /// </summary>
        /// <param name="forwardSpeed">Character's normalized forward speed</param>
        /// <param name="turningClockwise">Is the character turning clockwise</param>
        /// <param name="leftFootPlanted">Is the character's left foot currently planted</param>
        /// <returns>The determined AnimationInfo</returns>
        AnimationInfo GetCurrent(float forwardSpeed, bool turningClockwise, bool leftFootPlanted)
        {
            // idle turn
            if (forwardSpeed < m_NormalizedRunSpeedThreshold)
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

        /// <summary>
        /// Determines if the run or sprint AnimationInfo should be selected
        /// </summary>
        /// <param name="forwardSpeed">Character's normalized forward speed</param>
        /// <param name="turningClockwise">Is the character turning clockwise</param>
        /// <returns>The determined AnimationInfo</returns>
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
        [SerializeField, Tooltip("Should the turnaround be configured or just use defaults?")]
        bool m_ConfigureBlendspace;

        [SerializeField, Tooltip("Configuration settings of the turnaround")]
        BlendspaceProperties m_Configuration;

        bool m_IsSmallTurn;
        float m_TurningTime;
        float m_CurrentForwardSpeed;
        float m_CurrentTurningSpeed;
        float m_TargetAngle;

        Vector3 m_StartRotation;
        Vector3 m_MovementVector;

        ThirdPersonBrain m_ThirdPersonBrain;
        Transform m_Transform;

        // defaults used if configureBlendspace is false
        const float k_DefaultTurnTime = 0.2f, k_DefaultHeadTurnScale = 1.0f;
        float m_DefaultTurnClassificationAngle = 150.0f;
        AnimationCurve m_DefaultRotationCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        AnimationCurve m_DefaultForwardCurve = AnimationCurve.Linear(0.0f, 0.1f, 1.0f, 0.1f);
        AnimationCurve m_DefaultTurn180MovementCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);
        AnimationCurve m_DefaultTurn90MovementCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

        float timeToTurn
        {
            get { return m_ConfigureBlendspace ? m_Configuration.turnTime : k_DefaultTurnTime; }
        }

        AnimationCurve forwardSpeed
        {
            get { return m_ConfigureBlendspace ? m_Configuration.forwardSpeedOverTime : m_DefaultForwardCurve; }
        }

        BlendspaceCalculation forwardSpeedCalculation
        {
            get { return m_ConfigureBlendspace ? m_Configuration.forwardSpeedCalc : BlendspaceCalculation.Additive; }
        }

        float classificationAngle
        {
            get { return m_ConfigureBlendspace ? m_Configuration.classificationAngle : m_DefaultTurnClassificationAngle; }
        }

        AnimationCurve turnMovementOverTime
        {
            get
            {
                if (m_IsSmallTurn)
                {
                    return m_ConfigureBlendspace ? m_Configuration.turn90MovementOverTime : m_DefaultTurn90MovementCurve;
                }

                return m_ConfigureBlendspace ? m_Configuration.turn180MovementOverTime : m_DefaultTurn180MovementCurve;
            }
        }

        AnimationCurve rotationOverTime
        {
            get { return m_ConfigureBlendspace ? m_Configuration.rotationOverTime : m_DefaultRotationCurve; }
        }

        /// <inheritdoc/>
        public override float headTurnScale
        {
            get { return m_ConfigureBlendspace ? m_Configuration.headTurnScale : k_DefaultHeadTurnScale; }
        }

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
                if (m_TurningTime >= timeToTurn)
                {
                    EndTurnAround();
                }
            }
        }

        /// <inheritdoc/>
        public override Vector3 GetMovement()
        {
            var normalizedTime = m_TurningTime / timeToTurn;
            return m_MovementVector * turnMovementOverTime.Evaluate(normalizedTime) * Time.deltaTime;
        }

        /// <summary>
        /// Updates the <see cref="ThirdPersonBrain"/> with a new forward and turning speed.
        /// </summary>
        protected override void FinishedTurning()
        {
            m_TurningTime = timeToTurn;
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
            m_IsSmallTurn = Mathf.Abs(angle) < classificationAngle;
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

        void EvaluateTurn()
        {
            var normalizedTime = m_TurningTime / timeToTurn;

            var forwardSpeedValue = forwardSpeed.Evaluate(normalizedTime);

            if (forwardSpeedCalculation == BlendspaceCalculation.Multiplicative)
            {
                forwardSpeedValue = forwardSpeedValue * m_CurrentForwardSpeed;
            }
            else
            {
                forwardSpeedValue = forwardSpeedValue + m_CurrentForwardSpeed;
            }

            m_ThirdPersonBrain.UpdateForwardSpeed(forwardSpeedValue, Time.deltaTime);

            var newRotation =
                m_StartRotation + new Vector3(0.0f, rotationOverTime.Evaluate(normalizedTime) * m_TargetAngle, 0.0f);
            m_Transform.rotation = Quaternion.Euler(newRotation);
        }

        /// <summary>
        /// Data class used to store configuration settings used by <see cref="BlendspaceTurnAroundBehaviour"/>.
        /// </summary>
        [Serializable]
        protected class BlendspaceProperties
        {
            [SerializeField, Tooltip("Duration of the turn around")]
            float m_TimeToTurn = 0.2f;

            [SerializeField, Tooltip("Curve used to evaluate rotation throughout turn around")]
            AnimationCurve m_RotationDuringTurn = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

            [SerializeField, Tooltip("Curve used to evaluate forward speed throughout turn around")]
            AnimationCurve m_ForwardSpeed = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

            [SerializeField, Tooltip("Method to apply forward speed during turn around")]
            BlendspaceCalculation m_ForwardSpeedCalculation = BlendspaceCalculation.Multiplicative;

            [SerializeField, Tooltip("An angle less than this is classified as a small turn")]
            float m_TurnClassificationAngle = 150.0f;

            [SerializeField, Tooltip("Curve used to evaluate movement throughout a 180° turn around")]
            AnimationCurve m_MovementDuring180Turn = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

            [SerializeField, Tooltip("Curve used to evaluate movement throughout a 90° turn around")]
            AnimationCurve m_MovementDuring90Turn = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

            [SerializeField, Tooltip("Head look at angle scale during animation")]
            float m_HeadTurnMultiplier = 1.0f;

            /// <summary>
            /// Gets the turn duration in seconds.
            /// </summary>
            public float turnTime
            {
                get { return m_TimeToTurn; }
            }

            /// <summary>
            /// Gets the curve to evaluate forward speed over time.
            /// </summary>
            public AnimationCurve forwardSpeedOverTime
            {
                get { return m_ForwardSpeed; }
            }

            /// <summary>
            /// Gets the method of applying forward speed.
            /// </summary>
            public BlendspaceCalculation forwardSpeedCalc
            {
                get { return m_ForwardSpeedCalculation; }
            }

            /// <summary>
            /// Gets the angle used for small turn classification.
            /// </summary>
            public float classificationAngle
            {
                get { return m_TurnClassificationAngle; }
            }

            /// <summary>
            /// Gets the curve used to evaluate movement throughout a 180° turnaround.
            /// </summary>
            public AnimationCurve turn180MovementOverTime
            {
                get { return m_MovementDuring180Turn; }
            }

            /// <summary>
            /// Gets the curve used to evaluate movement throughout a 90° turnaround.
            /// </summary>
            public AnimationCurve turn90MovementOverTime
            {
                get { return m_MovementDuring90Turn; }
            }

            /// <summary>
            /// Gets the curve used to evaluate rotation over time.
            /// </summary>
            public AnimationCurve rotationOverTime
            {
                get { return m_RotationDuringTurn; }
            }

            /// <summary>
            /// Gets the head turn scale to be applied during a turnaround.
            /// </summary>
            public float headTurnScale
            {
                get { return m_HeadTurnMultiplier; }
            }
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
