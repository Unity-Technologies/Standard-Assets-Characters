using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	public static class LegacyCharacterInputDevicesCache
	{
		private static string s_ConventionCache;

		private static LegacyCharacterInputDevices s_CharacterInputDevices;

		private static bool xBoneActive; //XBox One
		private static bool xBoneWirelessActive; //XBox One Wireless
		private static bool xBoxActive; //XBox 360
		private static bool ps4Active; //PS4

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
			SetActiveGamepad();
		}

		private static void SetupConvention()
		{
			s_ConventionCache = s_CharacterInputDevices.controlConventions.Replace("{platform}", "{0}").Replace("{controller}", "{1}")
			                                     .Replace("{control}", "{2}");
		}

		private static void SetActiveGamepad()
		{
			foreach (var joystick in Input.GetJoystickNames())
			{
				Debug.Log(joystick);
			}

			xBoneActive = IsXboxOne();
			xBoxActive = IsXbox360();
			ps4Active = IsPS4();
			xBoneWirelessActive = IsXboxOneWireless();
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
			if (xBoneActive)
			{
				controllerId = s_CharacterInputDevices.xboxOneControllerIdentifier;
			}
			else if (xBoneWirelessActive)
			{
				controllerId = s_CharacterInputDevices.xboxOneWirelessControllerId1;
			}
			else if (xBoxActive)
			{
				controllerId = s_CharacterInputDevices.xbox360ControllerIdentifier;
			}
			else if (ps4Active)
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
				if (joystick.ToLower().Contains("xbox") && !joystick.ToLower().Contains("360")&& !joystick.ToLower().Contains("wireless"))
				{
					Debug.Log("XBone Wires");
					return true;
				}
			}

			return false;
		}
		
		private static bool IsXboxOneWireless()
		{
			foreach (var joystick in Input.GetJoystickNames())
			{
				if (joystick.ToLower().Contains("xbox")&& joystick.ToLower().Contains("wireless") && !joystick.ToLower().Contains("360"))
				{
					Debug.Log("XBone Wireless");
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
				if (!joystick.ToLower().Contains("xbox") & joystick.Length>1)
				{
					return true;
				}
			}

			return false;
		}
	}
}