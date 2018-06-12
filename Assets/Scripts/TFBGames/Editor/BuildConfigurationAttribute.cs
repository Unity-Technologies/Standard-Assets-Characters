using System;

namespace TFBGames.Editor
{
	//THIS WILL NOT BE INCLUDED IN THE FINAL BUILD - IT IS FOR THE BUILD SERVER
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
