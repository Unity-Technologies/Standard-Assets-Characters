using System;
using UnityEngine;

namespace StandardAssets.Characters.Cameras
{
	/// <summary>
	/// Basic implementation of the camera manager
	/// </summary>
	public class CameraManager : MonoBehaviour, ICameraManager
	{
		/// <inheritdoc />
		public GameObject currentCamera { get; private set; }
		
		/// <inheritdoc />
		public Action cameraChanged { get; set; }
		
		/// <inheritdoc />
		public void SetCurrentCamera(GameObject newCamera)
		{
			//Designed to be used with Cinemachine
			//disabling the current camera GameObject and enabling the newCamera GameObject
			//allows the Cinemachine brain to transition between the different cameras
			if (currentCamera != null)
			{
				currentCamera.SetActive(false);
			}
			currentCamera = newCamera;
			currentCamera.SetActive(true);
			
			if (cameraChanged != null)
			{
				cameraChanged();
			}
		}

		
	}
}