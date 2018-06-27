using UnityEngine;

namespace Demo
{
	public class ClayShootingController : MonoBehaviour
	{
		/// <summary>
		/// Gets required components
		/// </summary>
		[SerializeField]
		protected Rigidbody clayBullet;

		//public int direction;
		public Transform spawnLocation;
		public int speed;
		
		float m_Counter = 5;

		void Update()
		{
			m_Counter -= Time.deltaTime;

			if (!(m_Counter < 0))
			{
				return;
			}

			Shoot();
			m_Counter = 5;
		}

		void Shoot()
		{
			Rigidbody shootBullet = Instantiate(clayBullet, spawnLocation.position, spawnLocation.rotation);
			shootBullet.velocity = (spawnLocation.forward * speed);
			Debug.Log("is shooting them bullets");
		}
	}
}