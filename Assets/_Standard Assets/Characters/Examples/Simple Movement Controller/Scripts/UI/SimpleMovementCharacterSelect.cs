using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace StandardAssets.Characters.Examples.SimpleMovementController.UI
{
	/// <summary>
	/// UI for selecting a character via toggle buttons.
	/// </summary>
	public class SimpleMovementCharacterSelect : MonoBehaviour
	{

		[Header("Game Objects")]
		[SerializeField, Tooltip("The freelook camera.")]
		CinemachineFreeLook m_FreeLook;

		[SerializeField, Tooltip("Characters to select.")]
		GameObject[] m_Characters;

		[Header("Elements")]
		[SerializeField, Tooltip("A toggle for each character.")]
		Toggle[] m_Toggles;

		/// <summary>
		/// Called when a toggle's value changed.
		/// </summary>
		public void OnToggleValueChanged(bool isOn)
		{
			for (int i = 0, len = m_Toggles.Length; i < len; i++)
			{
				var toggle = m_Toggles[i];
				if (toggle != null &&
					toggle.isOn)
				{
					SelectCharacter(i);
					break;
				}
			}
		}

		// Select the default character controller.
		void Start()
		{
			if (m_Characters.Length != m_Toggles.Length)
			{
				Debug.LogError("The number of characters and toggles are not the same.");
			}

			SelectCharacter(0);
		}

		// Select the character by index.
		void SelectCharacter(int index)
		{
			var len = m_Characters.Length;
			if (index < 0)
			{
				index = len - 1;
			}

			if (index >= len)
			{
				index = 0;
			}

			GameObject activeCharacter = null;
			for (var i = 0; i < len; i++)
			{
				var isSelected = (index == i);
				var character = m_Characters[i];
				if (character != null)
				{
					character.SetActive(false);
					if (isSelected &&
						m_FreeLook != null)
					{
						m_FreeLook.LookAt = character.transform;
						m_FreeLook.Follow = character.transform;
					}

					if (isSelected)
					{
						activeCharacter = character;
					}
				}

				var toggle = m_Toggles[i];
				if (toggle != null)
				{
					toggle.isOn = isSelected;
				}
			}

			// Enable the correct character after the others have been disabled, in case one of the others
			// disables the global Controls.
			if (activeCharacter != null)
			{
				activeCharacter.SetActive(true);
			}
		}
	}
}
