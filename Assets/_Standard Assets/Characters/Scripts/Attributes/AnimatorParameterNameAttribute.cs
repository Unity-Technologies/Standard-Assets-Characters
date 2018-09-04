using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// Attribute used by <see cref="StandardAssets.Characters.Attributes.Editor.AnimatorParameterNamePropertyDrawer"/>
	/// to draw a drop down of <a href="http://docs.unity3d.com/ScriptReference/AnimatorControllerParameter.html">AnimatorControllerParameters</a>.
	/// Optionally filtered by <see cref="type"/>.
	/// </summary>
	public class AnimatorParameterNameAttribute : PropertyAttribute
	{
		public readonly string animatorField;
		public AnimatorControllerParameterType? type;
        
		public AnimatorParameterNameAttribute(string animatorField, AnimatorControllerParameterType type)
		{
			this.animatorField = animatorField;
			this.type = type;
		}
		
		public AnimatorParameterNameAttribute(string animatorField)
		{
			this.animatorField = animatorField;
			type = null;
		}
	}
}