using UnityEngine;
using UnityEngine.AI;

namespace StandardAssets.Characters.Examples.SimpleNavMeshInputController
{
	/// <summary>
	/// Simple mouse click input for Nav mesh character
	/// </summary>
	[RequireComponent(typeof(NavMeshAgent))]
	public class NavigateToMouseClick : MonoBehaviour
	{		
		/// <summary>
		/// Layers to use in the ground check
		/// </summary>
		[SerializeField, Tooltip("Layers to use in the ground click check")]
		LayerMask m_GroundCheckMask;

		Camera m_MainCamera;
		NavMeshAgent m_NavMesh;

		/// <summary>
		/// Caches main camera and get reference to NavMeshAgent
		/// </summary>
		void Start()
		{
			if (m_MainCamera == null)
			{
				m_MainCamera = Camera.main;
			}

			m_NavMesh = GetComponent<NavMeshAgent>();
		}

		/// <summary>
		/// Checks for click intersecting the world
		/// </summary>
		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				var ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				if (UnityEngine.Physics.Raycast(ray, out hit, float.MaxValue, m_GroundCheckMask))
				{
					m_NavMesh.SetDestination(hit.point);
				}
			}
		}
	}
}