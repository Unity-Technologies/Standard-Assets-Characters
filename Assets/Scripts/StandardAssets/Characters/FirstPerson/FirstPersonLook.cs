using StandardAssets.Characters.Cameras;
using StandardAssets.Characters.Input;
using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// Handles the look using the current camera from the camera manager
	/// Refactor of MouseLook
	/// </summary>
	[RequireComponent(typeof(ICameraManager))]
	[RequireComponent(typeof(IInput))]
	public class FirstPersonLook : MonoBehaviour
	{
		public GameObject character;
	    public float xSensitivity = 2f;
	    public float ySensitivity = 2f;
	    public bool clampVerticalRotation = true;
	    public float minimumX = -90F;
	    public float maximumX = 90F;
	    public bool smooth;
	    public float smoothTime = 5f;
	    public bool lockCursor = true;
		
	    bool m_CursorIsLocked = true;
		ICameraManager m_CameraManager;
		IInput m_LookInput;

		Quaternion m_CharacterTargetRot;

		/// <summary>
		/// Cache required components
		/// </summary>
		void Awake()
		{
			m_CameraManager = GetComponent<ICameraManager>();
			m_LookInput = GetComponent<IInput>();
			m_CharacterTargetRot = character.transform.localRotation;
		}

		/// <summary>
		/// Look rotation updated every frame
		/// </summary>
		void Update()
		{
			LookRotation();
		}

		/// <summary>
		/// Gets the current camera from the camera manager and adjusts rotations
		/// </summary>
		void LookRotation()
		{
			GameObject currentCamera = m_CameraManager.currentCamera;
            float yRot = m_LookInput.lookInput.x * xSensitivity;
            float xRot = m_LookInput.lookInput.y * ySensitivity;

			m_CharacterTargetRot	*= Quaternion.Euler(0f, -yRot, 0f);
			Quaternion cameraTargetRot = currentCamera.transform.localRotation;
            cameraTargetRot *= Quaternion.Euler(xRot, 0f, 0f);

            if (clampVerticalRotation)
            {
	            cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);
            }

			if (smooth)
            {
                character.transform.localRotation = Quaternion.Slerp(character.transform.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                currentCamera.transform.localRotation = Quaternion.Slerp(currentCamera.transform.localRotation, cameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.transform.localRotation = m_CharacterTargetRot;
                currentCamera.transform.localRotation = cameraTargetRot;
            }

            UpdateCursorLock();
        }

		void SetCursorLock(bool value)
        {
            lockCursor = value;
	        if (lockCursor)
	        {
		        return;
	        }

	        //we force unlock the cursor if the user disable the cursor locking helper
	        Cursor.lockState = CursorLockMode.None;
	        Cursor.visible = true;
        }

		void UpdateCursorLock()
        {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (lockCursor)
            {
	            InternalLockUpdate();
            }
        }

		void InternalLockUpdate()
        {
            if (UnityInput.GetKeyUp(KeyCode.Escape))
            {
                m_CursorIsLocked = false;
            }
            else if (UnityInput.GetMouseButtonUp(0))
            {
                m_CursorIsLocked = true;
            }

            if (m_CursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_CursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

		/// <summary>
		/// Clamps the rotation
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, minimumX, maximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
	}
}