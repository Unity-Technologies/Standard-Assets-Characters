using System;
using Boo.Lang;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	[CreateAssetMenu(fileName = "LegacyCharacterInputDevices", menuName = "LegacyInput/Create Input Device Mapping",
		order = 1)]
	public class LegacyCharacterInputDevices : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField]
		private string macPlatformId = "OSX", windowsPlatformId = "Windows";

		[SerializeField]
		private string xboxOneControllerId = "XBone", xbox360ControllerId = "XBox360", ps4ControllerId = "PS4";

		[SerializeField]
		private string controlConvention = "{control}{controller}{platform}";

		private string convention;

		public string GetAxisName(string axisString)
		{
			string platformId = string.Empty;

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			platformId = macPlatformId;
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			platformId = windowsPlatformId;
#endif
			string controllerId = string.Empty;
			if (IsXboxOne())
			{
				controllerId = xboxOneControllerId;
			}
			else if (IsXbox360())
			{
				controllerId = xbox360ControllerId;
			}
			else if (IsPS4())
			{
				controllerId = ps4ControllerId;
			}
			else
			{
				return axisString;
			}

			if (string.IsNullOrEmpty(convention))
			{   
				convention = controlConvention.Replace("{platform}", "{0}").Replace("{controller}", "{1}")
				                              .Replace("{control}", "{2}");	
			}
			string axis = string.Format(convention,platformId,controllerId,axisString);
			return axis;
		}

		private bool IsXboxOne()
		{
			foreach (var joystick in Input.GetJoystickNames())
			{
				if (joystick.ToLower().Contains("xbox"))
				{
					return true;
				}
					
			}
			return false;
		}
		
		private bool IsXbox360()
		{
			//TODO: dave
			return false;
		}
		
		private bool IsPS4()
		{
			foreach (var joystick in Input.GetJoystickNames())
			{
				if (!joystick.ToLower().Contains("xbox"))
				{
					return true;
				}
					
			}
			return false;
		}

		public void OnBeforeSerialize()
		{
			//convention = string.Empty;
		}

		public void OnAfterDeserialize()
		{
			convention = string.Empty;
		}
	}
}