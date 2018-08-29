using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// An attribute to make a field to be in a disabled state when the Unity Editor enters "run" mode
	/// </summary>
	public class DisableAtRuntimeAttribute : PropertyAttribute
	{
		public readonly bool enableIcon;

		/// <summary>
		/// Constructor 
		/// </summary>
		public DisableAtRuntimeAttribute(bool enableIcon = true)
		{
			this.enableIcon = enableIcon;
		}
	}
}