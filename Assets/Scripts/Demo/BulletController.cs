using UnityEngine;

namespace Demo
{
	public class BulletController : MonoBehaviour
	{
		void Start()
		{
			Destroy(this.gameObject, 5);
		}
	}
}