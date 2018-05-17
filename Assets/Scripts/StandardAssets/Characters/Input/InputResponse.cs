using System;
using UnityEngine;

namespace StandardAssets.Characters.Input
{
	/// <summary>
	/// A modifier input used by the FirstPersonMotorStateModification
	/// Allows that to be agnostic of the input system being used
	/// </summary>
	public abstract class InputResponse : ScriptableObject
	{
		/// <summary>
		/// Events when the input starts (enabled) and stops (disabled)
		/// </summary>
		public event Action enabled, disabled;

		/// <summary>
		/// Initialization
		/// </summary>
		public abstract void Init();
		
		/// <summary>
		/// Tick/Update
		/// </summary>
		public abstract void Tick();

		/// <summary>
		/// Safely broadcasts the enable event
		/// </summary>
		protected virtual void OnEnabled()
		{
			if (enabled == null)
			{
				return;
			}

			enabled();
		}

		/// <summary>
		/// Safely broadcasts the disable event
		/// </summary>
		protected virtual void OnDisabled()
		{
			if (disabled == null)
			{
				return;
			}

			disabled();
		}
	}
}