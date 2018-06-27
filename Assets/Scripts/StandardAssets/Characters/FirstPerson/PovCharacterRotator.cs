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
		[SerializeField]
		protected Camera mainCamera;

		/// <summary>
		/// Adjust rotation
		/// </summary>
		private void Update()
		{
			Vector3 currentRotation = transform.rotation.eulerAngles;
			currentRotation.y = mainCamera.transform.rotation.eulerAngles.y;
			transform.rotation = Quaternion.Euler(currentRotation);
		}
		
		
	}
}