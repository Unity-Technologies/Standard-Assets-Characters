using StandardAssets.Characters.Attributes;
using UnityEditor;
using UnityEngine; 

namespace Editor
{
#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(HelperBoxAttribute))]
	public class HelperBoxPropertyDrawer : PropertyDrawer
	{
		private const float k_Spacing = 3f;
		private const float k_IconSize = 50f;
		private float height = 0;
		private float textHeight = 0;
		
		private HelperBoxAttribute HelpAttribute
		{
			get { return (HelperBoxAttribute) attribute; }
		}

		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
		{
			// check for Conditional (custom extension).
			VisibleIfAttribute compat = this.TryGetAttribute<VisibleIfAttribute>();
			if (compat != null)
			{ 
				// if Height is zero, then this property is hidden.
				float h =
					VisibleIfPropertyDrawer.GetConditionalPropertyDrawerHeight(compat, prop, label);
				if (h < 1)
				{
					return 0;
				}
			}
			// carry on as usual:
			
			height = base.GetPropertyHeight(prop, label);
			var content = new GUIContent(HelpAttribute.text);
			var style = GUI.skin.GetStyle("helpbox");
			textHeight = style.CalcHeight(content, EditorGUIUtility.currentViewWidth - k_IconSize);
			return height + textHeight + k_Spacing;
		}


		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			if (position.height < 1)
			{
				// if hidden by conditional, do nothing.
				return;
			}
			EditorGUI.BeginProperty(position, label, prop);
			
			var helpPos = position;
			helpPos.height = textHeight;

			EditorGUI.HelpBox(helpPos, HelpAttribute.text, (MessageType)HelpAttribute.type);
			position.height = height;

			position.y +=  k_Spacing + helpPos.height;
				EditorGUI.PropertyField(position, prop, label);

			EditorGUI.EndProperty();
		}
	}
#endif
}