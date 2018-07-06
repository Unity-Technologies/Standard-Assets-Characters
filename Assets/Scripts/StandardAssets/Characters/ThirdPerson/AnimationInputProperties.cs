using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class AnimationInputProperties
	{
		[SerializeField]
		protected float directionInputGain = 0.5f;
		
		[SerializeField]
		protected float directionInputDecay = 10f;
		
		[SerializeField]
		protected float directionInputChangeGain = 5f;
		
		[SerializeField]
		protected float directionInputClamped = 0.5f;
		
		[SerializeField]
		protected float directionInputUnclamped = 1f;

		public float inputGain
		{
			get { return directionInputGain; }
		}

		public float inputDecay
		{
			get { return directionInputDecay; }
		}

		public float inputChangeGain
		{
			get { return directionInputChangeGain; }
		}

		public float inputClamped
		{
			get { return directionInputClamped; }
		}

		public float inputUnclamped
		{
			get { return directionInputUnclamped; }
		}
	}
}