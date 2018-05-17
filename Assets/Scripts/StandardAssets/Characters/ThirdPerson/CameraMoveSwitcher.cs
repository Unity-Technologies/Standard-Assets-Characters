using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Dummy class for demonstrating the ThirdPersonMotorStateMachine interface
	/// </summary>
	public class CameraMoveSwitcher : MonoBehaviour, IThirdPersonMotorStateMachine
	{
		/// <summary>
		/// Handles state changes on key press
		/// </summary>
		void Update()
		{
			if (UnityEngine.Input.GetKeyDown(KeyCode.Z))
			{
				idling();
			}

			if (UnityEngine.Input.GetKeyDown(KeyCode.X))
			{
				walking();
			}

			if (UnityEngine.Input.GetKeyDown(KeyCode.C))
			{
				running();
			}
		}

		public Action idling { get; set; }
		public Action walking { get; set; }
		public Action running { get; set; }
	}
}