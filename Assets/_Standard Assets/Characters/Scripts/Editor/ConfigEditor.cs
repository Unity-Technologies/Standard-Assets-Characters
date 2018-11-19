using System.Collections.Generic;
using System.Linq;
using StandardAssets.Characters.ThirdPerson.Configs;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	/// <summary>
	/// COMMENT TODO
	/// </summary>
	public abstract class ConfigEditor : UnityEditor.Editor
	{
		/// <summary>
		/// COMMENT TODO
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
		/// COMMENT TODO
		/// </summary>
		protected abstract string[] GetExclusions();
	}
	

	/// <summary>
	/// COMMENT TODO
	/// </summary>
	[CustomEditor(typeof(MotorConfig))]
	public class MotorConfigEditor : ConfigEditor
	{
		/// <summary>
		/// COMMENT TODO
		/// </summary>
		protected override string[] GetExclusions()
		{
			return new string[0];
		}
	}

	
	/// <summary>
	/// COMMENT TODO
	/// </summary>
	[CustomEditor(typeof(AnimationConfig))]
	public class AnimationConfigEditor : ConfigEditor
	{
		// COMMENT TODO
		const string k_HeadTurn 			= "m_HeadTurnProperties";
		const string k_StrafeChangeAngle 	= "m_StrafeRapidDirectionChangeAngle";
		const string k_StrafeChangeCurve 	= "m_StrafeRapidDirectionChangeSpeedCurve";
		
		
		/// <summary>
		/// COMMENT TODO
		/// </summary>
		protected override string[] GetExclusions()
		{
			List<string> exclusions = new List<string>();
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