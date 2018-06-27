using System;
using UnityEngine;


namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Default Unity Input System implementation of the InputResponse
	/// </summary>
	[CreateAssetMenu(fileName = "InputResponseXBoneMacAndPC", menuName = "Input Response/Create Default Unity Input Response MAC and PC ",
		order = 2)]
	public class LegacyUnityInputResponseMACvsPC: InputResponse
	{
		/// <summary>
		/// Classification of the type of response
		/// </summary>
		[SerializeField]
		protected DefaultInputResponseBehaviour behaviour;

		[SerializeField] 
		protected string axisRaw;

		[SerializeField]
		protected string XBoxOneButtonAxisOSX;
		
		[SerializeField]
		protected string XBoxOneButtonAxisWindows;

		[SerializeField] 
		protected string PS4ButtonAxisOSX;

		[SerializeField] 
		protected string PS4ButtonAxisWIndows;
		
		bool isXBone;

		/// <summary>
		/// Initializes the polling behaviour for the legacy input system
		/// </summary>
		public override void Init()
		{
			axisRaw = GetButtonAxisControllerOS();
			GameObject gameObject = new GameObject();
			gameObject.name = string.Format("LegacyInput_{0}_Poller", name);
			LegacyUnityInputResponsePoller poller = gameObject.AddComponent<LegacyUnityInputResponsePoller>();
			poller.Init(this, behaviour, axisRaw);
		}

		private string GetButtonAxisControllerOS()
		{
			
			foreach (var joystick in Input.GetJoystickNames())
			{
				if (joystick.ToLower().Contains("xbox"))
				{
					isXBone = true;
					break;
				}
				
			}
			//Only works with with XBox One for now
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			if(isXBone)
			{
				return XBoxOneButtonAxisOSX;	
			}
			return PS4ButtonAxisOSX;
			
#endif
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			if(isXBone)
			{
				return XBoxOneButtonAxisWindows;	
			}
			return PS4ButtonAxisWIndows;
#endif
			return axisRaw;
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
	
}