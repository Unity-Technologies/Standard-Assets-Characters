using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	public class OpenCharacterControllerPhysics : BaseCharacterPhysics
	{
		[SerializeField]
		protected OpenCharacterController characterController;
		
		public override float GetPredicitedFallDistance()
		{
			return characterController.GetPredicitedFallDistance();
		}

		public OpenCharacterController GetOpenCharacterController()
		{
			return characterController;
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

		private void LateUpdate()
		{
			characterController.LateUpdate();
		}
		
		#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			characterController.OnDrawGizmosSelected(transform);
		}
		#endif
	}
}