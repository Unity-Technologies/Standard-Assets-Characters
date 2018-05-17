using System;
using Cinemachine;
using UnityEngine;

namespace StandardAssets.Characters.Cameras
{
	/// <summary>
	/// Camera management agnostic of camera implementation
	/// </summary>
	public interface ICameraManager
	{
		/// <summary>
		/// Returns the GameObject associated with the current camera
		/// This allows the camera management to be agnostic of implementation
		/// </summary>
		CinemachineVirtualCamera currentCamera { get; }

		/// <summary>
		/// Sets the current camera game object
		/// </summary>
		/// <param name="newCamera"></param>
		void SetCurrentCamera(CinemachineVirtualCamera newCamera);
		
		/// <summary>
		/// The camera changed
		/// </summary>
		Action cameraChanged { get; set; }
	}
}