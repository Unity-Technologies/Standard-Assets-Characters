using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Simply polls on behalf of the LegacyUnityInputResponse
	/// </summary>
	public class LegacyInputResponsePoller : MonoBehaviour
	{
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

		//This is to enable the ability to use an axis as a button. XBone Left Trigger is a -1->1 axis,
		// If the player wishes to use the left/right triggers on an xBone controller on OSX as a "button" type response. 
		private bool useAxisAsButton; 

		/// <summary>
		/// Called by the LegacyInputResponse
		/// </summary>
		/// <param name="newResponse"></param>
		/// <param name="newBehaviour"></param>
		/// <param name="axisRaw"></param>
		public void Init(LegacyInputResponse newResponse, DefaultInputResponseBehaviour newBehaviour, String axisString,bool axisAsButton)
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
			if (useAxisAsButton)
			{
				isAxis = checkAxisAsButton();
			}
			else
			{
				isAxis = Input.GetButton(axisRaw);
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
			if (useAxisAsButton)
			{
				buttonDown = checkAxisAsButton();
			}
			else
			{
				buttonDown = Input.GetButtonDown(axisRaw);
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

		private bool checkAxisAsButton()
		{
			if (useAxisAsButton)
			{
				if (Input.GetAxis(axisRaw)==1) //If axis is 1, "button is pushed" 
				{
					return true;
				}
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

		private void OnEnd()
		{
			response.BroadcastEnd();
		}

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