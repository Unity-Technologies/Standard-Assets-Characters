using System;
using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace StandardAssets.Characters.Input
{
	[CreateAssetMenu(fileName = "InputResponse", menuName = "Input Response/Create Default Unity Input Response", order = 1)]
	public class DefaultInputResponse : InputResponse
	{
		public DefaultInputResponseBehaviour behaviour;

		public KeyCode key;

		bool m_Check;

		public override void Init()
		{
			m_Check = false;
		}

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

		void Hold()
		{
			bool keyPressed = UnityInput.GetKey(key);

			if (!m_Check && keyPressed)
			{
				OnEnabled();
			}

			if (m_Check && !keyPressed)
			{
				OnDisabled();
			}

			m_Check = keyPressed;
		}

		void Toggle()
		{
			if (UnityInput.GetKeyDown(key))
			{
				if (!m_Check)
				{
					OnEnabled();
				}
				else
				{
					OnDisabled();
				}

				m_Check = !m_Check;
			}
		}
	}

	public enum DefaultInputResponseBehaviour
	{
		Toggle,
		Hold
	}
}