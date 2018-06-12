using System;

namespace TFBGames.Editor
{
	[AttributeUsage(AttributeTargets.Method)]
	public class BuildConfigurationAttribute : Attribute
	{
		/// <summary>
		/// The flag that is passed into the command line in order to toggle this setting
		/// </summary>
		public string BuildStepFlag
		{
			get;
			private set;
		}

		public BuildConfigurationAttribute(string buildStepFlag)
		{
			this.BuildStepFlag = buildStepFlag;
		}
	}
}
