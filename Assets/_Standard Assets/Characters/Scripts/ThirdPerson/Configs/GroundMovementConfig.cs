using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.Configs
{
    /// <summary>
    /// ScriptableObject containing settings to define non root motion ground movement.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Ground Movement Config",
        menuName = "Standard Assets/Characters/Ground Movement Config", order = 1)]
    public class GroundMovementConfig : ScriptableObject
    {
        enum Type
        {
            RootMotion,
            SpecifiedSpeed
        }

        enum MaxSpeedType
        {
            Float,
            Curve
        }

        [SerializeField, Tooltip("What type of ground movement should this state use?")]
        Type m_MotionType = Type.RootMotion;
		
        [SerializeField, Tooltip("Root motion will be scaled by this before movement is applied.")]
        float m_RootMotionScale = 1f;

        [SerializeField, Tooltip("Type of max speed to use. Float will use a float that has a linear relationship " +
             "to input. Curve allows you to set any curve you like.")]
        MaxSpeedType m_MaxSpeedType;

        [SerializeField, Tooltip("Max speed relative to input.")]
        float m_MaxSpeed = 5.0f;
        
        [SerializeField, Tooltip("Curve that defines ground movement speed relative to input.")] 
        AnimationCurve m_MaxSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 5f);
        
        [SerializeField, Tooltip("Value to scale speed by while sprinting.")] 
        float m_SprintScale = 1.1f;
        
        [SerializeField, Tooltip("Rate at which the max speed will change.")]
        float m_SpeedDelta = 0.025f;

#if UNITY_EDITOR
        public bool isMaxSpeedValue
        {
            get { return m_MaxSpeedType == MaxSpeedType.Float; }
        }
#endif
        
        /// <summary>
        /// Gets the value used to scale movement while sprinting.
        /// </summary>
        public float sprintScale
        {
            get { return m_SprintScale; }
        }

        /// <summary>
        /// Gets the value used to scale root motion before applying the movement to the character.
        /// </summary>
        public float rootMotionScale
        {
            get { return m_RootMotionScale; }
        }

        /// <summary>
        /// Gets whether root motion or a specified speed is used to drive movement.
        /// </summary>
        public bool useRootMotion
        {
            get { return m_MotionType == Type.RootMotion; }
        }

        /// <summary>
        /// Gets the value used to interpolate max speed.
        /// </summary>
        public float speedDelta
        {
            get { return m_SpeedDelta; }
        }

        /// <summary>
        /// Gets the curve that defines ground movement speed based on input.
        /// </summary>
        public AnimationCurve maxSpeed
        {
            get
            {
                return m_MaxSpeedType == MaxSpeedType.Float ? 
                    AnimationCurve.Linear(0.0f, 0.0f, 1.0f, m_MaxSpeed) : m_MaxSpeedCurve;
            }
        }
    }
}