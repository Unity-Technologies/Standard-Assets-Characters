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
        public string jumpId = "jumping";

        /// <summary>
        /// Id of Landing event
        /// </summary>
        public string landingId = "landing";
        
        /// <summary>
        /// CharacterPhysics
        /// </summary>
        ICharacterPhysics m_CharacterPhysics;
        
        /// <summary>
        /// Is the jump in motion 
        /// </summary>
        private bool inJump;

        void Awake()
        {
            m_CharacterPhysics = GetComponent<ICharacterPhysics>();
        }

        /// <summary>
        /// Subscribe
        /// </summary>
        private void OnEnable()
        {
            m_CharacterPhysics.landed += Landed;
            m_CharacterPhysics.jumpVelocitySet += Jumped;
        }

        /// <summary>
        /// Unsubscribe
        /// </summary>
        private void OnDisable()
        {
            m_CharacterPhysics.landed -= Landed;
            m_CharacterPhysics.jumpVelocitySet -= Jumped;
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