using System;
using UnityEngine;


namespace StandardAssets.Characters.CharacterInput
{
	public class LegacyCrossPlatformCharacterInput : MonoBehaviour, ICharacterInput
	{
		[SerializeField]
		protected LegacyCharacterInput standaloneInput;

		[SerializeField]
		protected LegacyOnScreenCharacterInput mobileInput;

		[SerializeField]
		protected bool debugOnScreenControls;

		private LegacyCharacterInputBase currentInputSystem;
		

		private LegacyCharacterInputBase currentInput
		{
			get
			{
				if (currentInputSystem == null)
				{
					SetControls();
				}

				return currentInputSystem;
			}
		}

		public Vector2 lookInput
		{
			get { return currentInput.lookInput; }
		}

		public Vector2 moveInput
		{
			get { return currentInput.moveInput; }
		}

		public Vector2 previousNonZeroMoveInput
		{
			get { return currentInput.previousNonZeroMoveInput; }
		}

		public bool hasMovementInput
		{
			get { return currentInput.hasMovementInput; }
		}

		public bool isJumping
		{
			get { return currentInput.isJumping; }
		}

		public Action jumpPressed
		{
			get { return currentInput.jumpPressed; }
			set { currentInput.jumpPressed = value; }
		}

		private void SetControls()
		{
			if (debugOnScreenControls)
			{
				SetMobileControls();
				return;
			}
			
			#if UNITY_ANDROID || UNITY_IOS
			SetMobileControls();
			#else
			SetStandaloneControls();
			#endif
		}

		private void SetMobileControls()
		{
			currentInputSystem = mobileInput;
			mobileInput.gameObject.SetActive(true);
			standaloneInput.gameObject.SetActive(false);
		}

		private void SetStandaloneControls()
		{
			currentInputSystem = standaloneInput;
			standaloneInput.gameObject.SetActive(true);
			mobileInput.gameObject.SetActive(false);
		}
	}
}