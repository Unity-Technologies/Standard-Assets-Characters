using StandardAssets.Characters.ThirdPerson;
using UnityEngine;
using System;
using UnityEditor.ShaderGraph;
using Random = System.Random;

namespace StandardAssets.Characters.Effects
{
	/// <inheritdoc />
	public class AudioMovementEventPlayer : MovementEventPlayer
		{
			/// <summary>
			/// The audio source to be played
			/// </summary>
			[SerializeField, Tooltip("When using a single audio source")]
			protected AudioSource source;
			
			/// <summary>
			/// Audio clips to use for cycling through clips, i.e as foot steps
			/// </summary>
			[SerializeField, Tooltip("For using multiple audio sources, i.e footstep sounds")]
			protected AudioSource[] sources;
			
			/// <summary>
			/// Use a single sound, or cycle clips
			/// </summary>
			[SerializeField, Tooltip("Cycle through the sounds")]
			protected bool cycleThroughSources = false;

			private int currentSoundIndex;
	
			private void Awake()
			{
				currentSoundIndex = 0;
			}
	
			/// <summary>
			/// Play movement events
			/// </summary>
			/// <param name="movementEvent">Movement event data</param>
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

				if (source != null)
				{
					source.Play();
				}
			}
		}
}