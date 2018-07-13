using System;
using Cinemachine;
using StandardAssets.Characters.FirstPerson;
using UnityEngine;
using UnityEngine.Networking;

namespace StandardAssets.Characters.CharacterInput
{
	public class LegacyCrossPlatformCharacterInput : MonoBehaviour, ICharacterInput
	{
		[SerializeField]
		protected LegacyCharacterInput standaloneInput;

		[SerializeField]
		protected LegacyOnScreenCharacterInput mobileInput;

		/*
		 * [SerializeField]
		protected FirstPersonMouseLookPOVCamera mousePov; 
		 */

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

		public bool hasMovementInput
		{
			get { return currentInput.hasMovementInput; }
		}

		public Action jumpPressed
		{
			get { return currentInput.jumpPressed; }
			set { currentInput.jumpPressed = value; }
		}

		private void SetControls()
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
			mobileInput.gameObject.SetActive(true);
			standaloneInput.gameObject.SetActive(false);
            
           
			
			/*
			 * if (mousePov != null)
			{
				mousePov.enabled = false;
			}
			 */
		}

		private void SetStandaloneControls()
		{
			currentInputSystem = standaloneInput;
			standaloneInput.gameObject.SetActive(true);
			mobileInput.gameObject.SetActive(false);
		}
	}
}