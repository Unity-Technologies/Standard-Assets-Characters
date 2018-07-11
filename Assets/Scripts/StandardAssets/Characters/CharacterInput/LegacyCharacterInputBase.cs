using System;
using Cinemachine;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	public abstract class LegacyCharacterInputBase : MonoBehaviour, ICharacterInput
	{
		[Header("Cinemachine Axes")]
		[SerializeField]
		protected string cinemachineLookXAxisName = "Horizontal";

		[SerializeField]
		protected string cinemachineLookYAxisName = "Vertical";
		
		protected Vector2 moveInputVector;
		protected Action jumped;

		protected Vector2 lookInputVector;
		
		public Vector2 lookInput
		{
			get { return lookInputVector; }
		}

		public Vector2 moveInput
		{
			get { return moveInputVector; }
		}

		public bool hasMovementInput
		{
			get { return moveInput != Vector2.zero; }
		}

		public Action jumpPressed
		{
			get { return jumped; }
			set { jumped = value; }
		}
		
		private void OnEnable()
		{
			CinemachineCore.GetInputAxis = LookInputOverride;
		}
		
		/// <summary>
		/// Sets the Cinemachine cam POV to mouse inputs.
		/// </summary>
		private float LookInputOverride(string cinemachineAxisName)
		{
			if (cinemachineAxisName == cinemachineLookXAxisName)
			{
				return lookInput.x;
			}

			if (cinemachineAxisName == cinemachineLookYAxisName)
			{
				return lookInput.y;
			}

			return 0;
		}
		
		protected virtual void Update()
		{
			UpdateLookVector();
			UpdateMoveVector();
		}

		protected abstract void UpdateLookVector();
		protected abstract void UpdateMoveVector();
	}
}