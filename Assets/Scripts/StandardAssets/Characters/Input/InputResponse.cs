using System;
using UnityEngine;

namespace StandardAssets.Characters.Input
{
	public abstract class InputResponse : ScriptableObject
	{
		public event Action enabled, disabled;

		public abstract void Init();
		
		public abstract void Tick();

		protected virtual void OnEnabled()
		{
			if (enabled == null)
			{
				return;
			}

			enabled();
		}

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