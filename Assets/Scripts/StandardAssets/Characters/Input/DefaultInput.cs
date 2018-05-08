using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace StandardAssets.Characters.Input
{
	/// <summary>
	/// Unity original input implementation
	/// </summary>
	public class DefaultInput : MonoBehaviour, IInput
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
			//Cache the inputs
			m_MoveInput.Set(UnityInput.GetAxis("Horizontal"), UnityInput.GetAxis("Vertical"));
		}
	}
}