using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Default Unity Input System implementation of the InputResponse
	/// </summary>
	[CreateAssetMenu(fileName = "InputResponse", menuName = "Input Response/Create Default Unity Input Response",
		order = 1)]
	public class LegacyInputResponse : InputResponse
	{
		/// <summary>
		/// Classification of the type of response
		/// </summary>
		[SerializeField]
		protected DefaultInputResponseBehaviour behaviour;

		[SerializeField] 
		protected string axisRaw;
		
		[SerializeField]
		protected bool isGamepad;

		[SerializeField]
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
		Hold
	}
}