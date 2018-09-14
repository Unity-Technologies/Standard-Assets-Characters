using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Class used to store the forward input window size during action mode.
	/// </summary>
	[Serializable]
	public class ActionProperties
	{
		[SerializeField]
		protected int forwardInputSamples = 1;
		
		/// <summary>
		/// Gets the forward input window size used to create a moving average.
		/// </summary>
		public int forwardInputWindowSize
		{
			get { return forwardInputSamples; }
		}
	}
}