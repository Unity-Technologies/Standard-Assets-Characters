using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Default Unity Input System implementation of the InputResponse
	/// </summary>
	[CreateAssetMenu(fileName = "InputResponse", menuName = "Standard Assets/Characters/Input/Create Legacy Input Response",
		order = 1)]
	public class LegacyInputResponse : InputResponse
	{
		[SerializeField, Tooltip("Determines how the Input Response behaves")]
		protected DefaultInputResponseBehaviour behaviour;

		[SerializeField, Tooltip("The Input Manager axis name")] 
		protected string axisRaw;
		
		[SerializeField, Tooltip("Is the input response for a gamepad - allows separate the behaviour for gamepads")]
		protected bool isGamepad;

		[SerializeField, Tooltip("Allows axis to be specified as buttons (i.e. mapping analog input to a digital input)")]
		protected AxisAsButton useAxisAsButton;
		
		/// <summary>
		/// Initializes the polling behaviour for the legacy input system
		/// </summary>
		public override void Init()
		{
			string axis = axisRaw;
			if (isGamepad)
			{
				string gamePadAxisName = LegacyCharacterInputDevicesCache.ResolveControl(axis);
				if (axis == gamePadAxisName)
				{
					return;
				}

				axis = gamePadAxisName;
			}
			
			LegacyInputResponsePollerManager.instance.InitPoller(this, behaviour, axis, useAxisAsButton);
		}

		/// <summary>
		/// Exposes the input start for the poller
		/// </summary>
		public void BroadcastStart()
		{
			OnInputStarted();
		}

		/// <summary>
		/// Exposes the input end for the poller
		/// </summary>
		public void BroadcastEnd()
		{
			OnInputEnded();
		}
	}

	/// <summary>
	/// Describes the input behaviour
	/// </summary>
	public enum DefaultInputResponseBehaviour
	{
		Toggle,
		Hold,
		Button
	}
	
	/// <summary>
	/// Enum to define if an input axis will be treated as a button
	/// </summary>
	public enum AxisAsButton
	{
		None,
		Positive,
		Negative
	}
}