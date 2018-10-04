using UnityEngine;

namespace StandardAssets.Characters.Examples.SimpleNavMeshInputController
{
	public class PlatformController : MonoBehaviour {

		public Transform[] platformPos;
		public float speed;

		public int currentPos = 0; 

		void Update () {
   
			transform.position = Vector3.MoveTowards(transform.position, platformPos[currentPos].transform.position, speed * Time.deltaTime);

			if (transform.position == platformPos[currentPos].transform.position)
			{
				currentPos += 1;
			}

			if (currentPos >= platformPos.Length)
			{
				currentPos = 0;
			}
		}
	}
}
