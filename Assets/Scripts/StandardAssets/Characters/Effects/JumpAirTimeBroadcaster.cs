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
        /// List of IDs for movement events
        /// </summary>
        public string[] ids;

        /// <summary>
        /// The current index of the 
        /// </summary>
        int m_CurrentIdIndex = -1;
        
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
            m_CurrentIdIndex = 0;
            PlaySound();
        }
        
        void Landed()
        {
            inJump = false;
            m_CurrentIdIndex = 1;
            PlaySound();

        }

        void PlaySound()
        {
            MovementEvent movementEvent= new MovementEvent();
            movementEvent.id = ids[m_CurrentIdIndex];
			
            OnMoved(movementEvent);
        }
		
        
    }
}