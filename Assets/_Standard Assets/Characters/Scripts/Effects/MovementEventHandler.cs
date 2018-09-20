using System;
using StandardAssets.Characters.Attributes;
using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
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
				LevelMovementZoneConfiguration configuration = LevelMovementZoneManager.config;
				if (configuration != null)
				{
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

		private void ChangeMovementZone(string zoneId)
		{
			MovementEventLibrary library = zonesDefinition[zoneId];

			if (library != null)
			{
				SetCurrentMovementEventLibrary(library);
				return;
			}

			LevelMovementZoneConfiguration configuration = LevelMovementZoneManager.config;

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
}