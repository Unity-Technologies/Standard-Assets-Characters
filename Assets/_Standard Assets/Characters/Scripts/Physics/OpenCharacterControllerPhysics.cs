using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	public class OpenCharacterControllerPhysics : BaseCharacterPhysics
	{
		[SerializeField]
		protected OpenCharacterController characterController;

		/// <summary>
		/// Gets the open character controller.
		/// </summary>
		/// <value>The class that handles physics of the character.</value>
		public OpenCharacterController openCharacterController
		{
			get { return characterController; }
		}
		
		/// <inheritdoc/>
		public override bool startedSlide
		{
			get { return characterController.startedSlide; }
		}

		/// <inheritdoc/>
		protected override Vector3 footWorldPosition
		{
			get { return characterController.GetFootWorldPosition(); }
		}
		
		/// <inheritdoc/>
		protected override LayerMask collisionLayerMask
		{
			get { return characterController.GetCollisionLayerMask(); }
		}

		protected override void Awake()
		{
			base.Awake();
			characterController.Awake(transform);
		}

		/// <inheritdoc/>
		protected override float radius
		{
			get { return characterController.scaledRadius + characterController.GetSkinWidth(); }
		}
		
		/// <inheritdoc/>
		protected override bool CheckGrounded()
		{
			return characterController.isGrounded;
		}

		/// <inheritdoc/>
		protected override void MoveCharacter(Vector3 movement)
		{
			CollisionFlags collisionFlags = characterController.Move(movement);
			if ((collisionFlags & CollisionFlags.CollidedAbove) == CollisionFlags.CollidedAbove)
			{
				currentVerticalVelocity = 0f;
				initialJumpVelocity = 0f;
			}
		}

		#if UNITY_EDITOR
		private void OnValidate()
		{
			characterController.OnValidate();
		}
		#endif

		private void Update()
		{
			characterController.Update();
		}

		private void LateUpdate()
		{
			characterController.LateUpdate();
		}
		
		#if UNITY_EDITOR
		protected override void OnDrawGizmosSelected()
		{
			base.OnDrawGizmosSelected();
			characterController.OnDrawGizmosSelected(transform);
		}
		#endif
	}
}