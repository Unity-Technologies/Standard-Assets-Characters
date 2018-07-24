using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HelperBoxAttribute : PropertyAttribute
    {
        public readonly string text;
#if UNITY_EDITOR
        public readonly MessageType type;
#endif

        /// <summary>
        /// Adds a HelperBox to the Unity inspector above this field.
        /// </summary>
        public HelperBoxAttribute(string text
#if UNITY_EDITOR
            , MessageType type = MessageType.Info
#endif
        )
        {
            this.text = text;
#if UNITY_EDITOR
            this.type = type;
#endif
        }
    }
}

