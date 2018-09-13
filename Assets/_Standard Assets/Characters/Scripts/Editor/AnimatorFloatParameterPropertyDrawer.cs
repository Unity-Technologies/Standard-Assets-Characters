using System.Collections.Generic;
using System.Linq;
using Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace StandardAssets.Characters.Attributes.Editor
{
	/// <summary>
	/// Custom property drawer for the <see cref="AnimatorFloatParameterAttribute"/>.
	/// This will use the given animator to retrieve the float parameters and display a drop down for the parameter
	/// name of a <see cref="StandardAssets.Characters.ThirdPerson.AnimationFloatParameter"/>.
	/// </summary>
	[CustomPropertyDrawer(typeof(AnimatorFloatParameterAttribute))]
	public class AnimatorFloatParameterPropertyDrawer : PropertyDrawer
	{
		private const float k_XBuffer = 15;
		private const float k_DefaultHeight = 16;
		private bool shouldDisplay;
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return shouldDisplay ? 64 : k_DefaultHeight;
		}

		/// <summary>
		/// Uses the <see cref="Animator"/> found through the <paramref name="property"/> to retrieve a list on
		/// <see cref="AnimatorControllerParameter"/> to display as a drop down for selecting the parameter name.
		/// </summary>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.type != "AnimationFloatParameter")
			{
				Debug.LogErrorFormat("AnimatorFloatParameter attribute only appropriate for AnimationFloatParameter " +
				                     "fields. It is being used on {0} which is a {1}.", property.displayName, 
				                     property.type);
				base.OnGUI(position, property, label);
				return;
			}
			var animatorStateParameterAttribute = (AnimatorFloatParameterAttribute) attribute;
			SerializedProperty animatorProperty = EditorHelpers.FindPropertyRelative(property, 
												  animatorStateParameterAttribute.animatorField);
			if (animatorProperty == null || animatorProperty.objectReferenceValue == null)
			{
				base.OnGUI(position, property, label);
				return;
			}
			var animator = animatorProperty.objectReferenceValue as AnimatorController;
			if (animator == null)
			{
				base.OnGUI(position, property, label);
				return;
			}

			shouldDisplay = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, k_DefaultHeight), 
			                                  shouldDisplay, property.displayName);
			if (!shouldDisplay)
			{
				return;
			}

			AnimatorControllerParameter[] parameters = animator.parameters;
			string[] parametersAsStrings = ParametersAsArray(parameters);
			
			SerializedProperty nameProperty = property.FindPropertyRelative ("parameterName");
			string s = nameProperty.stringValue;
			int index = EditorGUI.Popup(new Rect(position.x + k_XBuffer, position.y + 16, position.width, k_DefaultHeight), 
			                           nameProperty.displayName, GetIndex(s, parametersAsStrings), parametersAsStrings);
			nameProperty.stringValue = parametersAsStrings[index];
			
			// simply draw the min and max interpolation times.
			SerializedProperty minProperty = property.FindPropertyRelative ("minInterpolationTime");
			SerializedProperty maxProperty = property.FindPropertyRelative ("maxInterpolationTime");
			EditorGUI.PropertyField(new Rect(position.x + k_XBuffer, position.y + 32, position.width, k_DefaultHeight), 
			                        minProperty);
			EditorGUI.PropertyField(new Rect(position.x + k_XBuffer, position.y + 48, position.width, k_DefaultHeight), 
			                        maxProperty);
		}

		private string[] ParametersAsArray(AnimatorControllerParameter[] parameters)
		{
			return (parameters.Where(parameter => parameter.type == AnimatorControllerParameterType.Float)
			                  .Select(parameter => parameter.name)).ToArray();
		}
		
		private int GetIndex(string selected, IList<string> parameters)
		{
			int index = parameters.IndexOf(selected);
			return index < 0 ? 0 : index;
		}
	}
}