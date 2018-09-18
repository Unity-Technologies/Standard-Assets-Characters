using System;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.FirstPerson
{
	[Serializable]
	public class FirstPersonMovementEventHandler : DistanceMovementEventHandler
	{
		/// <summary>
		/// Sets the brain to be used
		/// </summary>
		public void Init(FirstPersonBrain brainToUse)
		{
			base.Init(brainToUse);
		}

		/// <summary>
		/// Change the distance that footstep sounds are played
		/// </summary>
		/// <param name="strideLength"></param>
		public void AdjustAudioTriggerThreshold(float strideLength)
		{
			sqrDistanceThreshold = strideLength * strideLength;
		}
	}
}