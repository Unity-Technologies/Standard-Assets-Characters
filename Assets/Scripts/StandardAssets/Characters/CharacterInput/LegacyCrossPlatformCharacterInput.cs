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

		private LegacyCharacterInputBase currentInputSystem;

		public Vector2 lookInput
		{
			get { return currentInputSystem.lookInput; }
		}

		public Vector2 moveInput
		{
			get { return currentInputSystem.moveInput; }
		}

		public bool hasMovementInput
		{
			get { return currentInputSystem.hasMovementInput; }
		}

		public Action jumpPressed
		{
			get { return currentInputSystem.jumpPressed; }
			set { currentInputSystem.jumpPressed = value; }
		}

		private void Awake()
		{
			#if UNITY_ANDROID || UNITY_IOS
			SetMobileControls();
			#else
			SetStandaloneControls();
			#endif
		}

		private void SetMobileControls()
		{
			currentInputSystem = mobileInput;
			standaloneInput.gameObject.SetActive(false);
		}

		private void SetStandaloneControls()
		{
			currentInputSystem = standaloneInput;
			mobileInput.gameObject.SetActive(false);
		}
	}
}