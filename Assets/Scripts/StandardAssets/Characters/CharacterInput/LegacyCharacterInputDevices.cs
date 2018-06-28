using System;
using Boo.Lang;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	[CreateAssetMenu(fileName = "LegacyCharacterInputDevices", menuName = "LegacyInput/Create Input Device Mapping",
		order = 1)]
	public class LegacyCharacterInputDevices : ScriptableObject
	{
		[SerializeField]
		protected string macPlatformId = "OSX", windowsPlatformId = "Windows";

		[SerializeField]
		protected string xboxOneControllerId = "XBone", xbox360ControllerId = "XBox360", ps4ControllerId = "PS4";

		[SerializeField]
		protected string controlConvention = "{control}{controller}{platform}";

		public string macPlatformIdentifier
		{
			get { return macPlatformId; }
		}

		public string windowsPlatformIdentifier
		{
			get { return windowsPlatformId; }
		}

		public string xboxOneControllerIdentifier
		{
			get { return xboxOneControllerId; }
		}

		public string xbox360ControllerIdentifier
		{
			get { return xbox360ControllerId; }
		}

		public string ps4ControllerIdentifier
		{
			get { return ps4ControllerId; }
		}

		public string controlConventions
		{
			get { return controlConvention; }
		}
	}
}