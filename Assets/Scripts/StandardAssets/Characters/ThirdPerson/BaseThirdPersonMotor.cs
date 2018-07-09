using System;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Base implementation of ThirdPersonMotor which knows how to report the turning speed
	/// </summary>
	public abstract class BaseThirdPersonMotor : MonoBehaviour, IThirdPersonMotor
	{
		/// <summary>
		/// The actual turn speed
		/// </summary>
		[SerializeField]
		protected float turnSpeed = 300f;

		[Tooltip("Determines how quickly the normalized turning speed can change")]
		[Range(1, 20)]
		[SerializeField]
		protected float normalizedTurnLerpFactor = 5f;
		
		[SerializeField]
		protected Animator animator;
		
		protected float currentForwardSpeed;

		protected float currentLateralSpeed;
		
		/// <summary>
		/// Needed to calculate turning
		/// </summary>
		private float previousYRotation;

		/// <inheritdoc />
		public float normalizedTurningSpeed { get; private set; }

		/// <inheritdoc />
		public abstract float normalizedLateralSpeed { get; }
		
		/// <inheritdoc />
		public abstract float normalizedForwardSpeed { get; }
		
		public abstract float fallTime { get; }

		/// <inheritdoc />
		public Action jumpStarted { get; set; }
		
		/// <inheritdoc />
		public Action landed { get; set; }
				
		/// <inheritdoc />
		public Action<float> fallStarted { get; set; }

		public Action<float> rapidlyTurned { get; set; }

		public abstract void FinishedTurn();
		
		/// <summary>
		/// Calculates the rotations
		/// </summary>
		protected void CalculateYRotationSpeed(float deltaTime)
		{			
			float currentYRotation = MathUtilities.Wrap180(transform.rotation.eulerAngles.y);
			float yRotationSpeed = MathUtilities.Wrap180(currentYRotation - previousYRotation) / deltaTime;
			float targetNormalizedTurningSpeed = Mathf.Clamp(yRotationSpeed / turnSpeed, -1, 1);
			normalizedTurningSpeed = 
				Mathf.Lerp(normalizedTurningSpeed, targetNormalizedTurningSpeed, deltaTime * normalizedTurnLerpFactor);
			
			previousYRotation = currentYRotation;
		}

		/// <summary>
		/// Cache the previous rotation
		/// </summary>
		protected virtual void Awake()
		{
			previousYRotation = MathUtilities.Wrap180(transform.rotation.eulerAngles.y);
		}
	}
}