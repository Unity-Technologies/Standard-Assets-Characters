using System;
using UnityEditor;
using UnityEngine;

namespace StandardAssets.TFBGames.Editor
{
	/// <summary>
	/// Editor Styles
	/// </summary>
	internal static class TfbEditorStyles
	{
		private static bool s_IsInitialized;

		private static GUIStyle s_DeleteArrayItemButtonStyle,
		                        s_PanelHeader,
		                        s_PanelContent,
		                        s_PanelItem;

	    /// <summary>
	    /// A small button for deleting array elements. Intended for horizontal layout without expanded widths.
	    /// </summary>
	    public static GUIStyle DeletArrayItemButton
		{
			get { return PassCheckInitialized(s_DeleteArrayItemButtonStyle); }
		}

		/// <summary>
		/// Array Item styles for "RL" prefixed editor styles.
		/// </summary>
		public static GUIStyle PanelArrayItem
		{
			get { return PassCheckInitialized(s_PanelItem); }
		}

		public static void DrawPanel(GUIContent title, Action content)
		{
			if (!s_IsInitialized)
			{
				PassCheckInitialized(null);
			}

			// container for entire panel
			EditorGUILayout.BeginVertical();
			{
				// braces for readability

				// Header:
				EditorGUILayout.BeginHorizontal(s_PanelHeader);
				EditorGUILayout.LabelField(title);
				// we can add things to header here if needed.
				EditorGUILayout.EndHorizontal();
				// Content:
				EditorGUILayout.BeginVertical(s_PanelContent);
				EditorGUILayout.BeginVertical();
				content();
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
		}

		private static GUIStyle PassCheckInitialized(GUIStyle style)
		{
			if (!s_IsInitialized)
			{
				s_IsInitialized = true;

				// a new copy is needed if the style is modified, otherwise you can break unity's internal styles.
				s_DeleteArrayItemButtonStyle = new GUIStyle("button");
				s_DeleteArrayItemButtonStyle.fixedWidth = 24;
				s_DeleteArrayItemButtonStyle.alignment = TextAnchor.MiddleCenter;
				s_DeleteArrayItemButtonStyle.stretchWidth = false;

				// RL prefixed style elements are the kind used by UnityEvent inspectors and some list inspectors.
				// we are using it to construct panels in the same style.
				s_PanelHeader = "RL Header";
				s_PanelContent = new GUIStyle("RL Background");
				s_PanelContent.fixedHeight = 0;
				s_PanelContent.stretchHeight = false;
				s_PanelItem = "RL Element";
			}

			return style;
		}
	}
}