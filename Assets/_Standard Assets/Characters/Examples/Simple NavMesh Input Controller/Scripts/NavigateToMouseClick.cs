using UnityEngine;
using UnityEngine.AI;

namespace StandardAssets.Characters.Examples.SimpleNavMeshInputController
{
	[RequireComponent(typeof(NavMeshAgent))]
	public class NavigateToMouseClick : MonoBehaviour
	{
		[SerializeField]
		protected Camera mainCamera;
		
		/// <summary>
		/// Layers to use in the ground check
		/// </summary>
		[SerializeField, Tooltip("Layers to use in the ground check")]
		protected LayerMask groundCheckMask;

		NavMeshAgent navMesh;

		void Start()
		{
			if (mainCamera == null)
			{
				mainCamera = Camera.main;
			}

			navMesh = GetComponent<NavMeshAgent>();
		}

		public void Update()
		{
			if (UnityEngine.Input.GetKeyDown(KeyCode.Mouse0))
			{
				var ray = mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
				RaycastHit hit;

				if (UnityEngine.Physics.Raycast(ray, out hit, float.MaxValue, groundCheckMask))
				{
					navMesh.SetDestination(hit.point);
				}
			}
		}
	}
}