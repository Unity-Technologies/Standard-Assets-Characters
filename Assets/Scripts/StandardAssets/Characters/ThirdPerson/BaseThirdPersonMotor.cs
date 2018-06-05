using System;
using UnityEngine;

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
		public float turnSpeed = 300f;

		[Tooltip("Determines how quickly the normalized turning speed can change")]
		[Range(1, 20)]
		public float normalizedTurnLerpFactor = 5f;
		
		/// <summary>
		/// Needed to calculate turning
		/// </summary>
		protected float m_PreviousYRotation;

		/// <summary>
		/// The actual normalized turning speed
		/// </summary>
		protected float m_NormalizedTurningSpeed;

		/// <inheritdoc />
		public float normalizedTurningSpeed
		{
			get { return m_NormalizedTurningSpeed; }
		}

		/// <inheritdoc />
		public abstract float normalizedLateralSpeed { get; }
		
		/// <inheritdoc />
		public abstract float normalizedForwardSpeed { get; }
		
		/// <inheritdoc />
		public Action jumpStarted { get; set; }
		
		/// <inheritdoc />
		public Action landed { get; set; }
		
		/// <summary>
		/// Helper function for handling wrapping of angles
		/// </summary>
		/// <param name="toWrap"></param>
		/// <returns></returns>
		protected float Wrap180(float toWrap)
		{
			while (toWrap < -180)
			{
				toWrap += 360;
			}
			
			while (toWrap > 180)
			{
				toWrap -= 360;
			}

			return toWrap;
		}
		
		/// <summary>
		/// Calculates the rotations
		/// </summary>
		protected void CalculateYRotationSpeed(float deltaTime)
		{
			float currentYRotation = Wrap180(transform.rotation.eulerAngles.y);
			float yRotationSpeed = Wrap180(currentYRotation - m_PreviousYRotation) / deltaTime;
			float targetNormalizedTurningSpeed = Mathf.Clamp(yRotationSpeed / turnSpeed, -1, 1);
			m_NormalizedTurningSpeed = 
				Mathf.Lerp(m_NormalizedTurningSpeed, targetNormalizedTurningSpeed, deltaTime * normalizedTurnLerpFactor);
			m_PreviousYRotation = currentYRotation;
		}

		/// <summary>
		/// Cache the previous rotation
		/// </summary>
		protected virtual void Awake()
		{
			m_PreviousYRotation = Wrap180(transform.rotation.eulerAngles.y);
		}
	}
}