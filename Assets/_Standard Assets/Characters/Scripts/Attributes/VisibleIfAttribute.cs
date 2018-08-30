using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// Attribute used when one desires to make a field visible on a specified condition being fulfilled
	/// </summary>
	public class VisibleIfAttribute : PropertyAttribute
	{
		public readonly string conditionField;
		public readonly object conditionElement;
		
		/// <summary>
		/// Constructor
		/// </summary>
		public VisibleIfAttribute(string conditionField, object conditionElement = null)
		{
			this.conditionField = conditionField;
			this.conditionElement = conditionElement;
		}
	}
}