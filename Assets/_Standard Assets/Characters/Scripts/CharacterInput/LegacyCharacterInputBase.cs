using System;
using Cinemachine;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Abstract base class for all Legacy input implementations
	/// </summary>
	public abstract class LegacyCharacterInputBase : MonoBehaviour, ICharacterInput
	{
		[Header("Cinemachine Axes")]
		[SerializeField, Tooltip("The name of the horizontal looking axis setup on the Cinemachine camera")]
		protected string cinemachineLookXAxisName = "Horizontal";

		[SerializeField, Tooltip("The name of the vertical looking axis setup on the Cinemachine camera")]
		protected string cinemachineLookYAxisName = "Vertical";
		
		//The backing field of the moveInput property
		protected Vector2 moveInputVector;
		
		//The backing field of the lookInput property
		protected Vector2 lookInputVector;
		
		//The backing field for the jumpPressed property
		protected Action jumped;

		/// <inheritdoc />
		public Vector2 lookInput
		{
			get { return lookInputVector; }
		}

		/// <inheritdoc />
		public Vector2 moveInput
		{
			get { return moveInputVector; }
		}

		/// <inheritdoc />
		public bool hasMovementInput
		{
			get { return moveInput != Vector2.zero; }
		}

		/// <inheritdoc />
		public bool hasJumpInput { get; protected set; }

		/// <inheritdoc />
		public Action jumpPressed
		{
			get { return jumped; }
			set { jumped = value; }
		}
		
		/// <summary>
		/// Subscribe to the Cinemachine GetInputAxis delegate on enable
		/// </summary>
		private void OnEnable()
		{
			CinemachineCore.GetInputAxis = LookInputOverride;
		}
		
		/// <summary>
		/// Maps Cinemachine axis names to the look input vector
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
		
		/// <summary>
		/// Update the look and move vectors
		/// </summary>
		protected virtual void Update()
		{
			UpdateLookVector();
			UpdateMoveVector();
		}

		/// <summary>
		/// Mechanism for updating the Look Vector
		/// </summary>
		protected abstract void UpdateLookVector();
		
		/// <summary>
		/// Mechanism for updating the Move Vector
		/// </summary>
		protected abstract void UpdateMoveVector();
	}
}