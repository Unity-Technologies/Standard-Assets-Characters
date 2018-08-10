using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class ActionProperties
	{
		[SerializeField]
		protected int forwardInputSamples = 1;
		
		public int forwardInputWindowSize
		{
			get { return forwardInputSamples; }
		}
	}
}