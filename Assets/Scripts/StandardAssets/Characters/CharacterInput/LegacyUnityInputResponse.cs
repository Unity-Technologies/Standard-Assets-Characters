using System;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Default Unity Input System implementation of the InputResponse
	/// </summary>
	[CreateAssetMenu(fileName = "InputResponse", menuName = "Input Response/Create Default Unity Input Response",
		order = 1)]
	public class LegacyUnityInputResponse : InputResponse
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

		protected bool touchToggle;
		
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
			
			GameObject gameObject = new GameObject();
			gameObject.name = string.Format("LegacyInput_{0}_Poller", name);
			LegacyUnityInputResponsePoller poller = gameObject.AddComponent<LegacyUnityInputResponsePoller>();
			poller.Init(this, behaviour, axis);
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

		public void TouchToggle()
		{
			if (!touchToggle)
			{
				OnInputStarted();
				touchToggle = true;
			}
			else
			{
				OnInputEnded();
				touchToggle = false;
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