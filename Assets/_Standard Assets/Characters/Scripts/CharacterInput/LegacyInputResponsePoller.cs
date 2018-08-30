using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Simply polls on behalf of the LegacyUnityInputResponse
	/// </summary>
	public class LegacyInputResponsePoller : MonoBehaviour
	{
		/// <summary>
		/// Static list of pollers
		/// </summary>
		private static List<LegacyInputResponsePoller> s_Pollers;
		
		/// <summary>
		/// The Input Response needed
		/// </summary>
		private LegacyInputResponse response;
		
		/// <summary>
		/// Behaviour - hold/toggle
		/// </summary>
		private DefaultInputResponseBehaviour behaviour;
	
		/// <summary>
		/// Multi-purpose bool. For Toggles in represents the on state. For Holds it represents the previous button state
		/// </summary>
		private bool check;

		private string axisRaw;

		/// <summary>
		/// Whether the axis will be treated as raw or a button when negative or positive
		/// </summary>
		private AxisAsButton useAxisAsButton; 

		/// <summary>
		/// Called by the LegacyInputResponse
		/// </summary>
		/// <param name="newResponse"></param>
		/// <param name="newBehaviour"></param>
		/// <param name="axisRaw"></param>
		public void Init(LegacyInputResponse newResponse, DefaultInputResponseBehaviour newBehaviour, String axisString,AxisAsButton axisAsButton)
		{
			response = newResponse;
			behaviour = newBehaviour;
			axisRaw = axisString;	
			useAxisAsButton = axisAsButton;

			if (s_Pollers == null)
			{
				s_Pollers = new List<LegacyInputResponsePoller>();
			}
			
			s_Pollers.Add(this);
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
				case DefaultInputResponseBehaviour.Button:
					Press();
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
			bool isAxis;
			if (useAxisAsButton == AxisAsButton.None)
			{
				isAxis = Input.GetButton(axisRaw);
			}
			else
			{
				isAxis = CheckAxisAsButton();
			}
			
			
			if (!check && isAxis)
			{		
				OnStart();
			}

			if (check && !isAxis)
			{
				OnEnd();
			}
		
			check = isAxis;
		}

		/// <summary>
		/// Logic for Toggles
		/// </summary>
		private void Toggle()
		{
			bool buttonDown;
			if (useAxisAsButton == AxisAsButton.None)
			{
				buttonDown = Input.GetButtonDown(axisRaw);
			}
			else
			{
				buttonDown = CheckAxisAsButton();
			}
			
			if (buttonDown)
			{
				if (!check)
				{
					OnStart();
				}
				else
				{
					OnEnd();
				}
				check = !check;
			}
		}
		
		/// <summary>
		/// Logic for button
		/// </summary>
		private void Press()
		{
			bool buttonDown;
			if (useAxisAsButton == AxisAsButton.None)
			{
				buttonDown = Input.GetButtonDown(axisRaw);
			}
			else
			{
				buttonDown = CheckAxisAsButton();
			}

			if (buttonDown)
			{
				OnStart();
			}
		}

		/// <summary>
		/// Converts specific axis to a button press
		/// </summary>
		/// <returns></returns>
		private bool CheckAxisAsButton()
		{
			switch (useAxisAsButton)
			{
				case AxisAsButton.Positive:
					if (Mathf.Approximately(Input.GetAxis(axisRaw), 1)) // positive button pressed
					{
						return true;
					}
					break;
				case AxisAsButton.Negative:
					if (Mathf.Approximately(Input.GetAxis(axisRaw), -1)) // negative button pressed
					{
						return true;
					}
					break;
			}
			return false;
		}
		
		/// <summary>
		/// Checks the list of all active Pollers and turns off all active toggles except its own.
		/// This is to prevent a state change while check==true, which leads having to re-toggle before
		/// the response broadcast is started again.
		/// </summary>
		private void OnStart()
		{
			if(s_Pollers!=null)
			{
				foreach (var poller in s_Pollers)
				{
					if (poller != this)
					{
						poller.check = false;
					}
				}
			}
			response.BroadcastStart();
		}

		/// <summary>
		/// Broadcasts the end
		/// </summary>
		private void OnEnd()
		{
			response.BroadcastEnd();
		}

		/// <summary>
		/// Helper for mobile controls 
		/// </summary>
		public void TouchScreenButtonToggle()
		{
			if (!check)
			{
				OnStart();
			}
			else
			{
				OnEnd();
			}
			check = !check;
		}
	}
}