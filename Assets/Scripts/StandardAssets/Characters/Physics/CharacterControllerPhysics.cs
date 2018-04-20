using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	[RequireComponent(typeof(CharacterController))]
	public class CharacterControllerPhysics : MonoBehaviour, IPhysics
	{
		private CharacterController m_CharacterController;
		
		public void Move(Vector3 moveVector3)
		{
			m_CharacterController.Move(moveVector3);
		}

		void Awake()
		{
			m_CharacterController = GetComponent<CharacterController>();
		}
	}
}