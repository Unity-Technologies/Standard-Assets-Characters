using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// An attribute to disable inspector functionality allowing a field to be edited when the Unity Editor enters "run" mode
	/// </summary>
	public class DisableEditAtRuntimeAttribute : PropertyAttribute
	{
		public readonly bool enableIcon;

		/// <summary>
		/// Constructor 
		/// </summary>
		public DisableEditAtRuntimeAttribute(bool enableIcon = true)
		{
			this.enableIcon = enableIcon;
		}
	}
}