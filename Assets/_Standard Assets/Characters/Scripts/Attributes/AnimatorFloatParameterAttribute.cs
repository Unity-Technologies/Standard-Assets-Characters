using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// Attribute used by <see cref="StandardAssets.Characters.Attributes.Editor.AnimatorFloatParameterPropertyDrawer"/>
	/// to draw a drop down of <a href="http://docs.unity3d.com/ScriptReference/AnimatorControllerParameter.html">AnimatorControllerParameters</a>
	/// for a <see cref="StandardAssets.Characters.ThirdPerson.AnimationFloatParameter"/> to select the parameter name.
	/// </summary>
	/// <remarks>Only to be used on a <see cref="StandardAssets.Characters.ThirdPerson.AnimationFloatParameter"/> field.</remarks>
	public class AnimatorFloatParameterAttribute : PropertyAttribute
	{
		public readonly string animatorField;
        
		public AnimatorFloatParameterAttribute(string animatorField)
		{
			this.animatorField = animatorField;
		}
	}
}