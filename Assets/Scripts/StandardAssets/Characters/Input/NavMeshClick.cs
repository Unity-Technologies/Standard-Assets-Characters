using UnityEngine;
using UnityEngine.AI;

namespace StandardAssets.Characters.Input
{
	public class NavMeshClick : MonoBehaviour
	{
		public NavMeshAgent agent;

		void Update()
		{
			if (UnityEngine.Input.GetMouseButtonDown(0))
			{
				Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
				RaycastHit hit;
				if (UnityEngine.Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Ground")))
				{
					agent.SetDestination(hit.point);
				}
			}
		}
	}
}