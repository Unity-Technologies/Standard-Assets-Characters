using System;
using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// Subtype Enum to determine which type of helperbox is being used in order to generate the correct icon in the Unity Editor
	/// </summary>
	public enum HelperType
	{
		None,
		Info,
		Warning,
		Error,
	}

	/// <summary>
	/// An attribute used to generate a block of text above a field that should display relevant information about the field
	/// Also has the ability to show a built-in icon based on the HelperType element specified
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class HelperBoxAttribute : PropertyAttribute
	{
		public readonly string text;
		public readonly HelperType type;

		/// <summary>
		/// Constructor
		/// </summary>
		public HelperBoxAttribute(HelperType type,string text)
		{
			// This attribute should take priority over ConditionalInclude.
			base.order = 1000;
			this.type = type;
			this.text = text;
		}
	}
}

