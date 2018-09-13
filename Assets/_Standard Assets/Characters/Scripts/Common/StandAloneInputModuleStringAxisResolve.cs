using StandardAssets.Characters.CharacterInput;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StandardAssets.Characters.Common
{
	/// <summary>
	/// Resolves the input axis string for menu navigation using gamepad
	/// </summary>
	[RequireComponent(typeof(StandaloneInputModule))]
	public class StandAloneInputModuleStringAxisResolve: MonoBehaviour
	{
		[SerializeField, Tooltip("The Input Axis name. By default this is set to 'Jump' " +
		                         "which is bound to Button South on Gamepads")]
		protected string submitButtonAxisName;

		private void Awake()
		{	
			if (submitButtonAxisName != null)
			{
				GetComponent<StandaloneInputModule>().submitButton = 
					LegacyCharacterInputDevicesCache.ResolveControl(submitButtonAxisName);
			}
		}
	}
}