using Cinemachine;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CinemachineImpulseMovementEventPlayer : NormalizedSpeedMovementEventPlayer
    {
        [SerializeField] 
        protected float minAmplitude = 0.5f;

        [SerializeField] 
        protected float maxAmplitude = 1f;

        protected override float minValue
        {
            get { return minAmplitude; }
        }

        protected override float maxValue
        {
            get { return maxAmplitude; }
        }

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

        private CinemachineImpulseSource impulseSource;

        protected override void PlayMovementEvent(MovementEvent movementEvent, float effectMagnitude)
        {
            source.GenerateImpulse(effectMagnitude * Vector3.one);
        }
    }
}