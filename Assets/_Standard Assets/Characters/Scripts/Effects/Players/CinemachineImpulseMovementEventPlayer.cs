using Cinemachine;
using UnityEngine;

namespace StandardAssets.Characters.Effects.Players
{
    /// <summary>
    /// Plays the attached <see cref="CinemachineImpulseSource"/> with a velocity based on effect magnitude
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CinemachineImpulseMovementEventPlayer : MovementEventPlayer
    {
        [SerializeField, Tooltip("How the volume is scaled based on normalizedSpeed")]
        protected AnimationCurve impulseFromNormalizedSpeed = AnimationCurve.Linear(0f,0.5f,1f,1f);

        private CinemachineImpulseSource impulseSource;

        protected override void PlayMovementEvent(MovementEventData movementEventData, float effectMagnitude)
        {
            LazyLoadSource();
            impulseSource.GenerateImpulse(effectMagnitude * Vector3.one);
        }

        protected override float Evaluate(float normalizedSpeed)
        {
            return impulseFromNormalizedSpeed.Evaluate(normalizedSpeed);
        }

        protected void LazyLoadSource()
        {
            if (impulseSource != null)
            {
                return;
            }
                
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }
    }
}