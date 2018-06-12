using UnityEngine;
using UnityEngine.AI;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// NavMesh click controls for Third Person NavMesh scene
	/// </summary>
	public class NavMeshClick : MonoBehaviour
	{
		[SerializeField]
		private NavMeshAgent agent;

		private void Update()
		{
			if (!Input.GetMouseButtonDown(0))
			{
				return;
			}

			Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
			RaycastHit hit;
			if (UnityEngine.Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Ground")))
			{
				agent.SetDestination(hit.point);
			}
		}
	}
}