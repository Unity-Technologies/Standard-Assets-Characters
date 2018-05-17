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
		}

		void Update()
		{
			Vector3 currentRotation = transform.rotation.eulerAngles;
			currentRotation.y = mainCamera.transform.rotation.eulerAngles.y;
			transform.rotation = Quaternion.Euler(currentRotation);
		}
	}
}