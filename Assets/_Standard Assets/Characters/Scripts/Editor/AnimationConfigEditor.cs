using System.Collections.Generic;
using System.Linq;
using StandardAssets.Characters.ThirdPerson.Configs;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	/// <summary>
	/// Custom Editor for <see cref="AnimationConfig"/>
	/// </summary>
	[CustomEditor(typeof(AnimationConfig))]
	public class AnimationConfigEditor : UnityEditor.Editor
	{
		//Property names
		const string k_HeadTurn 			= "m_HeadTurnProperties";
		const string k_StrafeChangeAngle 	= "m_StrafeRapidDirectionChangeAngle";
		const string k_StrafeChangeCurve 	= "m_StrafeRapidDirectionChangeSpeedCurve";
		
		/// <summary>
		/// Draws the inspector GUI using exclusions
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawPropertiesExcluding(serializedObject, GetExclusions());
			
			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
		
		
		/// <summary>
		/// Gets the properties that should not be rendered
		/// </summary>
		string[] GetExclusions()
		{
			var exclusions = new List<string>();
			var config =  (AnimationConfig)target;

			if (!config.enableHeadLookAt)
			{
				exclusions.Add(k_HeadTurn);
			}
			if (!config.enableStrafeRapidDirectionChangeSmoothing)
			{
				exclusions.Add(k_StrafeChangeAngle);
				exclusions.Add(k_StrafeChangeCurve);
			}

			return exclusions.ToArray();
		}
	}
}