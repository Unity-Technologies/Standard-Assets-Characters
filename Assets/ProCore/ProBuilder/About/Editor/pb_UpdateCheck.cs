using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Check for updates to ProBuilder.
	 */
	[InitializeOnLoad]
	static class pb_UpdateCheck
	{
		const string PROBUILDER_VERSION_URL = "http://procore3d.github.io/probuilder2/current.txt";
		const string pbLastWebVersionChecked = "pbLastWebVersionChecked";
		static WWW updateQuery;
		static bool calledFromMenu = false;

		static pb_UpdateCheck()
		{
			if(pb_PreferencesInternal.GetBool(pb_Constant.pbCheckForProBuilderUpdates))
			{
				calledFromMenu = false;
				CheckForUpdate();
			}
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Check for Updates", false, pb_Constant.MENU_ABOUT + 1)]
		static void MenuCheckForUpdate()
		{
			calledFromMenu = true;
			CheckForUpdate();
		}

		public static void CheckForUpdate()
		{
			if(updateQuery == null)
			{
				updateQuery = new WWW(PROBUILDER_VERSION_URL);
				EditorApplication.update += Update;
			}
		}

		static void Update()
		{
			if (updateQuery != null)
			{
				if (!updateQuery.isDone)
					return;

				try
				{
					if (string.IsNullOrEmpty(updateQuery.error) || !Regex.IsMatch(updateQuery.text, "404 not found", RegexOptions.IgnoreCase) )
					{
						pb_VersionInfo webVersion;
						string webChangelog;

						if(!pb_VersionUtil.FormatChangelog(updateQuery.text, out webVersion, out webChangelog))
						{
							FailedConnection();
						}
						else
						{
							pb_VersionInfo current;

							// first test if the installed version is already up to date
							if( !pb_VersionUtil.GetCurrent(out current) || webVersion.CompareTo(current) > 0 )
							{
								// next, test if a notification for this version has already been shown
								string lastNotification = pb_PreferencesInternal.GetString(pbLastWebVersionChecked, "");

								if(calledFromMenu || !lastNotification.Equals(webVersion.text))
								{
									pb_UpdateAvailable.Init(webVersion, webChangelog);
									pb_PreferencesInternal.SetString(pbLastWebVersionChecked, webVersion.text);
								}
							}
							else
							{
								UpToDate(current.ToString());
							}
						}
					}
					else
					{
						FailedConnection();
					}
				}
				catch(System.Exception e)
				{
					FailedConnection(string.Format("Error: Is build target is Webplayer?\n\n{0}", e.ToString()));
				}

				updateQuery = null;
			}

			calledFromMenu = false;
			EditorApplication.update -= Update;
		}

		static void UpToDate(string version)
		{
			if(calledFromMenu)
				EditorUtility.DisplayDialog("ProBuilder Update Check", string.Format("You're up to date!\n\nInstalled Version: {0}\nLatest Version: {0}", version), "Okay");
		}

		static void FailedConnection(string error = null)
		{
			if(calledFromMenu)
				EditorUtility.DisplayDialog(
					"ProBuilder Update Check",
					error == null ? "Failed to connect to server!" : string.Format("Failed to connect to server!\n\n{0}", error.ToString()),
					"Okay");
		}
	}
}
