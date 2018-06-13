using System;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Simply polls on behalf of the LegacyUnityInputResponse
	/// </summary>
	public class LegacyUnityInputResponsePoller : MonoBehaviour
	{
		/// <summary>
		/// The Input Response needed
		/// </summary>
		private LegacyUnityInputResponse response;
		
		/// <summary>
		/// Behaviour - hold/toggle
		/// </summary>
		private DefaultInputResponseBehaviour behaviour;
		
		/// <summary>
		/// The key
		/// </summary>
		private KeyCode key;
		
		/// <summary>
		/// Multi-purpose bool. For Toggles in represents the on state. For Holds it represents the previous button state
		/// </summary>
		private bool check;

		/// <summary>
		/// Called by the LegacyInputResponse
		/// </summary>
		/// <param name="newResponse"></param>
		/// <param name="newBehaviour"></param>
		/// <param name="newKey"></param>
		public void Init(LegacyUnityInputResponse newResponse, DefaultInputResponseBehaviour newBehaviour, KeyCode newKey)
		{
			response = newResponse;
			behaviour = newBehaviour;
			key = newKey;
		}
		
		/// <summary>
		/// Does polling of inputs
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		private void Update()
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
				response.BroadcastStart();
			}

			if (check && !keyPressed)
			{
				response.BroadcastEnd();
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
					response.BroadcastStart();
				}
				else
				{
					response.BroadcastEnd();
				}

				check = !check;
			}
		}
	}
}