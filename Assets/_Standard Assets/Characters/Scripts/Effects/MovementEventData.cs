using UnityEngine;

namespace StandardAssets.Characters.Effects
{
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
}