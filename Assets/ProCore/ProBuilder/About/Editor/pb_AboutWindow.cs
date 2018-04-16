using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 * INSTRUCTIONS
	 *
	 *  - Only modify properties in the USER SETTINGS region.
	 *	- All content is loaded from external files (pc_AboutEntry_YourProduct.  Use the templates!
	 */

	/**
	 * Used to pop up the window on import.
	 */
	[InitializeOnLoad]
	static class pb_AboutWindowSetup
	{
		static pb_AboutWindowSetup()
		{
			EditorApplication.delayCall += () => { pb_AboutWindow.Init(false); };
		}
	}

	/**
	 * Changelog.txt file should follow this format:
	 *
	 *	| # Product Name 2.1.0
	 *	|
	 *	| ## Features
	 *	|
	 *	| - All kinds of awesome stuff
	 *	| - New flux capacitor design achieves time travel at lower velocities.
	 *	| - Dark matter reactor recalibrated.
	 *	|
	 *	| ## Bug Fixes
	 *	|
	 *	| - No longer explodes when spacebar is pressed.
	 *	| - Fix rolling issue in rickmeter.
	 *	|
	 *	| # Changes
	 *	|
	 *	| - Changed Blue to Red.
	 *	| - Enter key now causes explosions.
	 *
	 * This path is relative to the PRODUCT_ROOT path.
	 *
	 * Note that your changelog may contain multiple entries.  Only the top-most
	 * entry will be displayed.
	 */
	public class pb_AboutWindow : EditorWindow
	{
		// Modify these constants to customize about screen.
	 	const string PACKAGE_NAME = "ProBuilder";

 		private static string aboutRoot = "Assets/ProCore/" + PACKAGE_NAME + "/About";

		// Path to the root folder
		internal static string AboutRoot
		{
			get
			{
				if( Directory.Exists(aboutRoot) )
				{
					return aboutRoot;
				}
				else
				{
					aboutRoot = pb_FileUtil.FindFolder("ProBuilder/About");
					if(aboutRoot.EndsWith("/"))
						aboutRoot = aboutRoot.Remove(aboutRoot.LastIndexOf("/"), 1);
					return aboutRoot;
				}
			}
		}

		GUIContent gc_Learn = new GUIContent("Learn ProBuilder", "Documentation");
		GUIContent gc_Forum = new GUIContent("Support Forum", "ProCore Support Forum");
		GUIContent gc_Contact = new GUIContent("Contact Us", "Send us an email!");
		GUIContent gc_Banner = new GUIContent("", "ProBuilder Quick-Start Video Tutorials");

		private const string VIDEO_URL = @"http://bit.ly/pbstarter";
		private const string LEARN_URL = @"http://procore3d.com/docs/probuilder";
		private const string SUPPORT_URL = @"http://www.procore3d.com/forum/";
		private const string CONTACT_EMAIL = @"http://www.procore3d.com/about/";
		private const float BANNER_WIDTH = 480f;
		private const float BANNER_HEIGHT = 270f;

		internal const string FONT_REGULAR = "Asap-Regular.otf";
		internal const string FONT_MEDIUM = "Asap-Medium.otf";

		// Use less contast-y white and black font colors for better readabililty
		public static readonly Color font_white = HexToColor(0xCECECE);
		public static readonly Color font_black = HexToColor(0x545454);
		public static readonly Color font_blue_normal = HexToColor(0x00AAEF);
		public static readonly Color font_blue_hover = HexToColor(0x008BEF);


		private string productName = pb_Constant.PRODUCT_NAME;
		private pb_AboutEntry about = null;
		private string changelogRichText = "";

		internal static GUIStyle bannerStyle,
								header1Style,
								versionInfoStyle,
								linkStyle,
								separatorStyle,
								changelogStyle,
								changelogTextStyle;

		Vector2 scroll = Vector2.zero;

		/**
		 * Return true if Init took place, false if not.
		 */
		public static bool Init (bool fromMenu)
		{
			pb_AboutEntry about;

			if(!pb_VersionUtil.GetAboutEntry(out about))
			{
				Debug.LogWarning("Couldn't find pb_AboutEntry_ProBuilder.txt");
				return false;
			}

			if(fromMenu || pb_PreferencesInternal.GetString(about.identifier) != about.version)
			{
				pb_AboutWindow win;
				win = (pb_AboutWindow)EditorWindow.GetWindow(typeof(pb_AboutWindow), true, about.name, true);
				win.ShowUtility();
				win.SetAbout(about);
				pb_PreferencesInternal.SetString(about.identifier, about.version, pb_PreferenceLocation.Global);
				return true;
			}
			else
			{
				return false;
			}
		}

		private static Color HexToColor(uint x)
		{
			return new Color( 	((x >> 16) & 0xFF) / 255f,
								((x >> 8) & 0xFF) / 255f,
								(x & 0xFF) / 255f,
								1f);
		}

		public static void InitGuiStyles()
		{
			bannerStyle = new GUIStyle()
			{
				// RectOffset(left, right, top, bottom)
				margin = new RectOffset(12, 12, 12, 12),
				normal = new GUIStyleState() {
					background = LoadAssetAtPath<Texture2D>(string.Format("{0}/Images/Banner_Normal.png", AboutRoot))
				},
				hover = new GUIStyleState() {
					background = LoadAssetAtPath<Texture2D>(string.Format("{0}/Images/Banner_Hover.png", AboutRoot))
				},
			};

			header1Style = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				fontSize = 24,
				// fontStyle = FontStyle.Bold,
				font = LoadAssetAtPath<Font>(string.Format("{0}/Font/{1}", AboutRoot, FONT_MEDIUM)),
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? font_white : font_black }
			};

			versionInfoStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				fontSize = 14,
				font = LoadAssetAtPath<Font>(string.Format("{0}/Font/{1}", AboutRoot, FONT_REGULAR)),
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? font_white : font_black }
			};

			linkStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				fontSize = 16,
				font = LoadAssetAtPath<Font>(string.Format("{0}/Font/{1}", AboutRoot, FONT_REGULAR)),
				normal = new GUIStyleState() {
					textColor = font_blue_normal,
					background = LoadAssetAtPath<Texture2D>(
						string.Format("{0}/Images/ScrollBackground_{1}.png",
							AboutRoot,
							EditorGUIUtility.isProSkin ? "Pro" : "Light"))
				},
				hover = new GUIStyleState() {
					textColor = font_blue_hover,
					background = LoadAssetAtPath<Texture2D>(
						string.Format("{0}/Images/ScrollBackground_{1}.png",
							AboutRoot,
							EditorGUIUtility.isProSkin ? "Pro" : "Light"))
				}
			};

			separatorStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				fontSize = 16,
				font = LoadAssetAtPath<Font>(string.Format("{0}/Font/{1}", AboutRoot, FONT_REGULAR)),
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? font_white : font_black }
			};

			changelogStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				font = LoadAssetAtPath<Font>(string.Format("{0}/Font/{1}", AboutRoot, FONT_REGULAR)),
				richText = true,
				normal = new GUIStyleState() { background = LoadAssetAtPath<Texture2D>(
					string.Format("{0}/Images/ScrollBackground_{1}.png",
						AboutRoot,
						EditorGUIUtility.isProSkin ? "Pro" : "Light"))
				}
			};

			changelogTextStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				font = LoadAssetAtPath<Font>(string.Format("{0}/Font/{1}", AboutRoot, FONT_REGULAR)),
				fontSize = 14,
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? font_white : font_black },
				richText = true,
				wordWrap = true
			};
		}

		public void OnEnable()
		{
			InitGuiStyles();

			Texture2D banner = bannerStyle.normal.background;

			if(banner == null)
			{
				Debug.LogWarning("Could not load About window resources");
				this.Close();
			}
			else
			{
				bannerStyle.fixedWidth = BANNER_WIDTH; // banner.width;
				bannerStyle.fixedHeight = BANNER_HEIGHT; // banner.height;

				this.wantsMouseMove = true;

				this.minSize = new Vector2(BANNER_WIDTH + 24, BANNER_HEIGHT * 2.5f);
				this.maxSize = new Vector2(BANNER_WIDTH + 24, BANNER_HEIGHT * 2.5f);

				if(!productName.Contains("Basic"))
					productName = "ProBuilder Advanced";
			}
		}

		void SetAbout(pb_AboutEntry about)
		{
			this.about = about;

			if(!File.Exists(about.changelogPath))
				about.changelogPath = pb_FileUtil.FindFile("ProBuilder/About/changelog.txt");

			if(File.Exists(about.changelogPath))
			{
				string raw = File.ReadAllText(about.changelogPath);

				if(!string.IsNullOrEmpty(raw))
				{
					pb_VersionInfo vi;
					pb_VersionUtil.FormatChangelog(raw, out vi, out changelogRichText);
				}
			}
		}

		internal static T LoadAssetAtPath<T>(string InPath) where T : UnityEngine.Object
		{
			return (T) AssetDatabase.LoadAssetAtPath(InPath, typeof(T));
		}

		void OnGUI()
		{
			Vector2 mousePosition = Event.current.mousePosition;

			if( GUILayout.Button(gc_Banner, bannerStyle) )
				Application.OpenURL(VIDEO_URL);

			if(GUILayoutUtility.GetLastRect().Contains(mousePosition))
				Repaint();

			GUILayout.BeginVertical(changelogStyle);

			GUILayout.Label(productName, header1Style);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

				if(GUILayout.Button(gc_Learn, linkStyle))
					Application.OpenURL(LEARN_URL);

				GUILayout.Label("|", separatorStyle);

				if(GUILayout.Button(gc_Forum, linkStyle))
					Application.OpenURL(SUPPORT_URL);

				GUILayout.Label("|", separatorStyle);

				if(GUILayout.Button(gc_Contact, linkStyle))
					Application.OpenURL(CONTACT_EMAIL);

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			if(GUILayoutUtility.GetLastRect().Contains(mousePosition))
				Repaint();

			GUILayout.EndVertical();

			// always bold the first line (cause it's the version info stuff)
			scroll = EditorGUILayout.BeginScrollView(scroll, changelogStyle);
			GUILayout.Label(string.Format("Version: {0}", about.version), versionInfoStyle);
			GUILayout.Label("\n" + changelogRichText, changelogTextStyle);
			EditorGUILayout.EndScrollView();
		}

		/**
		 * Draw a horizontal line across the screen and update the guilayout.
		 */
		void HorizontalLine()
		{
			Rect r = GUILayoutUtility.GetLastRect();
			Color og = GUI.backgroundColor;
			GUI.backgroundColor = Color.black;
			GUI.Box(new Rect(0f, r.y + r.height + 2, Screen.width, 2f), "");
			GUI.backgroundColor = og;

			GUILayout.Space(6);
		}
	}
}
