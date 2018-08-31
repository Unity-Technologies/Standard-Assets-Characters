using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Class to handle rapid turns
	/// </summary>
	public abstract class TurnaroundBehaviour
	{
		/// <summary>
		/// Value multiplicatively applied to the head look at turn angle
		/// </summary>
		public abstract float headTurnScale { get; }
		
		/// <summary>
		/// Event fired on completion of turnaround
		/// </summary>
		public event Action turnaroundComplete;
		
		/// <summary>
		/// Whether this object is currently handling a turnaround motion
		/// </summary>
		protected bool isTurningAround;

		public abstract void Init(ThirdPersonBrain brain);

		public abstract void Update();

		/// <summary>
		/// Gets the movement of the character
		/// </summary>
		/// <returns>Movement to apply to the character</returns>
		public abstract Vector3 GetMovement();

		/// <summary>
		/// Starts a turnaround
		/// </summary>
		/// <param name="angle">Target y rotation in degrees</param>
		public void TurnAround(float angle)
		{
			if (isTurningAround)
			{
				return;
			}

			isTurningAround = true;
			StartTurningAround(angle);
		}

		/// <summary>
		/// Called on completion of turnaround. Fires <see cref="turnaroundComplete"/>  event.
		/// </summary>
		protected void EndTurnAround()
		{
			isTurningAround = false;
			FinishedTurning();
			if (turnaroundComplete != null)
			{
				turnaroundComplete();
			}
		}

		protected abstract void FinishedTurning();

		protected abstract void StartTurningAround(float angle);
	}
}