using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public abstract class BaseThirdPersonMotor : MonoBehaviour, IThirdPersonMotor
	{
		/// <summary>
		/// The actual turn speed
		/// </summary>
		public float turnSpeed = 300f;
		
		protected float m_PreviousYRotation;

		protected float m_NormalizedTurningSpeed;

		public float normalizedTurningSpeed
		{
			get { return m_NormalizedTurningSpeed; }
		}

		public abstract float normalizedLateralSpeed { get; }
		public abstract float normalizedForwardSpeed { get; }
		public Action jumpStarted { get; set; }
		public Action landed { get; set; }
		
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
		protected void CalculateYRotationSpeed()
		{
			float currentYRotation = Wrap180(transform.rotation.eulerAngles.y);
			float yRotationSpeed = Wrap180(currentYRotation - m_PreviousYRotation) / Time.deltaTime;
			m_NormalizedTurningSpeed = Mathf.Clamp(yRotationSpeed / turnSpeed, -1, 1);
			m_PreviousYRotation = currentYRotation;
		}
	}
}