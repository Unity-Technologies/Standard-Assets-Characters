#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	/// <summary>
	/// Some layout helpers and style boilerplate for custom inspectors.
	/// </summary>
	static class EditorHelpers
	{
		static Texture2D s_IconPlus, s_IconMinus;

		static readonly GUIContent
			// we will reuse this one over
			s_SharedContent = new GUIContent();

		static GUIStyle
			s_StylePanelHeader,
			s_StylePanelContent,
			s_StylePanelElement,
			s_StylePanelFooter,
			s_StylePanelFooterButton,
			s_StyleButtonPlus,
			s_StyleButtonMinus;

		/// <summary>
		/// Unity's Plus Icon used by RL Prefixed UI elements.
		/// </summary>
		public static Texture2D IconPlus
		{
			get { return Lazy(ref s_IconPlus, "d_Toolbar Plus"); }
		}

		/// <summary>
		/// Unity's Minus Icon used by RL Prefixed UI elements.
		/// </summary>
		public static Texture2D IconMinus
		{
			get { return Lazy(ref s_IconMinus, "d_Toolbar Minus"); }
		}
		
		
		/// <summary>
		/// Unity's Header Style used by RL Prefixed UI elements. (EG: Event Drawer)
		/// </summary>
		public static GUIStyle PanelHeader
		{
			get { return Lazy(ref s_StylePanelHeader, "RL Header"); }
		}

		/// <summary>
		/// Unity's Background Style used by RL Prefixed UI elements. (EG: Event Drawer)
		/// </summary>
		public static GUIStyle PanelContent
		{
			get { return Lazy(ref s_StylePanelContent, "RL Background"); }
		}
		
		/// <summary>
		/// Unity's Footer Style used by RL Prefixed UI elements.
		/// (EG: Event Drawer, footer section where +/- buttons are normally placed)
		/// </summary>
		public static GUIStyle PanelFooter
		{
			get { return Lazy(ref s_StylePanelFooter, "RL Footer"); }
		}

		/// <summary>
		/// Unity's style for elements placed inside RL prefixed editor styles. (EG: Dragable List Items).
		/// This style has no graphic and is just a container and margin setup.
		/// </summary>
		public static GUIStyle PanelElement
		{
			get { return Lazy(ref s_StylePanelElement, "RL Element"); }
		}

		/// <summary>
		/// Unity's style for buttons placed in the footer of RL prefixed editor styles.
		/// </summary>
		public static GUIStyle PanelFooterButton
		{
			get { return Lazy(ref s_StylePanelFooterButton, "RL FooterButton", SetupPlusMinusButton, true); }
		}

		/// <summary>
		/// Helper for manual draw unity style array/list editor using RL prefixed internal styles.
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

		/// <summary>
		/// A helper for building RL style inspector elements inside an autolayout inspector.
		/// </summary>
		public static void DrawPanel(string title, Action content, Action footerButtons = null)
		{
			s_SharedContent.text = title;
			s_SharedContent.tooltip = string.Empty;
			DrawPanel(title, content, footerButtons);
		}

		/// <summary>
		/// A helper for building RL style inspector elements inside an autolayout inspector.
		/// </summary>
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

		// initializer for our RL style buttons.
		static void SetupPlusMinusButton(GUIStyle style)
		{
			style.fixedHeight = 0;
			style.imagePosition = ImagePosition.ImageOnly;
			style.margin.top = 0;
			style.padding = new RectOffset(2, 2, 8, 0);
			style.contentOffset = new Vector2(0, -10);
		}

		// lazy passthrough field initializer.
		static Texture2D Lazy(ref Texture2D field, string src)
		{
			if (field == null)
			{
				field = EditorGUIUtility.FindTexture(src);
			}

			return field;
		}

		static GUIStyle Lazy(ref GUIStyle field, GUIStyle src, Action<GUIStyle> initAction = null,
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
		
		
		/// <summary>
		/// Helper to try find another attribute of a specified type.
		/// </summary> 
		public static T TryGetAttribute<T>(this PropertyDrawer drawer) where T : Attribute
		{
			// Moved from CondtionalIncludePropertyDrawer.cs
			object[] result = drawer.fieldInfo.GetCustomAttributes(typeof(T), true);
			if (result != null && result.Length > 0)
			{
				return result[0] as T;
			}
			return null;
		}
		
		/// <summary>
		/// Returns a string representation of a property. 
		/// </summary> 
		public static string AsStringValue(this SerializedProperty property)
		{
			// Moved from CondtionalIncludePropertyDrawer.cs
			switch (property.propertyType)
			{
				case SerializedPropertyType.String:
					return property.stringValue;
				case SerializedPropertyType.Character:
				case SerializedPropertyType.Integer:
					if (property.type == "char") return Convert.ToChar(property.intValue).ToString();
					return property.intValue.ToString();
				case SerializedPropertyType.ObjectReference:
					return property.objectReferenceValue != null ? property.objectReferenceValue.ToString() : "null";
				case SerializedPropertyType.Boolean:
					return property.boolValue.ToString();
				case SerializedPropertyType.Enum:
					return property.enumNames[property.enumValueIndex];
				default:
					return string.Empty;
			}
		}
		
		
		/// <summary>
		/// Find a property realtive to property.
		/// </summary> 
		public static SerializedProperty FindPropertyRelative(this SerializedProperty property, string propertyName)
		{
			// Moved from CondtionalIncludePropertyDrawer.cs
			if (property.depth == 0) return property.serializedObject.FindProperty(propertyName);

			var path = property.propertyPath.Replace(".Array.data[", "[");
			var elements = path.Split('.');
			SerializedProperty parent = null;

		
			for (int i = 0; i < elements.Length - 1; i++)
			{
				var element = elements[i];
				int index = -1;
				if (element.Contains("["))
				{
					index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "").Replace("]", ""));
					element = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
				}
			
				parent = i == 0 ? 
					property.serializedObject.FindProperty(element) : 
					parent.FindPropertyRelative(element);

				if (index >= 0) parent = parent.GetArrayElementAtIndex(index);
			}

			return parent.FindPropertyRelative(propertyName);
		}
		
	}  
} 

#endif