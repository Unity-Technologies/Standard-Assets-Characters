using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
    /// <summary>
    /// Allows an animation state to change the OpenCharacterController's capsule height and offset.
    /// </summary>
    public class ScaleCharacterColliderState : StateMachineBehaviour
    {
        // values used for data validation.
        const float k_MinHeightScale = 0.0f;
        const float k_MaxHeightScale = 2.0f;
        
        // enum used to determine the way in which time is tracked.
        enum NormalizedMode
        {
            Once,
            Loop,
            PingPong
        }


        [SerializeField, Tooltip("Should the animator's normalized time be used? Otherwise time is tracked manually.")]
        bool m_UseNormalizedTime = true;

        [SerializeField, Tooltip("Curve used for adjusting the collider scale. This curve is unused for looping animations.")]
        AnimationCurve m_ScaleBasedOnNormalizedTime = new AnimationCurve()
        {
            keys = new[]
            {
                new Keyframe(0, 0),
                new Keyframe(1, 1),
            }
        };

        [SerializeField, Tooltip("Normalized time mode")]
        NormalizedMode m_AnimationMode;

        [SerializeField, Tooltip("How many seconds it will take to change the height")]
        float m_Duration = 1.0f;

        [SerializeField, Tooltip("Adjusted character scale")]
        float m_HeightScale = 1.0f;

        [SerializeField, Range(0.0f, 1.0f), Tooltip("Scale character's collider and offset relative to character's height. 0.5 is center")]
        float m_HeightOrigin = 0.5f;

        [SerializeField, Tooltip("If false, we will not restore collider on exit. This allows another state behavior " +
             "to use the last state of the collider")]
        bool m_ResetOnExit = true;

        // Using own time as normalized time seems to be looping.
        float m_Time;

        // The current scale of the collider.
        float m_CurrentScale;

        // The entry scale of the collider.
        float m_EntryScale;

        // The entry offset of the collider.
        float m_EntryOffset;

        // Cached reference of the controller.
        OpenCharacterController m_Controller;


        /// <summary>
        /// Caches a reference to <see cref="OpenCharacterController"/> and the current state of the collider.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_Controller == null)
            {
                m_Controller = animator.GetComponent<OpenCharacterController>();
                if (m_Controller == null)
                {
                    return;
                }
            }

            // If another state does not reset the collider on exit, we can blend collider from it's existing state:
            m_CurrentScale = m_EntryScale = m_Controller.GetHeight() / m_Controller.defaultHeight;
            m_EntryOffset = m_Controller.GetCenter().y;
        }

        /// <summary>
        /// Adjusts the <see cref="OpenCharacterController"/> collider scale and offset.
        /// </summary>
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_Controller == null)
            {
                return;
            }

            if (m_UseNormalizedTime)
            {
                switch (m_AnimationMode)
                {
                    default:
                        m_Time = stateInfo.normalizedTime;
                        break;
                    case NormalizedMode.Loop:
                        m_Time = Mathf.Repeat(stateInfo.normalizedTime, 1.0f);
                        break;
                    case NormalizedMode.PingPong:
                        m_Time = Mathf.PingPong(stateInfo.normalizedTime, 1.0f);
                        break;
                }

                m_CurrentScale = m_ScaleBasedOnNormalizedTime.Evaluate(m_Time);
            }
            else
            {
                m_Time = m_Duration <= 0.0f ? 1.0f : Mathf.Clamp01(m_Time + (Time.deltaTime * (1.0f / m_Duration)));

                m_CurrentScale = Mathf.Lerp(m_EntryScale, m_HeightScale, m_Time);
            }

            var height = m_Controller.ValidateHeight(m_CurrentScale * m_Controller.defaultHeight);

            var offset = m_UseNormalizedTime ? Mathf.Lerp(m_EntryOffset, GetCenter(), 1.0f - m_ScaleBasedOnNormalizedTime.Evaluate(m_Time)) : Mathf.Lerp(m_EntryOffset, GetCenter(), 1.0f - m_Time);

            m_Controller.SetHeightAndCenter(height, new Vector3(0.0f, offset, 0.0f), true, false);
        }

        /// <summary>
        /// Resets the <see cref="OpenCharacterController"/> collider if <see cref="m_ResetOnExit"/> is true;
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            HandleStateExit();
        }

        /// <summary>
        /// Resets the <see cref="OpenCharacterController"/> collider if <see cref="m_ResetOnExit"/> is true;
        /// </summary>
        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            HandleStateExit();
        }

        // Resets the collider if necessary.
        void HandleStateExit()
        {
            if (m_Controller == null)
            {
                return;
            }

            if (m_ResetOnExit)
            {
                m_Controller.ResetHeightAndCenter(true, false);
            }

            m_Time = 0.0f;
        }

        // Validates the height scale and duration.
        void OnValidate()
        {
            m_HeightScale = Mathf.Clamp(m_HeightScale, k_MinHeightScale, k_MaxHeightScale);
            m_Duration = Mathf.Max(0.0f, m_Duration);
        }

        // Gets the center of the collider.
        float GetCenter()
        {
            // collider is centered on character:
            var center = (m_CurrentScale * m_Controller.defaultHeight) * 0.5f;
            var offset = center * (m_HeightOrigin - 0.5f);
            return center + offset;
        }
    }
}
