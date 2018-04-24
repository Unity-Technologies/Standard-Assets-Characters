using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace StandardAssets.Characters.Input
{
	public class DefaultThirdPersonInput : MonoBehaviour, IThirdPersonInput
	{
		Vector2 m_MoveInput;

		public Vector2 moveInput
		{
			get { return m_MoveInput; }
		}
		public bool isMoveInput 
		{ 
			get { return moveInput.sqrMagnitude > 0; }
		}

		void Update()
		{
			m_MoveInput.Set(UnityInput.GetAxis("Horizontal"), UnityInput.GetAxis("Vertical"));
		}
	}
}