using System;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// A representation of a button-like Input that is not shared by all characters
	/// e.g. Strafe button is only used by Third Person character and not the First Person character
	/// Allows that to be agnostic of the input system being used
	/// </summary>
	public abstract class InputResponse : ScriptableObject
	{
		/// <summary>
		/// Event raised when the input starts being applied
		/// </summary>
		public event Action started;

		/// <summary>
		/// Event raised when the input stops being applied
		/// </summary>
		public event Action ended;

		/// <summary>
		/// Sets up the input response. In the case of the LegacyInputResponse the Init sets the poller objects needed detect input
		/// </summary>
		public abstract void Init();

		/// <summary>
		/// Safely broadcasts the enable event
		/// </summary>
		protected virtual void OnInputStarted()
		{
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
			if (ended == null)
			{
				return;
			}

			ended();
		}
	}
}