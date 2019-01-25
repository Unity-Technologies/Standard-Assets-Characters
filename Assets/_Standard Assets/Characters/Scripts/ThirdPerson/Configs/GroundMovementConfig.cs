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
        enum GroundMovement
        {
            RootMotion,
            SpecifiedSpeed
        }

        [Header("Ground Motion")]
        [SerializeField, Tooltip("What type of ground movement should this state use?")]
        GroundMovement m_GroundMovementType = GroundMovement.RootMotion;
		
        [SerializeField, Tooltip("Root motion will be scaled by this before movement is applied.")]
        float m_RootMotionScale = 1f;
        
        [SerializeField, Tooltip("Curve that defines ground movement speed relative to input.")] 
        AnimationCurve m_MaxSpeed = AnimationCurve.Linear(0f, 0f, 1f, 5f);
        
        [SerializeField, Tooltip("Value to scale speed by while sprinting.")] 
        float m_SprintScale = 1.1f;
        
        [SerializeField, Tooltip("Speed at which max speed can change.")]
        float m_MovementSpeedDelta = 0.025f;

        public float sprintScale
        {
            get { return m_SprintScale; }
        }

        public float rootMotionScale
        {
            get { return m_RootMotionScale; }
        }

        public bool useRootMotion
        {
            get { return m_GroundMovementType == GroundMovement.RootMotion; }
        }

        /// <summary>
        /// Gets the value used to interpolate max speed.
        /// </summary>
        public float movementSpeedDelta
        {
            get { return m_MovementSpeedDelta; }
        }

        /// <summary>
        /// Gets the curve that defines ground movement speed based on input.
        /// </summary>
        public AnimationCurve maxSpeed
        {
            get { return m_MaxSpeed; }
        }
    }
}