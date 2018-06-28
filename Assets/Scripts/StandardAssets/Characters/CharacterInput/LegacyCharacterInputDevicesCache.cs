namespace StandardAssets.Characters.CharacterInput
{
	public static class LegacyCharacterInputDevicesCache
	{
		private static string s_ConventionCache;

		public static string GetConvention(string controlConvention)
		{
			if (string.IsNullOrEmpty(s_ConventionCache))
			{
				s_ConventionCache = controlConvention.Replace("{platform}", "{0}").Replace("{controller}", "{1}")
				                                       .Replace("{control}", "{2}");	
			}

			return s_ConventionCache;
		}
	}
}