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

        // COMMENT TODO
        MovementEventLibrary m_CurrentMovementEventLibrary;

        // COMMENT TODO
        CharacterBrain m_Brain;

        // COMMENT TODO
        PhysicMaterial m_PhysicMaterial;

        /// <summary>
        /// COMMENT TODO
        /// </summary>
        protected CharacterBrain brain { get { return m_Brain; } }

        /// <summary>
        /// COMMENT TODO
        /// </summary>
        protected bool canPlayEffect { get { return m_CurrentMovementEventLibrary != null; } }
        
        /// <summary>
        /// COMMENT TODO
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
        /// COMMENT TODO
        /// </summary>
        protected virtual void PlayLeftFoot(MovementEventData data)
        {
            if (canPlayEffect)
            {
                m_CurrentMovementEventLibrary.PlayLeftFoot(data);
            }
        }

        /// <summary>
        /// COMMENT TODO
        /// </summary>
        protected virtual void PlayRightFoot(MovementEventData data)
        {
            if (canPlayEffect)
            {
                m_CurrentMovementEventLibrary.PlayRightFoot(data);
            }
        }

        /// <summary>
        /// COMMENT TODO
        /// </summary>
        protected virtual void PlayLanding(MovementEventData data)
        {
            if (canPlayEffect)
            {
                m_CurrentMovementEventLibrary.PlayLanding(data);
            }
        }

        /// <summary>
        /// COMMENT TODO
        /// </summary>
        protected virtual void PlayJumping(MovementEventData data)
        {
            if (canPlayEffect)
            {
                m_CurrentMovementEventLibrary.PlayJumping(data);
            }
        }

        /// <summary>
        /// COMMENT TODO
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
        
        // COMMENT TODO
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
        // COMMENT TODO
        Transform m_FiredFrom;

        // COMMENT TODO
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
        [SerializeField, Tooltip("The movement event player prefab for handling left foot step")]
        MovementEventPlayer[] m_LeftFootStepPrefabs;

        [SerializeField, Tooltip("The movement event player prefab for handling right foot step")]
        MovementEventPlayer[] m_RightFootStepPrefabs;

        [SerializeField, Tooltip("The movement event player prefab for handling landing")]
        MovementEventPlayer[] m_LandingPrefabs;

        [SerializeField, Tooltip("The movement event player prefab for handling jumping")]
        MovementEventPlayer[] m_JumpingPrefabs;

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
            PlayInstancedEvent(movementEventData, m_LeftFootStepPrefabs, ref m_LeftFootStepInstances);
        }

        /// <summary>
        /// Helper for playing the Right Foot movement event
        /// </summary>
        /// <param name="movementEventData">The data relating to the movement event</param>
        public void PlayRightFoot(MovementEventData movementEventData)
        {
            PlayInstancedEvent(movementEventData, m_RightFootStepPrefabs, ref m_RightFootStepInstances);
        }

        /// <summary>
        /// Helper for playing the Landing movement event
        /// </summary>
        /// <param name="movementEventData">The data relating to the movement event</param>
        public void PlayLanding(MovementEventData movementEventData)
        {
            PlayInstancedEvent(movementEventData, m_LandingPrefabs, ref m_LandingInstances);
        }

        /// <summary>
        /// Helper for playing the Jumping movement event
        /// </summary>
        /// <param name="movementEventData">The data relating to the movement event</param>
        public void PlayJumping(MovementEventData movementEventData)
        {
            PlayInstancedEvent(movementEventData, m_JumpingPrefabs, ref m_JumpingInstances);
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

            if (instances == null || instances.Length == 0)
            {
                instances = new MovementEventPlayer[prefabs.Length];
                var i = -1;
                foreach (var movementEventPlayer in prefabs)
                {
                    i++;
                    instances[i] = Object.Instantiate(movementEventPlayer);
                }
            }

            var j = -1;
            foreach (var movementEventPlayer in instances)
            {
                j++;
                if (movementEventPlayer == null)
                {
                    var player = Object.Instantiate(prefabs[j]);
                    instances[j] =  player;
                    player.Play(movementEventData);               
                }
                else
                {
                    movementEventPlayer.Play(movementEventData);
                }
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

        [SerializeField, Tooltip("The corresponding library of effects")]
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
        // COMMENT TODO
        /// </summary>
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
