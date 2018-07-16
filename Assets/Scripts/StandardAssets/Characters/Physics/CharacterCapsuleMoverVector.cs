using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// A vector used by the CharacterCapsuleMover.
	/// </summary>
	public class CharacterCapsuleMoverVector
	{
		/// <summary>
		/// The move vector.
		/// </summary>
		public Vector3 moveVector;

		/// <summary>
		/// Can the movement slide along obstacles?
		/// </summary>
		public bool canSlide;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="newMoveVector">The move vector.</param>
		/// <param name="newCanSlide">Can the movement slide along obstacles?</param>
		public CharacterCapsuleMoverVector(Vector3 newMoveVector, bool newCanSlide = true)
		{
			moveVector = newMoveVector;
			canSlide = newCanSlide;
		}
	}
}