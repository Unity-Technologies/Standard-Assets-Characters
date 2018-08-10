using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Collision info used by the OpenCharacterController and sent to the OnOpenCharacterControllerHit message.
	/// </summary>
	public struct OpenCharacterControllerCollisionInfo
	{
		/// <summary>
		/// The collider that was hit by the controller.
		/// </summary>
		public readonly Collider collider;

		/// <summary>
		/// The controller that hit the collider.
		/// </summary>
		public readonly OpenCharacterController controller;

		/// <summary>
		/// The game object that was hit by the controller.
		/// </summary>
		public readonly GameObject gameObject;

		/// <summary>
		/// The direction the character Controller was moving in when the collision occured.
		/// </summary>
		public readonly Vector3 moveDirection;

		/// <summary>
		/// How far the character has travelled until it hit the collider.
		/// </summary>
		public readonly float moveLength;

		/// <summary>
		/// The normal of the surface we collided with in world space.
		/// </summary>
		public readonly Vector3 normal;

		/// <summary>
		/// The impact point in world space.
		/// </summary>
		public readonly Vector3 point;

		/// <summary>
		/// The rigidbody that was hit by the controller.
		/// </summary>
		public readonly Rigidbody rigidbody;

		/// <summary>
		/// The transform that was hit by the controller.
		/// </summary>
		public readonly Transform transform;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="openCharacterController">The character controller that hit.</param>
		/// <param name="hitInfo">The hit info.</param>
		/// <param name="directionMoved">Direction moved when collision occured.</param>
		/// <param name="distanceMoved">How far the character has travelled until it hit the collider.</param>
		public OpenCharacterControllerCollisionInfo(OpenCharacterController openCharacterController,
		                                          RaycastHit hitInfo,
		                                          Vector3 directionMoved,
		                                          float distanceMoved)
		{
			collider = hitInfo.collider;
			controller = openCharacterController;
			gameObject = hitInfo.collider.gameObject;
			moveDirection = directionMoved;
			moveLength = distanceMoved;
			normal = hitInfo.normal;
			point = hitInfo.point;
			rigidbody = hitInfo.rigidbody;
			transform = hitInfo.transform;
		}
	}
}
