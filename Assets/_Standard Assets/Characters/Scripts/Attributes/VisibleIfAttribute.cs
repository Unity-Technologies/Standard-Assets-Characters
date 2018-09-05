using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// Attribute used to place a condition on the visibility of the relevant field in the inspector, determined by a conditionField and conditionElement
	/// <example>
	/// <code>
	///		[VisibleIf("conditionType", conditionType.Element)]
	/// 	protected string visibleIfElementSelected;
	///
	/// 	[VisibleIf("conditionBool")]
	/// 	protected string visibleIfConditionTrue;
	/// </code>
	/// </example>
	/// </summary>
	public class VisibleIfAttribute : PropertyAttribute
	{
		/// <summary>
		/// The name of the inspector field that will contain the elements of the condition that determines whether this field is Visible.
		/// </summary>
		public readonly string conditionField;
		
		/// <summary>
		/// The element in the conditionField that needs to be matched to decide this attribute's field visibility
		/// </summary>
		public readonly object conditionElement;
		
		/// <summary>
		/// Creates a new instance of <see cref="VisibleIfAttribute"/> with the given conditionalField and conditionElement
		/// </summary>
		public VisibleIfAttribute(string conditionField, object conditionElement = null)
		{
			this.conditionField = conditionField;
			this.conditionElement = conditionElement;
		}
	}
}