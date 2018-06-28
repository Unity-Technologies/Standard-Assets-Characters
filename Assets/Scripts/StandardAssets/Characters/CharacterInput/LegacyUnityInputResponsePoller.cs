using System;
using System.Threading;
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
		/// Multi-purpose bool. For Toggles in represents the on state. For Holds it represents the previous button state
		/// </summary>
		private bool check;

		private string axisRaw;
		
		
		private bool axisRawPressed;

	

		/// <summary>
		/// Called by the LegacyInputResponse
		/// </summary>
		/// <param name="newResponse"></param>
		/// <param name="newBehaviour"></param>
		/// <param name="axisRaw"></param>
		public void Init(LegacyUnityInputResponse newResponse, DefaultInputResponseBehaviour newBehaviour, String axisString)
		{
			response = newResponse;
			behaviour = newBehaviour;
			axisRaw = axisString;
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
			bool isAxis = Input.GetAxisRaw(axisRaw) !=0;
			
			if (!check && isAxis)
			{
				response.BroadcastStart();
			}

			if (check && !isAxis)
			{
				response.BroadcastEnd();
			}
		
			check = isAxis;
			
		}

		/// <summary>
		/// Logic for Toggles
		/// </summary>
		private void Toggle()
		{
			
			if (Input.GetAxisRaw(axisRaw) == 0)
			{
				axisRawPressed = false;
			}
			if (axisRawPressed) return;
			
			if ( Input.GetAxisRaw(axisRaw) != 0)
			{
				axisRawPressed = true;
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