using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCamera_Test : MonoBehaviour
{
	public bool lockCursor;
	public float mouseSensitivity;
	public Transform target;
	public float distanceFromTarget;
	public Vector2 pitchMinMax = new Vector2(-40, 85);
	public float rotationSmoothTime = 0.1f;
	
	
	//Private Variables
	private Vector3 m_rotationSmoothingVector;
	private Vector3 m_currentRotationVector;
	private float m_pitch;
	private float m_yaw;

	void Start()
	{
		if (lockCursor)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	void LateUpdate()
	{
		m_yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
		m_pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
		m_pitch = Mathf.Clamp(m_pitch, pitchMinMax.x, pitchMinMax.y);

		m_currentRotationVector = Vector3.SmoothDamp(m_currentRotationVector, new Vector3(m_pitch, m_yaw), ref m_rotationSmoothingVector, rotationSmoothTime);
		transform.eulerAngles = m_currentRotationVector;

		Vector3 e = transform.eulerAngles;
		e.x = 0;

		target.eulerAngles = e;
		transform.position = target.position - transform.forward * distanceFromTarget;
	}
}
