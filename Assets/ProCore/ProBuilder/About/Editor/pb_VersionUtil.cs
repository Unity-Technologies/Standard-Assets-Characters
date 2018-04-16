using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Contains information that the pb_AboutEntry.txt file holds.
	 */
	[System.Serializable]
	internal class pb_AboutEntry
	{
		public string name;
		public string identifier;
		public string version;
		public string date;
		public string changelogPath;

		public const string KEY_NAME = "name: ";
		public const string KEY_IDENTIFIER = "identifier: ";
		public const string KEY_VERSION = "version: ";
		public const string KEY_DATE = "date: ";
		public const string KEY_CHANGELOG = "changelog: ";
	}

	/**
	 *	Utility methods for finding and extracting version & changelog information.
	 */
	internal static class pb_VersionUtil
	{
		/**
		 *	Get information from the currently installed ProBuilder version.
		 */
		public static bool GetAboutEntry(out pb_AboutEntry about)
		{
			about = null;

			string[] matches = Directory.GetFiles("./Assets", "pc_AboutEntry_ProBuilder.txt", SearchOption.AllDirectories);

			if(matches == null || matches.Length < 1)
				return false;

			for(int i = 0; i < matches.Length && about == null; i++)
				about = ParseAboutEntry(matches[i]);

			return about != null;
		}

		public static bool GetCurrent(out pb_VersionInfo version)
		{
			pb_AboutEntry about;

			if(!GetAboutEntry(out about))
			{
				version = new pb_VersionInfo();
				return false;
			}

			version = pb_VersionInfo.FromString(about.version);
			return true;
		}

		/**
		 *	Extracts and formats the latest changelog entry into rich text.  Also grabs the version.
		 */
		public static bool FormatChangelog(string raw, out pb_VersionInfo version, out string formatted_changes)
		{
			bool success = true;

			// get first version entry
			string[] split = Regex.Split(raw, "(?mi)^#\\s", RegexOptions.Multiline);

			// get the version info
			try
			{
				Match versionMatch = Regex.Match(split[1], @"(?<=^ProBuilder\s).[0-9]*\.[0-9]*\.[0-9]*[a-z][0-9]*");
				version = pb_VersionInfo.FromString(versionMatch.Success ? versionMatch.Value : split[1].Split('\n')[0]);
			}
			catch
			{
				version = pb_VersionInfo.FromString("not found");
				success = false;
			}

			try
			{
				StringBuilder sb = new StringBuilder();
				string[] newLineSplit = split[1].Trim().Split('\n');
				for(int i = 2; i < newLineSplit.Length; i++)
					sb.AppendLine(newLineSplit[i]);

				formatted_changes = sb.ToString();
				formatted_changes = Regex.Replace(formatted_changes, "^-", "\u2022", RegexOptions.Multiline);
				formatted_changes = Regex.Replace(formatted_changes, @"(?<=^##\\s).*", "<size=16><b>${0}</b></size>", RegexOptions.Multiline);
				formatted_changes = Regex.Replace(formatted_changes, @"^##\ ", "", RegexOptions.Multiline);
			}
			catch
			{
				formatted_changes = "";
				success = false;
			}

			return success;
		}

		private static pb_AboutEntry ParseAboutEntry(string path)
		{
			if (!File.Exists(path))
				return null;

			pb_AboutEntry about = new pb_AboutEntry();

			foreach(string str in File.ReadAllLines(path))
			{
				if(str.StartsWith(pb_AboutEntry.KEY_NAME))
					about.name = str.Replace(pb_AboutEntry.KEY_NAME, "").Trim();
				else if(str.StartsWith(pb_AboutEntry.KEY_IDENTIFIER))
					about.identifier = str.Replace(pb_AboutEntry.KEY_IDENTIFIER, "").Trim();
				else if(str.StartsWith(pb_AboutEntry.KEY_VERSION))
					about.version = str.Replace(pb_AboutEntry.KEY_VERSION, "").Trim();
				else if(str.StartsWith(pb_AboutEntry.KEY_DATE))
					about.date = str.Replace(pb_AboutEntry.KEY_DATE, "").Trim();
				else if(str.StartsWith(pb_AboutEntry.KEY_CHANGELOG))
					about.changelogPath = str.Replace(pb_AboutEntry.KEY_CHANGELOG, "").Trim();
			}

			return about;
		}
	}
}
