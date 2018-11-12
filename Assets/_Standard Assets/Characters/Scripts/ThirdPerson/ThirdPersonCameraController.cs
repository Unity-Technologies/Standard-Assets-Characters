using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Displays crosshair when strafing
	/// </summary>
	public class ThirdPersonCameraController : MonoBehaviour
	{
		[FormerlySerializedAs("crosshair")]
		[SerializeField, Tooltip("The aiming crosshair that is visible during strafe")]
		GameObject m_Crosshair;

		public ThirdPersonBrain m_ThirdPersonBrain;

		void Update()
		{
			if (m_ThirdPersonBrain == null)
			{
				Debug.LogError("No Third Person Brain in the scene", gameObject);
				gameObject.SetActive(false);
				return;
			}
            SetCrosshairVisible(m_ThirdPersonBrain.IsStrafing);
		}

		void SetCrosshairVisible(bool isVisible = true)
		{
			if (m_Crosshair != null)
    			m_Crosshair.SetActive(isVisible);
		}
	}
}