using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// Rotates the first character to the Y rotation of the POV camera (i.e. main camera)
	/// </summary>
	public class PovCharacterRotator : MonoBehaviour
	{
		/// <summary>
		/// Main Camera that is using the POV camera
		/// </summary>
		public Camera mainCamera;

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