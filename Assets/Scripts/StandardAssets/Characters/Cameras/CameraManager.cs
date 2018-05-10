using UnityEngine;

namespace StandardAssets.Characters.Cameras
{
	public class CameraManager : MonoBehaviour, ICameraManager
	{
		public GameObject currentCamera { get; private set; }
		
		public void SetCurrentCamera(GameObject newCamera)
		{
			if (currentCamera != null)
			{
				currentCamera.SetActive(false);
			}
			currentCamera = newCamera;
			currentCamera.SetActive(true);
		}
	}
}