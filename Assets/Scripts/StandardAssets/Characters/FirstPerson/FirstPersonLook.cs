using StandardAssets.Characters.Cameras;
using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace StandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof(ICameraManager))]
	public class FirstPersonLook : MonoBehaviour
	{
		public GameObject character;
	    public float XSensitivity = 2f;
	    public float YSensitivity = 2f;
	    public bool clampVerticalRotation = true;
	    public float MinimumX = -90F;
	    public float MaximumX = 90F;
	    public bool smooth;
	    public float smoothTime = 5f;
	    public bool lockCursor = true;
		
	    private bool m_cursorIsLocked = true;
		ICameraManager m_CameraManager;

		Quaternion m_CharacterTargetRot;

		void Awake()
		{
			m_CameraManager = GetComponent<ICameraManager>();
			m_CharacterTargetRot = character.transform.localRotation;
		}

		void Update()
		{
			LookRotation();
		}

		void LookRotation()
		{
			GameObject camera = m_CameraManager.currentCamera;
            float yRot = UnityInput.GetAxis("Mouse X") * XSensitivity;
            float xRot = UnityInput.GetAxis("Mouse Y") * YSensitivity;

			m_CharacterTargetRot	*= Quaternion.Euler(0f, -yRot, 0f);
			Quaternion m_CameraTargetRot = camera.transform.localRotation;
            m_CameraTargetRot *= Quaternion.Euler(xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (smooth)
            {
                character.transform.localRotation = Quaternion.Slerp(character.transform.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.transform.localRotation = Quaternion.Slerp(camera.transform.localRotation, m_CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.transform.localRotation = m_CharacterTargetRot;
                camera.transform.localRotation = m_CameraTargetRot;
            }

            UpdateCursorLock();
        }

        public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if (!lockCursor)
            {
                //we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void UpdateCursorLock()
        {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (lockCursor)
                InternalLockUpdate();
        }

        private void InternalLockUpdate()
        {
            if (UnityInput.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }
            else if (UnityInput.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
	}
}