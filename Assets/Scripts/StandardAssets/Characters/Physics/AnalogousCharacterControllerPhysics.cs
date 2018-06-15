using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	[RequireComponent(typeof(AnalogousCharacterController))]
	public class AnalogousCharacterControllerPhysics : BaseCharacterPhysics
	{
		private AnalogousCharacterController characterController;
		
		protected override void Awake()
		{
			base.Awake();
			characterController = GetComponent<AnalogousCharacterController>();
		}

		protected override bool CheckGrounded()
		{
			return characterController.isGrounded;
		}

		protected override void MoveCharacter(Vector3 movement)
		{
			characterController.Move(movement);
		}
	}
}