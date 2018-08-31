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
	
			private Random rand;
	
			private void Awake()
			{
				rand = new Random();
				currentSoundIndex = 0;

			}
	
			/// <inheritdoc />
			protected override void PlayMovementEvent(MovementEvent movementEvent)
			{
				if (cycleThroughSources && sources!=null)
				{
					//source.clip = sources[rand.Next(0, sources.Length)];
					//sources[rand.Next(0, sources.Length)].Play();
					sources[currentSoundIndex++].Play();
					if (currentSoundIndex >= sources.Length)
					{
						currentSoundIndex = 0;
					}

					return;
				}

			//	Debug.Log("Source Play");
				source.Play();
				
	
			}
		
	}
}