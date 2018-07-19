using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Collision info used by the OpenCharacterController.
	/// </summary>
	public struct CharacterCapsuleMoverCollisionInfo
	{
		/// <summary>
		/// The hit info.
		/// </summary>
		public RaycastHit hitInfo;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="newHitInfo">New hit info.</param>
		public CharacterCapsuleMoverCollisionInfo(RaycastHit newHitInfo)
		{
			hitInfo = newHitInfo;
		}
	}
}
