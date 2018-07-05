using System;
using UnityEngine;

namespace Demo
{
	public class TurnSpeedOptions : MonoBehaviour
	{
		[SerializeField]
		protected Animator animator;

		[SerializeField]
		protected TurnSpeedOptionsUiController controller;
		
		[SerializeField]
		protected AnimationProperties[] properties = new AnimationProperties[10];

		private void Awake()
		{
			if (controller != null)
			{
				controller.Init(properties);	
			}
			
			SetAnimationProperties(0);
		}

		// Update is called once per frame
		private void Update()
		{
			HandleKeyInput(KeyCode.Alpha1, 0);
			HandleKeyInput(KeyCode.Alpha2, 1);
			HandleKeyInput(KeyCode.Alpha3, 2);
			HandleKeyInput(KeyCode.Alpha4, 3);
			HandleKeyInput(KeyCode.Alpha5, 4);
			HandleKeyInput(KeyCode.Alpha6, 5);
			HandleKeyInput(KeyCode.Alpha7, 6);
			HandleKeyInput(KeyCode.Alpha8, 7);
			HandleKeyInput(KeyCode.Alpha9, 8);
			HandleKeyInput(KeyCode.Alpha0, 9);
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
			
			if (controller != null)
			{
				controller.SetIndex(index);	
			}
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