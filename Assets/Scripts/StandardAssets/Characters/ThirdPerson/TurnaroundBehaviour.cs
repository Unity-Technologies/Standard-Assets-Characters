using System;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	public abstract class TurnaroundBehaviour : MonoBehaviour
	{
		public Action turnaroundComplete;
		
		protected bool isTurningAround;
		
		public void TurnAround(float angle)
		{
			if (isTurningAround)
			{
				return;
			}

			isTurningAround = true;
			StartTurningAround(angle);
		}

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