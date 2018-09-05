using System;
using Cinemachine;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Abstract base class for all Legacy input implementations
	/// </summary>
	/// <seealso cref="LegacyCharacterInput"/>
	/// <seealso cref="LegacyOnScreenCharacterInput"/>
	public abstract class LegacyCharacterInputBase : MonoBehaviour, ICharacterInput
	{
		/// <inheritdoc />
		public event Action jumpPressed;
		
		[Header("Cinemachine Axes")]
		[SerializeField, Tooltip("The name of the horizontal looking axis setup on the Cinemachine camera")]
		protected string cinemachineLookXAxisName = "Horizontal";

		[SerializeField, Tooltip("The name of the vertical looking axis setup on the Cinemachine camera")]
		protected string cinemachineLookYAxisName = "Vertical";
		
		//The backing field of the moveInput property
		protected Vector2 moveInputVector;
		
		//The backing field of the lookInput property
		protected Vector2 lookInputVector;

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
		
		/// <summary>
		/// Subscribe to the Cinemachine GetInputAxis delegate on enable
		/// </summary>
		protected virtual void OnEnable()
		{
			CinemachineCore.GetInputAxis = LookInputOverride;
		}
		
		/// <summary>
		/// Maps Cinemachine axis names to the look input vector
		/// </summary>
		/// <remarks>
		/// Currently Cinemachine passes the current axis to be updated as string
		/// Hence the need for string comparison
		/// </remarks>
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
		/// Helper method for firing jump
		/// </summary>
		protected virtual void OnJumpPressed()
		{
			if (jumpPressed != null)
			{
				jumpPressed();
			}
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