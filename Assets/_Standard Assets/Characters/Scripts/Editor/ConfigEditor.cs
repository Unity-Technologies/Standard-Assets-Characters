using System.Collections.Generic;
using System.Linq;
using StandardAssets.Characters.ThirdPerson.Configs;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	public abstract class ConfigEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			DrawPropertiesExcluding(serializedObject, GetExclusions());
			
			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}

		protected abstract string[] GetExclusions();
	}
	
	[CustomEditor(typeof(MotorConfig))]
	public class MotorConfigEditor : ConfigEditor
	{
		private const string k_ActionName = "action", k_StrafeName = "strafing";
		
		protected override string[] GetExclusions()
		{
			List<string> exclusions = new List<string>(); 
			
			var config = (MotorConfig)target;

			if (!config.customActionParametersToBeUsed)
			{
				exclusions.Add(k_ActionName);
			}
			
			if (!config.customStrafeParametersToBeUsed)
			{
				exclusions.Add(k_StrafeName);
			}
			
			return exclusions.ToArray();	
		}
	}
	
	[CustomEditor(typeof(AnimationConfig))]
	public class AnimationConfigEditor : ConfigEditor
	{
		private const string k_HeadTurn = "headTurnProperties",
		                     k_StrafeChangeAngle = "strafeRapidDirectionChangeAngle",
		                     k_StrafeChangeCurve = "strafeRapidDirectionChangeSpeedCurve";
		
		protected override string[] GetExclusions()
		{
			List<string> exclusions = new List<string>();
			var config =  (AnimationConfig)target;

			if (!config.enableHeadLookAt)
			{
				exclusions.Add(k_HeadTurn);
			}
			if (!config.enableStrafeRapidDirectionChangeSmoothingLogic)
			{
				exclusions.Add(k_StrafeChangeAngle);
				exclusions.Add(k_StrafeChangeCurve);
			}

			return exclusions.ToArray();
		}
	}
}