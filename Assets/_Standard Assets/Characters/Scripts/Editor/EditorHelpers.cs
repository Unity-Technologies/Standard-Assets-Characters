#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace StandardAssets.Editor
{
	/// <summary>
	/// Some layout helpers and style boilerplate for custom inspectors.
	/// </summary>
	internal static class EditorHelpers
	{
		private static Texture2D s_IconPlus, s_IconMinus;

		private static readonly GUIContent
			// we will reuse this one over
			s_SharedContent = new GUIContent();

		private static GUIStyle
			s_StylePanelHeader,
			s_StylePanelContent,
			s_StylePanelElement,
			s_StylePanelFooter,
			s_StylePanelFooterButton,
			s_StyleButtonPlus,
			s_StyleButtonMinus;

		public static Texture2D IconPlus
		{
			get { return Lazy(ref s_IconPlus, "d_Toolbar Plus"); }
		}

		public static Texture2D IconMinus
		{
			get { return Lazy(ref s_IconMinus, "d_Toolbar Minus"); }
		}

		public static GUIStyle PanelHeader
		{
			get { return Lazy(ref s_StylePanelHeader, "RL Header"); }
		}

		public static GUIStyle PanelContent
		{
			get { return Lazy(ref s_StylePanelContent, "RL Background"); }
		}

		public static GUIStyle PanelFooter
		{
			get { return Lazy(ref s_StylePanelFooter, "RL Footer"); }
		}

		public static GUIStyle PanelElement
		{
			get { return Lazy(ref s_StylePanelElement, "RL Element"); }
		}

		public static GUIStyle PanelFooterButton
		{
			get { return Lazy(ref s_StylePanelFooterButton, "RL FooterButton", SetupPlusMinusButton, true); }
		}

		/// <summary>
		/// Helper for manual draw unity style arra/list editor.
		/// </summary>
		public static void DrawArrayPropertyPanel(SerializedProperty array, Action<SerializedProperty> drawItem,
		                                          int minItems = 0, int maxItems = 0)
		{
			s_SharedContent.text = array.displayName;
			s_SharedContent.tooltip = array.tooltip;

			Action contentFunc = () =>
			{
				 for (int i = 0; i < array.arraySize; i++)
				 {
					 // per item:
					 EditorGUILayout.BeginVertical(PanelElement);
					 drawItem(array.GetArrayElementAtIndex(i));
					 EditorGUILayout.EndVertical();
				 }
			};

			Action footerFunc = () =>
			{
				bool canDelete = array.arraySize > minItems;
				bool canAdd = (maxItems < 1) || (array.arraySize < maxItems);

				EditorGUI.BeginDisabledGroup(!canAdd);
				if (GUILayout.Button(new GUIContent(IconPlus), PanelFooterButton,
									 GUILayout.ExpandWidth(true)))
				{
					array.arraySize++;
					array.serializedObject.ApplyModifiedProperties();
				}

				EditorGUI.EndDisabledGroup();

				EditorGUI.BeginDisabledGroup(!canDelete);
				if (GUILayout.Button(new GUIContent(IconMinus), PanelFooterButton,
									 GUILayout.ExpandWidth(true)))
				{
					array.arraySize--;
					array.serializedObject.ApplyModifiedProperties();
				}

				EditorGUI.EndDisabledGroup();
			};

			DrawPanel(s_SharedContent, contentFunc, footerFunc);
		}

		public static void DrawPanel(string title, Action content, Action footerButtons = null)
		{
			s_SharedContent.text = title;
			s_SharedContent.tooltip = string.Empty;
			DrawPanel(title, content, footerButtons);
		}

		public static void DrawPanel(GUIContent title, Action content, Action footerButtons = null)
		{
			EditorGUILayout.BeginVertical();
			{
				// Header:
				EditorGUILayout.BeginVertical(PanelHeader);
				EditorGUILayout.LabelField(title);
				EditorGUILayout.EndVertical();
				// Content:
				EditorGUILayout.BeginVertical(PanelContent);
				content();
				EditorGUILayout.Separator();
				EditorGUILayout.EndVertical();

				if (footerButtons != null)
				{
					// Footer container:
					EditorGUILayout.BeginHorizontal();
					{
						// spacer:
						GUILayout.FlexibleSpace();

						EditorGUILayout.BeginHorizontal(PanelFooter, GUILayout.MaxWidth(60));
						GUILayout.BeginHorizontal(GUILayout.MaxHeight(16));
						footerButtons();
						GUILayout.EndHorizontal();
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
		}

		private static void SetupPlusMinusButton(GUIStyle style)
		{
			style.fixedHeight = 0;
			style.imagePosition = ImagePosition.ImageOnly;
			style.margin.top = 0;
			style.padding = new RectOffset(2, 2, 8, 0);
			style.contentOffset = new Vector2(0, -10);
		}

		private static Texture2D Lazy(ref Texture2D field, string src)
		{
			if (field == null)
			{
				field = EditorGUIUtility.FindTexture(src);
			}

			return field;
		}

		private static GUIStyle Lazy(ref GUIStyle field, GUIStyle src, Action<GUIStyle> initAction = null,
		                             bool isCopy = false)
		{
			if (field == null)
			{
				field = isCopy ? new GUIStyle(src) : src;
				// for some reason still gettig null:
				if (field == null)
				{
					field = new GUIStyle();
				}

				if (initAction != null)
				{
					initAction(field);
				}
			}

			return field;
		}
	}
}

#endif