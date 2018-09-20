using Cinemachine;
using StandardAssets.Characters.Attributes;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Input options that can be changed via an options menu in the game.
	/// </summary>
	public class InputOptions : MonoBehaviour
	{
		/// <summary>
		/// Invert camera Y axis. It overrides the Cinemachine virtual cameras' setting.
		/// </summary>
		[DisableEditAtRuntime, SerializeField, Tooltip("Invert camera Y axis. It overrides the Cinemachine virtual cameras' setting.")]
		protected bool cameraInvertY;
		
		/// <summary>
		/// Toggle the cursor lock mode while in play mode.
		/// </summary>
		[SerializeField, Tooltip("Toggle the Cursor Lock Mode, press ESCAPE during play mode")]
		protected bool cursorLocked = true;
		
		/// <summary>
		/// Instance property
		/// </summary>
		public static InputOptions Instance { get; private set; }

		/// <summary>
		/// Update invert Y of all the CinemachineStateDrivenCameras on the specified game object, and the game object's children.
		/// </summary>
		public void UpdateCinemachineCameras(GameObject onGameObject)
		{
			CinemachineStateDrivenCamera[] stateDrivenCameras = onGameObject.GetComponentsInChildren<CinemachineStateDrivenCamera>(true);
			if (stateDrivenCameras == null ||
			    stateDrivenCameras.Length <= 0)
			{
				return;
			}

			for (int i = 0, len = stateDrivenCameras.Length; i < len; i++)
			{
				CinemachineStateDrivenCamera stateDrivenCamera = stateDrivenCameras[i];
				if (stateDrivenCamera == null ||
				    stateDrivenCamera.ChildCameras == null ||
				    stateDrivenCamera.ChildCameras.Length <= 0)
				{
					continue;
				}
				
				for (int n = 0, lenN = stateDrivenCamera.ChildCameras.Length; n < lenN; n++)
				{
					UpdateCinemachineVirtualCamera(stateDrivenCamera.ChildCameras[n]);
				}
			}
		}

		/// <summary>
		/// Update invert Y of the CinemachineVirtualCameraBase.
		/// </summary>
		private void UpdateCinemachineVirtualCamera(CinemachineVirtualCameraBase virtualBase)
		{
			CinemachineVirtualCamera virtualCamera = virtualBase as CinemachineVirtualCamera;
			if (virtualCamera != null)
			{
				CinemachinePOV pov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
				if (pov != null)
				{
					pov.m_VerticalAxis.m_InvertInput = cameraInvertY;
				}
			}
					
			CinemachineFreeLook freeLook = virtualBase as CinemachineFreeLook;
			if (freeLook != null)
			{
				freeLook.m_YAxis.m_InvertInput = cameraInvertY;
			}
		}
		
		/// <summary>
		/// Singleton checks
		/// </summary>
		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
			}
			else
			{
				Instance = this;
			}

			ToggleCursorLockState();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				cursorLocked = !cursorLocked;
			}
			ToggleCursorLockState();
		}
		
		private void ToggleCursorLockState()
		{
			Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		}

		/// <summary>
		/// Singleton checks
		/// </summary>
		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}
	}
}