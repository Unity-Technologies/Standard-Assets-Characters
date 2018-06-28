using System;
using Parabox.STL;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	public static class LegacyCharacterInputDevicesCache
	{
		private static string s_ConventionCache;

		private static LegacyCharacterInputDevices s_CharacterInputDevices;

		static LegacyCharacterInputDevicesCache()
		{
			LegacyCharacterInputDevices[] resources = Resources.LoadAll<LegacyCharacterInputDevices>(string.Empty);
			int length = resources.Length;
			if (length == 0)
			{
				Debug.LogError("Could not find LegacyCharacterInputDevices in Resources folder");
				return;
			}

			if (length > 1)
			{
				Debug.LogError("Found multiple instances of LegacyCharacterInputDevices in Resources folder. There can be only one!!!");
				return;
			}
			
			s_CharacterInputDevices = resources[0];
			SetupConvention();
		}

		private static void SetupConvention()
		{
			s_ConventionCache = s_CharacterInputDevices.controlConventions.Replace("{platform}", "{0}").Replace("{controller}", "{1}")
			                                     .Replace("{control}", "{2}");
		}

		public static string ResolveControl(string control)
		{
			string platformId = string.Empty;

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			platformId = s_CharacterInputDevices.macPlatformIdentifier;
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			platformId = s_CharacterInputDevices.windowsPlatformIdentifier;
#endif
			string controllerId = string.Empty;
			if (IsXboxOne())
			{
				controllerId = s_CharacterInputDevices.xboxOneControllerIdentifier;
			}
			else if (IsXbox360())
			{
				controllerId = s_CharacterInputDevices.xbox360ControllerIdentifier;
			}
			else if (IsPS4())
			{
				controllerId = s_CharacterInputDevices.ps4ControllerIdentifier;
			}
			else
			{
				return control;
			}

			return string.Format(s_ConventionCache, platformId, controllerId,
			                     control);
		}

		private static bool IsXboxOne()
		{
			foreach (var joystick in Input.GetJoystickNames())
			{
				if (joystick.ToLower().Contains("xbox") & !joystick.ToLower().Contains("360"))
				{
					return true;
				}
			}

			return false;
		}

		private static bool IsXbox360()
		{
			foreach (var joystick in Input.GetJoystickNames())
			{
				if (joystick.ToLower().Contains("360"))
				{
					return true;
				}
			}

			return false;
		}

		private static bool IsPS4()
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
	}
}