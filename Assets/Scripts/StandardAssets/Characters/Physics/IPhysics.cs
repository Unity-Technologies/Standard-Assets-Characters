using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// A wrapper for the physics controllers
	/// Can swa
	/// </summary>
	public interface IPhysics
	{
		void Move(Vector3 moveVector3);
		
		bool canJump { get; }
	}
}