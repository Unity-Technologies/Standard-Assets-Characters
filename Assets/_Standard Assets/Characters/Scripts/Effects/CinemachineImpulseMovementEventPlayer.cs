using Cinemachine;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
    /// <summary>
    /// Plays the attached <see cref="CinemachineImpulseSource"/> with a velocity based on effect magnitude
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CinemachineImpulseMovementEventPlayer : NormalizedSpeedMovementEventPlayer
    {
        [SerializeField, Tooltip("The amplitude that the impulse will have at low speed")] 
        protected float minAmplitude = 0.5f;

        [SerializeField, Tooltip("The amplitude that the impulse will have at high speed")] 
        protected float maxAmplitude = 1f;

        private CinemachineImpulseSource impulseSource;
        
        /// <summary>
        /// Gets the minAmplitude as the minValue which is used to calculate the effectMagnitude in <see cref="NormalizedSpeedMovementEventPlayer"/>
        /// </summary>
        protected override float minValue
        {
            get { return minAmplitude; }
        }

        /// <summary>
        /// Gets the maxAmplitude as the maxValue which is used to calculate the effectMagnitude in <see cref="NormalizedSpeedMovementEventPlayer"/>
        /// </summary>
        protected override float maxValue
        {
            get { return maxAmplitude; }
        }

        /// <summary>
        /// Gets the attached <see cref="CinemachineImpulseSource"/>
        /// </summary>
        protected CinemachineImpulseSource source
        {
            get
            {
                if (impulseSource == null)
                {
                    impulseSource = GetComponent<CinemachineImpulseSource>();
                }

                return impulseSource;
            }
        }

        /// <summary>
        /// Generates the impulse with the effectMagnitude
        /// </summary>
        /// <param name="movementEvent">The data container of the <see cref="MovementEvent"/>. Unused in this case</param>
        /// <param name="effectMagnitude">The magnitude of the effect</param>
        protected override void PlayMovementEvent(MovementEvent movementEvent, float effectMagnitude)
        {
            source.GenerateImpulse(effectMagnitude * Vector3.one);
        }
    }
}