using StandardAssets.Characters.Cameras;
using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof(ICameraManager))]
	public class PovCharacterRotator : MonoBehaviour
	{
		public Camera mainCamera;
		
//		ICameraManager m_CameraManager;
//		GameObject m_CurrentCamera;
		
//		void Awake()
//		{
//			m_CameraManager = GetComponent<ICameraManager>();
//			CameraChanged();
//			m_CameraManager.cameraChanged += CameraChanged;
//		}
//
//		void CameraChanged()
//		{
//			m_CurrentCamera = m_CameraManager.currentCamera;
//		}

		void Update()
		{
//			if (m_CurrentCamera == null)
//			{
//				return;
//			}

			Vector3 currentRotation = transform.rotation.eulerAngles;
			currentRotation.y = mainCamera.transform.rotation.eulerAngles.y;
			transform.rotation = Quaternion.Euler(currentRotation);
		}
	}
}