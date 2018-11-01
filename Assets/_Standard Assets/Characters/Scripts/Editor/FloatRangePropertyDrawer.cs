using UnityEngine;
using UnityEditor;
using StandardAssets.Characters.Helpers;

namespace Editor
{
	/// <summary>
	/// Custom Drawer for the <see cref="StandardAssets.Characters.Helpers.FloatRange"/>
	/// </summary>
	[CustomPropertyDrawer(typeof(FloatRange), true)]
	public class FloatRangePropertyDrawer : PropertyDrawer
	{
		/// <summary>
		/// Handles the drawing of the <see cref="Util.FloatRange"/>
		/// </summary>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label = EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, label);

			SerializedProperty minProp = property.FindPropertyRelative("minValue");
			SerializedProperty maxProp = property.FindPropertyRelative("maxValue");

			float minValue = minProp.floatValue;
			float maxValue = maxProp.floatValue;

			float rangeMin = 0;
			float rangeMax = 1;
			int decimalPoints = 2;

			var ranges = (FloatRangeSetupAttribute[]) fieldInfo.GetCustomAttributes(typeof(FloatRangeSetupAttribute), true);
			if (ranges.Length > 0)
			{
				rangeMin = ranges[0].min;
				rangeMax = ranges[0].max;
				decimalPoints = ranges[0].decimalPoints;
			}

			const float rangeBoundsLabelWidth = 40f;
			var rangeBoundsLabel1Rect = new Rect(position);
			rangeBoundsLabel1Rect.width = rangeBoundsLabelWidth;
			position.xMin += rangeBoundsLabelWidth;

			var rangeBoundsLabel2Rect = new Rect(position);
			rangeBoundsLabel2Rect.xMin = rangeBoundsLabel2Rect.xMax - rangeBoundsLabelWidth;
			position.xMax -= rangeBoundsLabelWidth;

			EditorGUI.BeginChangeCheck();
			minValue = Mathf.Clamp(EditorGUI.FloatField(rangeBoundsLabel1Rect, minValue), rangeMin, maxValue);
			maxValue = Mathf.Clamp(EditorGUI.FloatField(rangeBoundsLabel2Rect, maxValue), minValue, rangeMax);

			EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, rangeMin, rangeMax);
			if (EditorGUI.EndChangeCheck())
			{
				minProp.floatValue = (float) System.Math.Round(minValue, decimalPoints);
				maxProp.floatValue = (float) System.Math.Round(maxValue, decimalPoints);
			}

			EditorGUI.EndProperty();
		}
	}
}