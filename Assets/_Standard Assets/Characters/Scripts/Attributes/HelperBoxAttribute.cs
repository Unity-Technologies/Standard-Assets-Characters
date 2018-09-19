using System;
using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// An attribute used to generate a block of text above a field that should display relevant information about the field
	/// Also has the ability to show a built-in icon based on the HelperType element specified
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class HelperBoxAttribute : PropertyAttribute
	{
		/// <summary>
		/// Enum to determine which type of helperbox is being used in order to generate the correct icon in the Unity Editor
		/// </summary>
		public enum HelperType
		{
			None,
			Info,
			Warning,
			Error,
		}
		
		/// <summary>
		/// The text that will be displayed in the rendered HelperBox in the Inspector
		/// </summary>
		public readonly string text;
		
		/// <summary>
		/// Enum element used to determine which icon is generated in the HelperBox in the Inspector
		/// </summary>
		public readonly HelperType type;

		/// <summary>
		/// Creates a new instance of <see cref="HelperBoxAttribute"/> with the given type and label
		/// </summary>
		public HelperBoxAttribute(HelperType type = HelperType.None, string text = "")
		{
			// This attribute should take priority over ConditionalInclude.
			order = 1000;
			this.type = type;
			this.text = text;
		}
	}
}

