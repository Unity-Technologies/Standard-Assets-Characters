using StandardAssets.Characters.Cameras;
using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof(ICameraManager))]
	public class PovCharacterRotator : MonoBehaviour
	{
		public Camera mainCamera;
		
		ICameraManager m_CameraManager;
		
		void Awake()
		{
			m_CameraManager = GetComponent<ICameraManager>();
			CameraChanged();
			m_CameraManager.cameraChanged += CameraChanged;
		}

		void CameraChanged()
		{
			//DAVE check out keeping the POV the same
		}

		void Update()
		{
			Vector3 currentRotation = transform.rotation.eulerAngles;
			currentRotation.y = mainCamera.transform.rotation.eulerAngles.y;
			transform.rotation = Quaternion.Euler(currentRotation);
		}
	}
}