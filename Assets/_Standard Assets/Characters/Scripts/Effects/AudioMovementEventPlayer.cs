using StandardAssets.Characters.ThirdPerson;
using UnityEngine;
using System;
using Random = System.Random;

namespace StandardAssets.Characters.Effects
{
	/// <inheritdoc />
	public class AudioMovementEventPlayer : MovementEventPlayer
		{
			/// <summary>
			/// The audio source to be played
			/// </summary>
			[SerializeField]
			protected AudioSource source;
			
			/// <summary>
			/// Audio clips to use for cycling through clips, i.e as foot steps
			/// </summary>
			[SerializeField]
			protected AudioSource[] sources;
			
			/// <summary>
			/// Use a single sound, or cycle clips
			/// </summary>
			[SerializeField]
			protected bool cycleThroughSources = false;

			private int currentSoundIndex;
	
			private void Awake()
			{
				currentSoundIndex = 0;
			}
	
			/// <summary>
			/// Play movement events and scale volume
			/// </summary>
			/// <param name="movementEvent"></param>
			protected override void PlayMovementEvent(MovementEvent movementEvent)
			{
				if (cycleThroughSources && sources!=null)
				{
					sources[currentSoundIndex].Play();

					currentSoundIndex++;
					if (currentSoundIndex >= sources.Length)
					{
						currentSoundIndex = 0;
					}
					return;
				}
				source.Play();
			}
	}
}