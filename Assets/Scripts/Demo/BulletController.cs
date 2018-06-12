using UnityEngine;

namespace Demo
{
	/// <summary>
	/// Helper for clay pigeon projectiles in the eventual FPS
	/// </summary>
	public class BulletController : MonoBehaviour
	{
		private void Start()
		{
			Destroy(this.gameObject, 5);
		}
	}
}