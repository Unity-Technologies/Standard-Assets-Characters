using StandardAssets.Characters.Cameras;
using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// Rotates the first character to the Y rotation of the POV camera (i.e. main camera)
	/// </summary>
	[RequireComponent(typeof(ICameraManager))]
	public class PovCharacterRotator : MonoBehaviour
	{
		/// <summary>
		/// Main Camera that is using the POV camera
		/// </summary>
		public Camera mainCamera;
		
		/// <summary>
		/// The camera manager
		/// </summary>
		ICameraManager m_CameraManager;
		
		/// <summary>
		/// Gets attached camera manager
		/// </summary>
		void Awake()
		{
			m_CameraManager = GetComponent<ICameraManager>();
		}

		/// <summary>
		/// Adjust rotation
		/// </summary>
		void Update()
		{
			Vector3 currentRotation = transform.rotation.eulerAngles;
			currentRotation.y = mainCamera.transform.rotation.eulerAngles.y;
			transform.rotation = Quaternion.Euler(currentRotation);
		}
	}
}