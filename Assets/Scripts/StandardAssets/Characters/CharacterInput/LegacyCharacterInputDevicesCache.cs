using Parabox.STL;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	public static class LegacyCharacterInputDevicesCache
	{
		private const string k_Filename = "LegacyCharacterInputDevices";

		private static string s_ConventionCache;

		private static LegacyCharacterInputDevices s_CharacterInputDevices;

		static LegacyCharacterInputDevicesCache()
		{
			s_CharacterInputDevices = Resources.Load<LegacyCharacterInputDevices>(k_Filename);
			SetupConvention(s_CharacterInputDevices.controlConventions);
		}

		private static void SetupConvention(string controlConvention)
		{
			s_ConventionCache = controlConvention.Replace("{platform}", "{0}").Replace("{controller}", "{1}")
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
				if (joystick.ToLower().Contains("xbox"))
				{
					return true;
				}
			}

			return false;
		}

		private static bool IsXbox360()
		{
			//TODO: dave
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