using System;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// A modifier input used by the FirstPersonMotorStateModification
	/// Allows that to be agnostic of the input system being used
	/// </summary>
	public abstract class InputResponse : ScriptableObject
	{

		private bool paused;
		/// <summary>
		/// Events when the input starts (enabled) and stops (disabled)
		/// </summary>
		public event Action started, ended;

		/// <summary>
		/// Initialization
		/// </summary>
		public abstract void Init();

		/// <summary>
		/// Safely broadcasts the enable event
		/// </summary>
		protected virtual void OnInputStarted()
		{
			if (paused)
			{
				return;
			}
			if (started == null)
			{
				return;
			}
			started();
		}

		/// <summary>
		/// Safely broadcasts the disable event
		/// </summary>
		protected virtual void OnInputEnded()
		{
			if (paused)
			{
				return;
			}
			if (ended == null)
			{
				return;
			}
			ended();
		}

		public void PauseResponse()
		{
			paused = true;
		}

		public void ResumeResponse()
		{
			paused = false;
		}
	}
}