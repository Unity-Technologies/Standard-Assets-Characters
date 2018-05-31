using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// Needed to play dummy animations so that the SDC can respond
	/// </summary>
	[RequireComponent(typeof(Animator))]
	public class AnimationManager : MonoBehaviour
	{
		/// <summary>
		/// The animator
		/// </summary>
		Animator m_Animator;

		/// <summary>
		/// Cache the animator
		/// </summary>
		void Awake()
		{
			LazyLoad();
		}

		/// <summary>
		/// Lazy load protected against out of order operations
		/// </summary>
		void LazyLoad()
		{
			if (m_Animator != null)
			{
				return;
			}
			
			m_Animator = GetComponent<Animator>();
		}

		/// <summary>
		/// Ideally you would be able to drag in the state but this does not seem available in the inspector
		/// </summary>
		/// <param name="state"></param>
		public void SetAnimation(AnimationState state)
		{
			LazyLoad();
			m_Animator.Play(state.GetHashCode());
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		public void SetAnimation(string state)
		{
			LazyLoad();
			m_Animator.Play(state);
		}
	}
}