using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// Attribute used by <see cref="StandardAssets.Characters.Attributes.Editor.AnimatorStateNamePropertyDrawer"/>
	/// to display a drop down with a list of animator state names to be selected for a string field.
	/// </summary>
	public class AnimatorStateNameAttribute : PropertyAttribute
	{
		public readonly string animatorField;
        
		public AnimatorStateNameAttribute(string animatorField)
		{
			this.animatorField = animatorField;
		}
	}
}