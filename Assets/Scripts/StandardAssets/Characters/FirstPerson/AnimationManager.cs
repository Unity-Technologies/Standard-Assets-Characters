using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// Needed to play dummy animations so that the SDC can respond
	/// </summary>
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(FirstPersonBrain))]
	public class AnimationManager : MonoBehaviour
	{
		/// <summary>
		/// The animator
		/// </summary>
		private Animator animator;

		private FirstPersonBrain brain;

		/// <summary>
		/// Cache the animator
		/// </summary>
		private void Awake()
		{
			LazyLoadAnimator();
			LazyLoadController();
		}

		/// <summary>
		/// Subscribe
		/// </summary>
		private void OnEnable()
		{
			LazyLoadController();
			foreach (FirstPersonMovementProperties firstPersonMovementProperties in brain.allMovementProperties)
			{
				firstPersonMovementProperties.enterState += SetAnimation;
			}
		}
		
		/// <summary>
		/// Unsubscribe
		/// </summary>
		private void OnDisable()
		{
			foreach (FirstPersonMovementProperties firstPersonMovementProperties in brain.allMovementProperties)
			{
				firstPersonMovementProperties.enterState -= SetAnimation;
			}
		}

		/// <summary>
		/// Lazy load protected against out of order operations
		/// </summary>
		private void LazyLoadAnimator()
		{
			if (animator != null)
			{
				return;
			}
			
			animator = GetComponent<Animator>();
		}

		/// <summary>
		/// Lazy load of controller
		/// </summary>
		private void LazyLoadController()
		{
			if (brain != null)
			{
				return;
			}

			brain = GetComponent<FirstPersonBrain>();
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		protected void SetAnimation(string state)
		{
			LazyLoadAnimator();
			animator.Play(state);
		}
	}
}