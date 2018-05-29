using UnityEngine;

namespace Util
{
	/// <summary>
	/// Extended monobehaviour that allows the user to choose which Update it uses from the inspector
	/// </summary>
	public abstract class ConfigurableUpdateBehaviour : MonoBehaviour
	{
		/// <summary>
		/// Choose where the update happens
		/// </summary>
		public ConfigurableUpdateType updateOn;
		
		/// <summary>
		/// The configurable update
		/// </summary>
		/// <param name="scaledDeltaTime">The scale time delta e.g. Time.deltaTime for Update</param>
		/// <param name="unscaledDeltaTime">The unscaled time delta e.g. Time.unscaledDeltaTime for Update</param>
		protected abstract void ConfigurableUpdate(float scaledDeltaTime, float unscaledDeltaTime);
	
		protected virtual void Update()
		{
			if (updateOn != ConfigurableUpdateType.Update)
			{
				return;
			}
			
			ConfigurableUpdate(Time.deltaTime, Time.unscaledDeltaTime);
		}
		
		protected virtual void LateUpdate()
		{
			if (updateOn != ConfigurableUpdateType.LateUpdate)
			{
				return;
			}
			
			ConfigurableUpdate(Time.deltaTime, Time.unscaledDeltaTime);
		}
		
		protected virtual void FixedUpdate()
		{
			if (updateOn != ConfigurableUpdateType.FixedUpdate)
			{
				return;
			}
			
			ConfigurableUpdate(Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime);
		}
	}
}