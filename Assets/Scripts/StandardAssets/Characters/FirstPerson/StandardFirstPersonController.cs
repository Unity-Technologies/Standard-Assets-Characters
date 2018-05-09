using System.Collections;
using System.Collections.Generic;
using UnityInput = UnityEngine.Input;
using UnityEngine;




namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// First Person controler based of unity standard 
	/// </summary>

	[RequireComponent(typeof(CharacterController))]


	public class StandardFirstPersonController : MonoBehaviour
	{


		
		[SerializeField] private float m_StickToGroundForce;
		[SerializeField] private float m_RunSpeed;
		
		public MouseLook m_MouseLook;

		public Camera m_Camera;
		private Vector2 m_Input;
		private Vector3 m_MoveDir = Vector3.zero;
		private CharacterController m_CharacterController;
		private CollisionFlags m_CollisionFlags;

		private Vector3 m_OriginalCameraPosition;

		// Use this for initialization
		void Start()
		{
			m_CharacterController = GetComponent<CharacterController>();
			m_OriginalCameraPosition = m_Camera.transform.localPosition;
			m_MouseLook.Init(transform, m_Camera.transform);
		}

		// Update is called once per frame
		void Update()
		{
			RotateView();


			// the jump state needs to read here to make sure it is not missed

		}




		private void FixedUpdate()
		{
			float speed;
			GetInput(out speed);
			// always move along the camera forward as it is the direction that it being aimed at
			Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

			// get a normal for the surface that is being touched to move along it
			RaycastHit hitInfo;
			//Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
			//	m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
			//desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

			m_MoveDir.x = desiredMove.x * speed;
			m_MoveDir.z = desiredMove.z * speed;
			m_MoveDir.y = -m_StickToGroundForce;


			m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

			// UpdateCameraPosition(speed);

			m_MouseLook.UpdateCursorLock();
		}



		private void GetInput(out float speed)
		{
			// Read input
			float horizontal = UnityInput.GetAxis("Horizontal");
			float vertical = UnityInput.GetAxis("Vertical");




			// On standalone builds, walk/run speed is modified by a key press.
			// keep track of whether or not the character is walking or running
			//m_IsWalking = !Input.GetKey(KeyCode.LeftShift);

			// set the desired speed to be walking or running
			speed = m_RunSpeed;

			m_Input = new Vector2(horizontal, vertical);

			// normalize input if it exceeds 1 in combined length:
			if (m_Input.sqrMagnitude > 1)
			{
				m_Input.Normalize();
			}


		}


		private void RotateView()
		{
			m_MouseLook.LookRotation(transform, m_Camera.transform);
		}


		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			Rigidbody body = hit.collider.attachedRigidbody;
			//dont move the rigidbody if the character is on top of it
			if (m_CollisionFlags == CollisionFlags.Below)
			{
				return;
			}

			if (body == null || body.isKinematic)
			{
				return;
			}

			body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
		}

		

	}
}
