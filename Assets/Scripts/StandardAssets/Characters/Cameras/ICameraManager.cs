using UnityEngine;

namespace StandardAssets.Characters.Cameras
{
	public interface ICameraManager
	{
		GameObject currentCamera { get; }

		void SetCurrentCamera(GameObject newCamera);
	}
}