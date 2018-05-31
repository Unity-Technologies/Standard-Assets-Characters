using System;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Unity original input implementation
	/// </summary>
	public class LegacyUnityCharacterInput : MonoBehaviour, ICharacterInput
	{
		public string horizontalAxisName = "Horizontal";
		public string verticalAxisName = "Vertical";
		public KeyCode jumpKey = KeyCode.Space;
		
		Vector2 m_MoveInput;

		public Vector2 moveInput
		{
			get { return m_MoveInput; }
		}
		
		public bool hasMovementInput 
		{ 
			get { return moveInput != Vector2.zero; }
		}
		
		Action m_Jump;

		public Action jumpPressed
		{
			get { return m_Jump; }
			set { m_Jump = value; }
			
		}

		void Update()
		{
			//Cache the inputs
			m_MoveInput.Set(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName));
			if (Input.GetKeyDown(jumpKey))
			{
				
				if (jumpPressed != null)
				{
					jumpPressed();
				}
			}
		}
	}
}