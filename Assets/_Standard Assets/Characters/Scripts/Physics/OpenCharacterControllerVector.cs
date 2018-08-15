using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// A vector used by the OpenCharacterController.
	/// </summary>
	public struct OpenCharacterControllerVector
	{
		/// <summary>
		/// The move vector.
		/// Note: This gets used up during the move loop, so will be zero by the end of the loop.
		/// </summary>
		public Vector3 moveVector;

		/// <summary>
		/// Can the movement slide along obstacles?
		/// </summary>
		public bool canSlide;
		
		#if UNITY_EDITOR
		public Vector3 debugOriginalVector;
		#endif

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="newMoveVector">The move vector.</param>
		/// <param name="newCanSlide">Can the movement slide along obstacles?</param>
		public OpenCharacterControllerVector(Vector3 newMoveVector, bool newCanSlide = true)
		{
			moveVector = newMoveVector;
			canSlide = newCanSlide;
			
			#if UNITY_EDITOR
			debugOriginalVector = newMoveVector;
			#endif
		}
	}
}