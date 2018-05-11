using System;
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
		
		Vector2 m_LookInput;

		public Vector2 lookInput
		{
			get { return m_LookInput; }
		}

		Action m_Jump;

		public Action jump
		{
			get { return m_Jump; }
			set { m_Jump = value; }
		}

		void Update()
		{
			//Cache the inputs
			m_MoveInput.Set(UnityInput.GetAxis("Horizontal"), UnityInput.GetAxis("Vertical"));
			m_LookInput.Set(UnityInput.GetAxis("Mouse X"), UnityInput.GetAxis("Mouse Y"));
			if (UnityInput.GetKeyDown(KeyCode.Space))
			{
				if (jump != null)
				{
					jump();
				}
			}
		}
	}
}