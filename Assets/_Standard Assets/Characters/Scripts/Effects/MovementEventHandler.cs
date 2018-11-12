using System;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Effects.Players;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace StandardAssets.Characters.Effects
{
    /// <summary>
    /// Enum for representing the different type of movement zones
    /// </summary>
    public enum MovementZoneId
    {
        Concrete,
        Metal,
        Grass,
        Gravel
    }

    /// <summary>
    /// Abstract class for handling MovementEvents
    /// </summary>
    [Serializable]
    public abstract class MovementEventHandler
    {
        [FormerlySerializedAs("zonesDefinition")]
        [SerializeField, Tooltip("List of movement event libraries for different movement zones")]
        MovementEventZoneDefinitionList m_ZonesDefinition;

        MovementEventLibrary m_CurrentMovementEventLibrary;

        CharacterBrain m_Brain;

        protected CharacterBrain brain
        {
            get { return m_Brain; }
        }

        protected MovementEventLibrary defaultLibrary
        {
            get
            {
                var configuration = LevelMovementZoneManager.config;
                if (configuration != null)
                {
                    var library = m_ZonesDefinition[configuration.defaultId];
                    if (library != null)
                    {
                        return library;
                    }

                    return configuration.defaultLibrary;
                }

                return null;
            }
        }

        protected bool canPlayEffect
        {
            get { return m_CurrentMovementEventLibrary != null; }
        }

        /// <summary>
        /// Sets the current <see cref="MovementEventLibrary"/>
        /// </summary>
        /// <param name="newMovementEventLibrary">Movement event library data</param>
        public void SetCurrentMovementEventLibrary(MovementEventLibrary newMovementEventLibrary)
        {
            m_CurrentMovementEventLibrary = newMovementEventLibrary;
        }

        /// <summary>
        /// Sets the current event library to the starting event library
        /// </summary>
        public void Init(CharacterBrain brainToUse)
        {
            m_Brain = brainToUse;
            m_Brain.changeMovementZone += ChangeMovementZone;
            SetCurrentMovementEventLibrary(defaultLibrary);
        }

        void ChangeMovementZone(MovementZoneId? zoneId)
        {
            var library = m_ZonesDefinition[zoneId];

            if (library != null)
            {
                SetCurrentMovementEventLibrary(library);
                return;
            }

            var configuration = LevelMovementZoneManager.config;

            if (configuration != null)
            {
                library = configuration[zoneId];
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

        protected virtual void PlayLeftFoot(MovementEventData data)
        {
            if (canPlayEffect)
            {
                m_CurrentMovementEventLibrary.PlayLeftFoot(data);
            }
        }

        protected virtual void PlayRightFoot(MovementEventData data)
        {
            if (canPlayEffect)
            {
                m_CurrentMovementEventLibrary.PlayRightFoot(data);
            }
        }

        protected virtual void PlayLanding(MovementEventData data)
        {
            if (canPlayEffect)
            {
                m_CurrentMovementEventLibrary.PlayLanding(data);
            }
        }

        protected virtual void PlayJumping(MovementEventData data)
        {
            if (canPlayEffect)
            {
                m_CurrentMovementEventLibrary.PlayJumping(data);
            }
        }
    }

    /// <summary>
    /// Container of data associated with a movement event
    /// </summary>
    public struct MovementEventData
    {
        Transform m_FiredFrom;

        float m_NormalizedSpeed;

        /// <summary>
        /// Where the event was fired from
        /// </summary>
        public Transform firedFrom
        {
            get { return m_FiredFrom; }
        }

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
        [FormerlySerializedAs("leftFootStepPrefab")]
        [SerializeField, Tooltip("The movement event player prefab for handling left foot step")]
        MovementEventPlayer m_LeftFootStepPrefab;

        [FormerlySerializedAs("rightFootStepPrefab")]
        [SerializeField, Tooltip("The movement event player prefab for handling right foot step")]
        MovementEventPlayer m_RightFootStepPrefab;

        [FormerlySerializedAs("landingPrefab")]
        [SerializeField, Tooltip("The movement event player prefab for handling landing")]
        MovementEventPlayer m_LandingPrefab;

        [FormerlySerializedAs("jumpingPrefab")]
        [SerializeField, Tooltip("The movement event player prefab for handling jumping")]
        MovementEventPlayer m_JumpingPrefab;

        /// <summary>
        /// Cache of the various instances so that the prefab is only spawned once
        /// </summary>
        MovementEventPlayer m_LeftFootStepInstance, m_RightFootStepInstance, m_LandingInstance, m_JumpingInstance;

        /// <summary>
        /// Helper function for ensuring that the <see cref="MovementEventPlayer"/> prefab is only instantiated once and the cached version is then used
        /// </summary>
        /// <param name="movementEventData">The data relating to the movement event</param>
        /// <param name="prefab">The prefab to instantiate, if it is not cached</param>
        /// <param name="instance">The cached instance of the prefab - this could be null and therefore the keyword ref is required</param>
        void PlayInstancedEvent(MovementEventData movementEventData, MovementEventPlayer prefab, ref MovementEventPlayer instance)
        {
            if (prefab == null)
            {
                return;
            }

            if (instance == null)
            {
                instance = Object.Instantiate(prefab);
            }

            instance.Play(movementEventData);
        }

        /// <summary>
        /// Helper for playing the Left Foot movement event
        /// </summary>
        /// <param name="movementEventData">The data relating to the movement event</param>
        public void PlayLeftFoot(MovementEventData movementEventData)
        {
            PlayInstancedEvent(movementEventData, m_LeftFootStepPrefab, ref m_LeftFootStepInstance);
        }

        /// <summary>
        /// Helper for playing the Right Foot movement event
        /// </summary>
        /// <param name="movementEventData">The data relating to the movement event</param>
        public void PlayRightFoot(MovementEventData movementEventData)
        {
            PlayInstancedEvent(movementEventData, m_RightFootStepPrefab, ref m_RightFootStepInstance);
        }

        /// <summary>
        /// Helper for playing the Landing movement event
        /// </summary>
        /// <param name="movementEventData">The data relating to the movement event</param>
        public void PlayLanding(MovementEventData movementEventData)
        {
            PlayInstancedEvent(movementEventData, m_LandingPrefab, ref m_LandingInstance);
        }

        /// <summary>
        /// Helper for playing the Jumping movement event
        /// </summary>
        /// <param name="movementEventData">The data relating to the movement event</param>
        public void PlayJumping(MovementEventData movementEventData)
        {
            PlayInstancedEvent(movementEventData, m_JumpingPrefab, ref m_JumpingInstance);
        }
    }

    /// <summary>
    /// Defines which zone ID matches to which <see cref="MovementEventLibrary"/>
    /// </summary>
    [Serializable]
    public class MovementEventZoneDefinition
    {
        [FormerlySerializedAs("zoneId")]
        [SerializeField, Tooltip("The ID of the zone used to play the effect")]
        MovementZoneId m_ZoneId;

        [FormerlySerializedAs("zoneLibrary")]
        [SerializeField, Tooltip("The corresponding library of effects")]
        MovementEventLibrary m_ZoneLibrary;

        /// <summary>
        /// Gets the zoneId
        /// </summary>
        public MovementZoneId id
        {
            get { return m_ZoneId; }
        }

        /// <summary>
        /// Gets the <see cref="MovementEventLibrary"/>
        /// </summary>
        public MovementEventLibrary library
        {
            get { return m_ZoneLibrary; }
        }
    }

    /// <summary>
    /// A set of <see cref="MovementEventLibrary"/> for different zone IDs
    /// </summary>
    [Serializable]
    public class MovementEventZoneDefinitionList
    {
        [FormerlySerializedAs("movementZoneLibraries")]
        [SerializeField, Tooltip("List of movement event libraries for different movement zones")]
        MovementEventZoneDefinition[] m_MovementZoneLibraries;

        /// <summary>
        /// Gets the Gets the <see cref="MovementEventLibrary"/> for a specified zoneId for a specified zoneId
        /// </summary>
        /// <param name="zoneId">The zoneId needed to look up the <see cref="MovementEventLibrary"/></param>
        /// <value>Gets the <see cref="MovementEventLibrary"/> for a specified zoneId. returns null if the zoneId does not have an associated <see cref="MovementEventLibrary"/></value>
        public MovementEventLibrary this[MovementZoneId? zoneId]
        {
            get
            {
                if (!zoneId.HasValue)
                {
                    return null;
                }

                foreach (var movementEventZoneDefinition in m_MovementZoneLibraries)
                {
                    if (movementEventZoneDefinition.id == zoneId)
                    {
                        return movementEventZoneDefinition.library;
                    }
                }

                return null;
            }
        }
    }
}
