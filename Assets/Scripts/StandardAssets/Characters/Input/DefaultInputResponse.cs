using System;
using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace StandardAssets.Characters.Input
{
	/// <summary>
	/// Default Unity Input System implementation of the InputResponse
	/// </summary>
	[CreateAssetMenu(fileName = "InputResponse", menuName = "Input Response/Create Default Unity Input Response", order = 1)]
	public class DefaultInputResponse : InputResponse
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
		bool m_Check;

		/// <summary>
		/// Initializes
		/// </summary>
		public override void Init()
		{
			m_Check = false;
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
		void Hold()
		{
			bool keyPressed = UnityInput.GetKey(key);

			if (!m_Check && keyPressed)
			{
				OnInputStarted();
			}

			if (m_Check && !keyPressed)
			{
				OnInputEnded();
			}

			m_Check = keyPressed;
		}

		/// <summary>
		/// Logic for Toggles
		/// </summary>
		void Toggle()
		{
			if (UnityInput.GetKeyDown(key))
			{
				if (!m_Check)
				{
					OnInputStarted();
				}
				else
				{
					OnInputEnded();
				}

				m_Check = !m_Check;
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