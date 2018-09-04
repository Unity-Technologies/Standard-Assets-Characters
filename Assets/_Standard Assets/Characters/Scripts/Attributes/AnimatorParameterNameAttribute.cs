using System;
using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// Attribute used by <see cref="AnimatorParameterNamePropertyDrawer"/> to draw a drop down of
	/// <see cref="AnimatorControllerParameter"/>.
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