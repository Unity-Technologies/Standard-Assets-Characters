using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	public class AnimatorStateNameAttribute : PropertyAttribute
	{
		public readonly string animatorField;
        
		public AnimatorStateNameAttribute(string animatorField)
		{
			this.animatorField = animatorField;
		}
	}
}