using System;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Effects.Configs;
using StandardAssets.Characters.Effects.Players;
using UnityEngine;

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
		[SerializeField, Tooltip("List of movement event libraries for different movement zones")]
		protected MovementEventZoneDefinitionList zonesDefinition;

		protected MovementEventLibrary currentMovementEventLibrary;

		protected CharacterBrain brain;

		protected MovementEventLibrary defaultLibrary
		{
			get
			{
				LevelMovementZoneConfig configuration = LevelMovementZoneManager.config;
				if (configuration != null)
				{
					MovementEventLibrary library = zonesDefinition[configuration.defaultId];
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
			get { return currentMovementEventLibrary != null; }
		}

		/// <summary>
		/// Sets the current <see cref="MovementEventLibrary"/>
		/// </summary>
		/// <param name="newMovementEventLibrary">Movement event library data</param>
		public void SetCurrentMovementEventLibrary(MovementEventLibrary newMovementEventLibrary)
		{
			currentMovementEventLibrary = newMovementEventLibrary;
		}

		/// <summary>
		/// Sets the current event library to the starting event library
		/// </summary>
		public virtual void Init(CharacterBrain brainToUse)
		{
			brain = brainToUse;
			brain.changeMovementZone += ChangeMovementZone;
			SetCurrentMovementEventLibrary(defaultLibrary);
		}

		private void ChangeMovementZone(MovementZoneId? zoneId)
		{
			MovementEventLibrary library = zonesDefinition[zoneId];

			if (library != null)
			{
				SetCurrentMovementEventLibrary(library);
				return;
			}

			LevelMovementZoneConfig configuration = LevelMovementZoneManager.config;

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
				currentMovementEventLibrary.PlayLeftFoot(data);
			}
		}

		protected virtual void PlayRightFoot(MovementEventData data)
		{
			if (canPlayEffect)
			{
				currentMovementEventLibrary.PlayRightFoot(data);
			}
		}

		protected virtual void PlayLanding(MovementEventData data)
		{
			if (canPlayEffect)
			{
				currentMovementEventLibrary.PlayLanding(data);
			}
		}

		protected virtual void PlayJumping(MovementEventData data)
		{
			if (canPlayEffect)
			{
				currentMovementEventLibrary.PlayJumping(data);
			}
		}
	}
	
	/// <summary>
	/// Container of data associated with a movement event
	/// </summary>
	public struct MovementEventData
	{
		/// <summary>
		/// Where the event was fired from
		/// </summary>
		public Transform firedFrom;

		/// <summary>
		/// The velocity that the effect occurs at
		/// </summary>
		public float normalizedSpeed;

		/// <summary>
		/// Constructs an instance of struct
		/// </summary>
		/// <param name="firedFromTransform">the transform of the emission of the movement - optional, default is null</param>
		/// <param name="normalizedSpeedToUse">the normalized speed of the movement - optional, default is 0</param>
		public MovementEventData(Transform firedFromTransform = null, float normalizedSpeedToUse = 0f)
		{
			firedFrom = firedFromTransform;
			normalizedSpeed = normalizedSpeedToUse;
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
		protected MovementEventPlayer leftFootStepPrefab;
		
		[SerializeField, Tooltip("The movement event player prefab for handling right foot step")]
		protected MovementEventPlayer rightFootStepPrefab;

		[SerializeField, Tooltip("The movement event player prefab for handling landing")]
		protected MovementEventPlayer landingPrefab;

		[SerializeField, Tooltip("The movement event player prefab for handling jumping")]
		protected MovementEventPlayer jumpingPrefab;

		/// <summary>
		/// Cache of the various instances so that the prefab is only spawned once
		/// </summary>
		protected MovementEventPlayer leftFootStepInstance, rightFootStepInstance, landingInstance, jumpingInstance;

		/// <summary>
		/// Helper function for ensuring that the <see cref="MovementEventPlayer"/> prefab is only instantiated once and the cached version is then used
		/// </summary>
		/// <param name="movementEventData">The data relating to the movement event</param>
		/// <param name="prefab">The prefab to instantiate, if it is not cached</param>
		/// <param name="instance">The cached instance of the prefab - this could be null and therefore the keyword ref is required</param>
		protected void PlayInstancedEvent(MovementEventData movementEventData, MovementEventPlayer prefab, ref MovementEventPlayer instance)
		{
			if (prefab == null)
			{
				return;
			}

			if (instance == null)
			{
				instance = GameObject.Instantiate(prefab);
			}
					
			instance.Play(movementEventData);
		}

		/// <summary>
		/// Helper for playing the Left Foot movement event
		/// </summary>
		/// <param name="movementEventData">The data relating to the movement event</param>
		public void PlayLeftFoot(MovementEventData movementEventData)
		{
			PlayInstancedEvent(movementEventData, leftFootStepPrefab, ref leftFootStepInstance);
		}
		
		/// <summary>
		/// Helper for playing the Right Foot movement event
		/// </summary>
		/// <param name="movementEventData">The data relating to the movement event</param>
		public void PlayRightFoot(MovementEventData movementEventData)
		{
			PlayInstancedEvent(movementEventData, rightFootStepPrefab, ref rightFootStepInstance);
		}
		
		/// <summary>
		/// Helper for playing the Landing movement event
		/// </summary>
		/// <param name="movementEventData">The data relating to the movement event</param>
		public void PlayLanding(MovementEventData movementEventData)
		{
			PlayInstancedEvent(movementEventData, landingPrefab, ref landingInstance);
		}
		
		/// <summary>
		/// Helper for playing the Jumping movement event
		/// </summary>
		/// <param name="movementEventData">The data relating to the movement event</param>
		public void PlayJumping(MovementEventData movementEventData)
		{
			PlayInstancedEvent(movementEventData, jumpingPrefab, ref jumpingInstance);
		}
	}
	
	/// <summary>
	/// Defines which zone ID matches to which <see cref="MovementEventLibrary"/>
	/// </summary>
	[Serializable]
	public class MovementEventZoneDefinition
	{
		[SerializeField, Tooltip("The ID of the zone used to play the effect")]
		protected MovementZoneId zoneId;

		[SerializeField, Tooltip("The corresponding library of effects")]
		protected MovementEventLibrary zoneLibrary;
		
		/// <summary>
		/// Gets the zoneId
		/// </summary>
		public MovementZoneId id
		{
			get { return zoneId; }
		}

		/// <summary>
		/// Gets the <see cref="MovementEventLibrary"/>
		/// </summary>
		public MovementEventLibrary library
		{
			get { return zoneLibrary; }
		}
	}
	
	/// <summary>
	/// A set of <see cref="MovementEventLibrary"/> for different zone IDs
	/// </summary>
	[Serializable]
	public class MovementEventZoneDefinitionList
	{
		[SerializeField, Tooltip("List of movement event libraries for different movement zones")]
		protected MovementEventZoneDefinition[] movementZoneLibraries;
		
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
				
				foreach (MovementEventZoneDefinition movementEventZoneDefinition in movementZoneLibraries)
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