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
			protected AudioClip[] sources;
			
			/// <summary>
			/// Use a single sound, or cycle clips
			/// </summary>
			[SerializeField]
			protected bool cycleThroughSources = false;
	
			
	
			private Random rand;
	
			private void Awake()
			{
				rand = new Random();
				
				
			}
	
			/// <inheritdoc />
			protected override void PlayMovementEvent(MovementEvent movementEvent)
			{
				if (cycleThroughSources && sources!=null)
				{
					source.clip = sources[rand.Next(0, sources.Length)];
					
				}

				Debug.Log("Source Play");
				source.Play();
				
	
			}
		
	}
}