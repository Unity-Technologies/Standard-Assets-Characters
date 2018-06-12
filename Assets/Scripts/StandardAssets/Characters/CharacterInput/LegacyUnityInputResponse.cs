using System;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Default Unity Input System implementation of the InputResponse
	/// </summary>
	[CreateAssetMenu(fileName = "InputResponse", menuName = "Input Response/Create Default Unity Input Response", order = 1)]
	public class LegacyUnityInputResponse : InputResponse
	{
		/// <summary>
		/// Classification of the type of response
		/// </summary>
		public DefaultInputResponseBehaviour behaviour;

		/// <summary>
		/// The key
		/// </summary>
		public KeyCode key;

		/// <summary>
		/// Multi-purpose bool. For Toggles in represents the on state. For Holds it represents the previous button state
		/// </summary>
		private bool check;

		/// <summary>
		/// Initializes
		/// </summary>
		public override void Init()
		{
			check = false;
		}

		/// <summary>
		/// Updates the Input Response
		/// </summary>
		public override void Tick()
		{
			switch (behaviour)
			{
				case DefaultInputResponseBehaviour.Hold:
					Hold();
					break;
				case DefaultInputResponseBehaviour.Toggle:
					Toggle();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Logic for Holds
		/// </summary>
		private void Hold()
		{
			bool keyPressed = Input.GetKey(key);

			if (!check && keyPressed)
			{
				OnInputStarted();
			}

			if (check && !keyPressed)
			{
				OnInputEnded();
			}

			check = keyPressed;
		}

		/// <summary>
		/// Logic for Toggles
		/// </summary>
		private void Toggle()
		{
			if (Input.GetKeyDown(key))
			{
				if (!check)
				{
					OnInputStarted();
				}
				else
				{
					OnInputEnded();
				}

				check = !check;
			}
		}
	}

	/// <summary>
	/// Describes the input behaviour
	/// </summary>
	public enum DefaultInputResponseBehaviour
	{
		Toggle,
		Hold
	}
}