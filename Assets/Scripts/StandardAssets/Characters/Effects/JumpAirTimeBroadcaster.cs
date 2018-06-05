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
            //TODO: subscribe to events
            //m_CharacterPhysics.landed += Landed;
        }

        private void FixedUpdate()
        {
            CheckJump();
        }
        
        /// <summary>
        /// Checks if the character is jumping
        /// </summary>
        void CheckJump()
        {
            if (!m_CharacterPhysics.isGrounded & !inJump)
            {
                Jumped();
            }

            if (inJump & m_CharacterPhysics.isGrounded)
            {
                Landed();
            }
        }
        
        /// <summary>
        /// Set the IDs index and play sound
        /// </summary>
        void Jumped()
        {
            inJump = true;
            PlaySound(jumpId);
        }
        
        void Landed()
        {
            inJump = false;
            PlaySound(landingId);
        }

        void PlaySound(string id)
        {
            MovementEvent movementEvent= new MovementEvent();
            movementEvent.id = id;
			
            OnMoved(movementEvent);
        }
		
        
    }
}