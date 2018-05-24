using System;
using Cinemachine;
using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// Basic implementation of the camera manager
	/// </summary>
	public class CameraManager : MonoBehaviour, ICameraManager
	{
		/// <inheritdoc />
		public CinemachineVirtualCamera currentCamera { get; private set; }
		
		/// <inheritdoc />
		public Action cameraChanged { get; set; }
		
		/// <inheritdoc />
		public void SetCurrentCamera(CinemachineVirtualCamera newCamera)
		{
			if (currentCamera != null)
			{
				currentCamera.Priority = 0;
			}

			currentCamera = newCamera;
			currentCamera.Priority = 100;
			
			if (cameraChanged != null)
			{
				cameraChanged();
			}
		}

		
	}
}