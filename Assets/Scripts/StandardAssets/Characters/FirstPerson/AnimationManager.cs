using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof(Animator))]
	public class AnimationManager : MonoBehaviour
	{
		/// <summary>
		/// The animator
		/// </summary>
		Animator m_Animator;

		void Awake()
		{
			m_Animator = GetComponent<Animator>();
		}

		public void SetAnimation(AnimationState state)
		{
			m_Animator.Play(state.GetHashCode());
		}
		
		public void SetAnimation(string state)
		{
			m_Animator.Play(state);
		}
	}
}