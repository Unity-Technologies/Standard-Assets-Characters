using System;
using UnityEngine;

namespace Demo
{
	public class TurnSpeedOptions : MonoBehaviour
	{
		[SerializeField]
		protected Animator animator;
		
		[SerializeField]
		protected AnimationProperties[] properties = new AnimationProperties[10];

		private void Awake()
		{
			SetAnimationProperties(0);
		}

		// Update is called once per frame
		private void Update()
		{
			HandleKeyInput(KeyCode.Alpha0, 0);
			HandleKeyInput(KeyCode.Alpha1, 1);
			HandleKeyInput(KeyCode.Alpha2, 2);
			HandleKeyInput(KeyCode.Alpha3, 3);
			HandleKeyInput(KeyCode.Alpha4, 4);
			HandleKeyInput(KeyCode.Alpha5, 5);
			HandleKeyInput(KeyCode.Alpha6, 6);
			HandleKeyInput(KeyCode.Alpha7, 7);
			HandleKeyInput(KeyCode.Alpha8, 8);
			HandleKeyInput(KeyCode.Alpha9, 9);
		}

		private void HandleKeyInput(KeyCode key, int index)
		{
			if (Input.GetKeyDown(key))
			{
				SetAnimationProperties(index);
			}
		}

		private void SetAnimationProperties(int index)
		{
			AnimationProperties selectedProperties = properties[index];
			
			animator.SetFloat("WalkRapidTurnOffset", selectedProperties.walkRapidTurnOffset);
			animator.SetFloat("RunRapidTurnOffset", selectedProperties.runRapidTurnOffset);
			animator.SetFloat("WalkRapidTurnSpeed", selectedProperties.walkRapidTurnSpeed);
			animator.SetFloat("RunRapidTurnSpeed", selectedProperties.runRapidTurnSpeed);
		}
	}

	[Serializable]
	public class AnimationProperties
	{
		public string name;

		[Range(0,1)]
		public float walkRapidTurnOffset, runRapidTurnOffset;

		public float walkRapidTurnSpeed = 1f, runRapidTurnSpeed = 1f;
	}
}