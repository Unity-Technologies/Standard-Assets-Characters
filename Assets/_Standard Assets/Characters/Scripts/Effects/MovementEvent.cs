using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Container of data associated with a movement event
	/// </summary>
	public struct MovementEvent
	{
		/// <summary>
		/// Unique identifier for origin of the movement effect. e.g. footstep
		/// </summary>
		public string id;

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
		/// <param name="idToUse">the Identifier of the movement e.g. leftfoot - compulsory</param>
		/// <param name="firedFromTransform">the transform of the emission of the movement - optional, default is null</param>
		/// <param name="normalizedSpeedToUse">the normalized speed of the movement - optional, default is 0</param>
		public MovementEvent(string idToUse, Transform firedFromTransform = null, float normalizedSpeedToUse = 0f)
		{
			id = idToUse;
			firedFrom = firedFromTransform;
			normalizedSpeed = normalizedSpeedToUse;
		}
	}
}