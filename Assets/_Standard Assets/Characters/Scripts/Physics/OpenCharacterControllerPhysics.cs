using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	public class OpenCharacterControllerPhysics : BaseCharacterPhysics
	{
		[SerializeField]
		protected OpenCharacterController characterController;

		public OpenCharacterController GetOpenCharacterController()
		{
			return characterController;
		}

		public override bool startedSlide
		{
			get { return characterController.startedSlide; }
		}

		protected override Vector3 footWorldPosition
		{
			get { return characterController.GetFootWorldPosition(); }
		}

		protected override LayerMask collisionLayerMask
		{
			get { return characterController.GetCollisionLayerMask(); }
		}

		protected override void Awake()
		{
			base.Awake();
			characterController.Awake(transform);
		}

		protected override bool CheckGrounded()
		{
			return characterController.isGrounded;
		}

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