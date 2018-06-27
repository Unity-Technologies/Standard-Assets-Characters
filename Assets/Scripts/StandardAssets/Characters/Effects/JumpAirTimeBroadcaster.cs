using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
    /// <summary>
    /// Broadcasts an event with an id selected from jump or landing 
    /// </summary>
    [RequireComponent(typeof(ICharacterPhysics))]
    public class JumpAirTimeBroadcaster : MovementEventBroadcaster
    {
        /// <summary>
        /// Id of Jumping event
        /// </summary>
        [SerializeField]
        protected string jumpId = "jumping";

        /// <summary>
        /// Id of Landing event
        /// </summary>
        [SerializeField]
        protected string landingId = "landing";
        
        /// <summary>
        /// CharacterPhysics
        /// </summary>
        private ICharacterPhysics characterPhysics;
        
        /// <summary>
        /// Is the jump in motion 
        /// </summary>
        private bool inJump;

        private void Awake()
        {
            characterPhysics = GetComponent<ICharacterPhysics>();
        }

        /// <summary>
        /// Subscribe
        /// </summary>
        private void OnEnable()
        {
            characterPhysics.landed += Landed;
            characterPhysics.jumpVelocitySet += Jumped;
        }

        /// <summary>
        /// Unsubscribe
        /// </summary>
        private void OnDisable()
        {
            characterPhysics.landed -= Landed;
            characterPhysics.jumpVelocitySet -= Jumped;
        }

        /// <summary>
        /// Calls PlayEvent on the jump ID
        /// </summary>
        void Jumped()
        {
            BroadcastMovementEvent(jumpId);
        }
        
        /// <summary>
        /// Calls PlayEvent on the landing ID
        /// </summary>
        void Landed()
        {
            BroadcastMovementEvent(landingId);
        }        
    }
}