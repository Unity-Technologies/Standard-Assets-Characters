using UnityEngine;

namespace Demo.StrafeTest
{
	[RequireComponent(typeof(Animator))]
	public class StrafeTestController : MonoBehaviour
	{
		[SerializeField]
		protected float moveVelocity = 5;
	
		private Animator animator;
	
		private void Awake ()
		{
			animator = GetComponent<Animator>();
		}
	
		private void Update ()
		{
			float x = Input.GetAxis("Horizontal"),
			      z = Input.GetAxis("Vertical");
			animator.SetFloat("Forward", z);
			animator.SetFloat("Lateral", x);
		
			var input = new Vector3(x, 0, z);
			if (input.magnitude > 1)
			{
				input.Normalize();
			}
			transform.position += input * moveVelocity * Time.deltaTime;
		}
	}
}
