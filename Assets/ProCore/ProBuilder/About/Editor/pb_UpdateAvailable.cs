using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	class pb_UpdateAvailable : EditorWindow
	{
		public static void Init(pb_VersionInfo newVersion, string changelog)
		{
			pb_UpdateAvailable win = EditorWindow.GetWindow<pb_UpdateAvailable>(true, "ProBuilder Update Available", true);
			win.m_NewVersion = newVersion;
			win.m_NewChangelog = changelog;
		}

		[SerializeField] pb_VersionInfo m_NewVersion;
		[SerializeField] string m_NewChangelog;
		Vector2 scroll = Vector2.zero;
		GUIContent gc_DownloadUpdate = new GUIContent("", "Open Asset Store Download Page");

		private static GUIStyle downloadImageStyle = null,
								updateHeader = null;

		private bool checkForProBuilderUpdates
		{
			get { return pb_PreferencesInternal.GetBool(pb_Constant.pbCheckForProBuilderUpdates, true); }
			set { pb_PreferencesInternal.SetBool(pb_Constant.pbCheckForProBuilderUpdates, value); }
		}

		void OnEnable()
		{
			pb_AboutWindow.InitGuiStyles();

			wantsMouseMove = true;
			minSize = new Vector2(400f, 350f);

			downloadImageStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				fixedWidth = 154,
				fixedHeight = 85,
				normal = new GUIStyleState() {
					background = pb_AboutWindow.LoadAssetAtPath<Texture2D>(string.Format("{0}/Images/DownloadPB_Normal.png", pb_AboutWindow.AboutRoot))
				},
				hover = new GUIStyleState() {
					background = pb_AboutWindow.LoadAssetAtPath<Texture2D>(string.Format("{0}/Images/DownloadPB_Hover.png", pb_AboutWindow.AboutRoot))
				},
			};

			updateHeader = new GUIStyle()
			{
				margin = new RectOffset(0, 0, 0, 0),
				alignment = TextAnchor.MiddleCenter,
				fixedHeight = 85,
				fontSize = 24,
				wordWrap = true,
				font = pb_AboutWindow.LoadAssetAtPath<Font>(string.Format("{0}/Font/{1}", pb_AboutWindow.AboutRoot, pb_AboutWindow.FONT_MEDIUM)),
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? pb_AboutWindow.font_white : pb_AboutWindow.font_black }
			};
		}

		void OnGUI()
		{
			GUILayout.BeginHorizontal();

				if( GUILayout.Button(gc_DownloadUpdate, downloadImageStyle) )
					Application.OpenURL("http://u3d.as/30b");

				if(GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
					Repaint();

				GUILayout.BeginVertical(pb_AboutWindow.changelogStyle);
				GUILayout.Label("ProBuilder Update\nAvailable", updateHeader);
				GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			scroll = EditorGUILayout.BeginScrollView(scroll, pb_AboutWindow.changelogStyle);
			GUILayout.Label(string.Format("Version: {0}", m_NewVersion.text), pb_AboutWindow.versionInfoStyle);
			GUILayout.Label("\n" + m_NewChangelog, pb_AboutWindow.changelogTextStyle);
			EditorGUILayout.EndScrollView();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			checkForProBuilderUpdates = EditorGUILayout.Toggle("Show Update Notifications", checkForProBuilderUpdates);
			GUILayout.Space(4);
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
		}
	}
}
