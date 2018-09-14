using StandardAssets.Characters.CharacterInput;
using UnityEngine;

namespace StandardAssets.Characters.Common
{
	/// <summary>
	/// Needed to play dummy animations so that the SDC can respond
	/// </summary>
	[RequireComponent(typeof(Animator))]
	public class CameraAnimationManager : MonoBehaviour
	{
		/// <summary>
		/// The animator
		/// </summary>
		private Animator animator;

		/// <summary>
		/// Cache the animator
		/// </summary>
		private void Awake()
		{
			LazyLoadAnimator();
		}

		/// <summary>
		/// Update cameras based on the input options (e.g. invert Y).
		/// </summary>
		protected virtual void Start()
		{
			if (InputOptions.Instance != null)
			{
				InputOptions.Instance.UpdateCinemachineCameras(gameObject);
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
		/// 
		/// </summary>
		/// <param name="state"></param>
		public void SetAnimation(string state, int layer = 0)
		{
			LazyLoadAnimator();
			animator.Play(state,layer);
		}
	}
}