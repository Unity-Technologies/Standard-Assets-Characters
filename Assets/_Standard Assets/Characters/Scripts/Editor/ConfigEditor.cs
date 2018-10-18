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
			
			MotorConfig config = target as MotorConfig;

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
		private const string k_HeadTurn = "headTurnProperties";
		
		protected override string[] GetExclusions()
		{
			AnimationConfig config =  target as AnimationConfig;

			if (config.enableHeadLookAt)
			{
				return new string[0];
			}
			
			return new string[]
			{
				k_HeadTurn
			};	
		}
	}
}