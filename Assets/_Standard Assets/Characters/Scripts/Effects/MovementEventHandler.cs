using System;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Effects.Players;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StandardAssets.Characters.Effects
{
    /// <summary>
    /// Abstract class for handling MovementEvents
    /// </summary>
    [Serializable]
    public abstract class MovementEventHandler
    {
        [SerializeField, Tooltip("List of movement event libraries for different movement zones")]
        MovementEventZoneDefinitionList m_ZonesDefinition;

        // Current movement event library
        MovementEventLibrary m_CurrentMovementEventLibrary;

        // Character brain
        CharacterBrain m_Brain;

        // Current Physic Material
        PhysicMaterial m_PhysicMaterial;

        /// <summary>
        /// Gets the associated character brain
        /// </summary>
        protected CharacterBrain brain { get { return m_Brain; } }

        /// <summary>
        /// Gets whether an effect can be played
        /// </summary>
        protected bool canPlayEffect { get { return m_CurrentMovementEventLibrary != null; } }
        
        /// <summary>
        /// Gets the default movement event library
        /// </summary>
        protected MovementEventLibrary defaultLibrary
        {
            get
            {
                var configuration = LevelMovementZoneManager.config;
                if (configuration != null)
                {
                    var library = m_ZonesDefinition[configuration.defaultPhysicMaterial];
                    if (library != null)
                    {
                        return library;
                    }

                    return configuration.defaultLibrary;
                }

                return null;
            }
        }

        /// <summary>
        /// Sets the current event library to the starting event library
        /// </summary>
        protected void Init(CharacterBrain brainToUse)
        {
            m_Brain = brainToUse;
            SetCurrentMovementEventLibrary(defaultLibrary);
        }

        /// <summary>
        /// Helper for playing left foot movement effect
        /// </summary>
        protected void PlayLeftFoot(MovementEventData data)
        {
            if (canPlayEffect)
            {
                m_CurrentMovementEventLibrary.PlayLeftFoot(data);
            }
        }

        /// <summary>
        /// Helper for playing the right foot movement effect
        /// </summary>
        protected void PlayRightFoot(MovementEventData data)
        {
            if (canPlayEffect)
            {
                m_CurrentMovementEventLibrary.PlayRightFoot(data);
            }
        }

        /// <summary>
        /// Helper for playing the landing movement effect
        /// </summary>
        protected void PlayLanding(MovementEventData data)
        {
            if (canPlayEffect)
            {
                m_CurrentMovementEventLibrary.PlayLanding(data);
            }
        }

        /// <summary>
        /// Helper for playing the jumping movement effect
        /// </summary>
        protected void PlayJumping(MovementEventData data)
        {
            if (canPlayEffect)
            {
                m_CurrentMovementEventLibrary.PlayJumping(data);
            }
        }

        /// <summary>
        /// Helper for setting the current Physic Material, calls ChangeMovementZone if the Physic Material is different  
        /// </summary>
        protected void SetPhysicMaterial(PhysicMaterial physicMaterial)
        {
            if (m_PhysicMaterial != physicMaterial)
            {
                ChangeMovementZone(physicMaterial);
            }

            m_PhysicMaterial = physicMaterial;
        }

        // Sets the current MovementEventLibrary
        //      newMovementEventLibrary: Movement event library data
        void SetCurrentMovementEventLibrary(MovementEventLibrary newMovementEventLibrary)
        {
            m_CurrentMovementEventLibrary = newMovementEventLibrary;
        }
        
        // Changes the movement event library based on the Physic Material
        void ChangeMovementZone(PhysicMaterial physicMaterial)
        {
            var library = m_ZonesDefinition[physicMaterial];

            if (library != null)
            {
                SetCurrentMovementEventLibrary(library);
                return;
            }

            var configuration = LevelMovementZoneManager.config;

            if (configuration != null)
            {
                library = configuration[physicMaterial];
                if (library != null)
                {
                    SetCurrentMovementEventLibrary(library);
                    return;
                }
            }

            if (defaultLibrary != null)
            {
                SetCurrentMovementEventLibrary(defaultLibrary);
            }
        }        
    }

    /// <summary>
    /// Container of data associated with a movement event
    /// </summary>
    public struct MovementEventData
    {
        // Where the event is fired from
        Transform m_FiredFrom;

        // Normalized speed of the event
        float m_NormalizedSpeed;

        /// <summary>
        /// Where the event was fired from
        /// </summary>
        public Transform firedFrom { get { return m_FiredFrom; } }

        /// <summary>
        /// The velocity that the effect occurs at
        /// </summary>
        public float normalizedSpeed
        {
            get { return m_NormalizedSpeed; }
            set { m_NormalizedSpeed = value; }
        }

        /// <summary>
        /// Constructs an instance of struct
        /// </summary>
        /// <param name="firedFromTransform">the transform of the emission of the movement - optional, default is null</param>
        /// <param name="normalizedSpeedToUse">the normalized speed of the movement - optional, default is 0</param>
        public MovementEventData(Transform firedFromTransform = null, float normalizedSpeedToUse = 0f)
        {
            m_FiredFrom = firedFromTransform;
            m_NormalizedSpeed = normalizedSpeedToUse;
        }
    }

    /// <summary>
    /// A library of movement effects.
    /// This is what would be swapped out for different zones.
    /// e.g. walking on dirt versus walking on metal.
    /// </summary>
    [Serializable]
    public class MovementEventLibrary
    {
        [SerializeField, Tooltip("A list of Movement Event Players to trigger on a left foot step")]
        MovementEventPlayer[] m_LeftFootEffects;

        [SerializeField, Tooltip("A list of Movement Event Players to trigger on a right foot step")]
        MovementEventPlayer[] m_RightFootEffects;

        [SerializeField, Tooltip("A list of Movement Event Players to trigger on landing")]
        MovementEventPlayer[] m_LandingEffects;

        [SerializeField, Tooltip("A list of Movement Event Players to trigger on jumping")]
        MovementEventPlayer[] m_JumpingEffects;

        // Cache of the various instances so that the prefab is only spawned once
        MovementEventPlayer[] m_LeftFootStepInstances;
        MovementEventPlayer[] m_RightFootStepInstances;
        MovementEventPlayer[] m_LandingInstances;
        MovementEventPlayer[] m_JumpingInstances;

        /// <summary>
        /// Helper for playing the Left Foot movement event
        /// </summary>
        /// <param name="movementEventData">The data relating to the movement event</param>
        public void PlayLeftFoot(MovementEventData movementEventData)
        {
            PlayInstancedEvent(movementEventData, m_LeftFootEffects, ref m_LeftFootStepInstances);
        }

        /// <summary>
        /// Helper for playing the Right Foot movement event
        /// </summary>
        /// <param name="movementEventData">The data relating to the movement event</param>
        public void PlayRightFoot(MovementEventData movementEventData)
        {
            PlayInstancedEvent(movementEventData, m_RightFootEffects, ref m_RightFootStepInstances);
        }

        /// <summary>
        /// Helper for playing the Landing movement event
        /// </summary>
        /// <param name="movementEventData">The data relating to the movement event</param>
        public void PlayLanding(MovementEventData movementEventData)
        {
            PlayInstancedEvent(movementEventData, m_LandingEffects, ref m_LandingInstances);
        }

        /// <summary>
        /// Helper for playing the Jumping movement event
        /// </summary>
        /// <param name="movementEventData">The data relating to the movement event</param>
        public void PlayJumping(MovementEventData movementEventData)
        {
            PlayInstancedEvent(movementEventData, m_JumpingEffects, ref m_JumpingInstances);
        }

        // Helper function for ensuring that the MovementEventPlayer prefab is only instantiated once and the cached version is then used
        //      movementEventData: The data relating to the movement event
        //      prefab: The prefab to instantiate, if it is not cached
        //      instance: The cached instance of the prefab - this could be null and therefore the keyword ref is required
        void PlayInstancedEvent(MovementEventData movementEventData, MovementEventPlayer[] prefabs, ref MovementEventPlayer[] instances)
        {
            if (prefabs == null || prefabs.Length == 0)
            {
                return;
            }

            var arrayLength = prefabs.Length;
            if (instances == null || instances.Length == 0)
            {
                instances = new MovementEventPlayer[arrayLength];

                for (var i = 0; i < arrayLength; i++)
                {
                    instances[i] = Object.Instantiate(prefabs[i]);
                }
            }
           
            for (int i = 0; i < arrayLength; i++)
            {
                var instance = instances[i];
                if (instance == null)
                {
                    var player = Object.Instantiate(prefabs[i]);
                    instances[i] =  player;
                    instance = player;
                    player.Play(movementEventData);               
                }
                
                instance.Play(movementEventData); 
            }
        }        
    }

    /// <summary>
    /// Defines which zone ID matches to which <see cref="MovementEventLibrary"/>
    /// </summary>
    [Serializable]
    public class MovementEventZoneDefinition
    {
        [SerializeField, Tooltip("The ID of the zone used to play the effect")]
        PhysicMaterial m_PhysicMaterial;

        [SerializeField]
        MovementEventLibrary m_ZoneLibrary;

        /// <summary>
        /// Gets the zoneId
        /// </summary>
        public PhysicMaterial physicMaterial { get { return m_PhysicMaterial; } }

        /// <summary>
        /// Gets the <see cref="MovementEventLibrary"/>
        /// </summary>
        public MovementEventLibrary library { get { return m_ZoneLibrary; } }
    }

    /// <summary>
    /// A set of <see cref="MovementEventLibrary"/> for different zone IDs
    /// </summary>
    [Serializable]
    public class MovementEventZoneDefinitionList
    {
        [SerializeField, Tooltip("List of movement event libraries for different movement zones")]
        MovementEventZoneDefinition[] m_MovementZoneLibraries;

        /// <summary>
        /// Indexer to return a <see cref="MovementEventLibrary"/> based on a PhysicMaterial
        /// </summary>
        /// <param name="physicMaterial">PhysicMaterial index</param>
        public MovementEventLibrary this[PhysicMaterial physicMaterial]
        {
            get
            {
                if (physicMaterial == null)
                {
                    return null;
                }

                foreach (var movementEventZoneDefinition in m_MovementZoneLibraries)
                {
                    if (movementEventZoneDefinition.physicMaterial == physicMaterial)
                    {
                        return movementEventZoneDefinition.library;
                    }
                }

                return null;
            }
        }
    }
}
