using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// An attribute used to ensure a field in the Inspector is read-only when the editor enters play mode.
	/// </summary>
	public class DisableEditAtRuntimeAttribute : PropertyAttribute
	{
		/// <summary>
		/// Boolean used to enable the drawing of a predefined icon in the inspector when the field is read-only.
		/// </summary>
		public readonly bool enableIcon;

		/// <summary>
		/// /// Creates a new instance of <see cref="DisableEditAtRuntimeAttribute"/> with an option to enable a predefined icon
		/// </summary>
		public DisableEditAtRuntimeAttribute(bool enableIcon = true)
		{
			this.enableIcon = enableIcon;
		}
	}
}