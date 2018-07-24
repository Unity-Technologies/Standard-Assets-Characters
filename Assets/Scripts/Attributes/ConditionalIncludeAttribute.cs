using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

#endif

namespace Attributes
{
    public abstract class ConditionalIncludeAttribute : PropertyAttribute
    {
        public bool Toggle;
        public string Condition;

        public ConditionalIncludeAttribute()
        {
            Toggle = true;
        }

        public ConditionalIncludeAttribute(bool toggle)
        {
            Toggle = toggle;
        }

        public ConditionalIncludeAttribute(string condition)
        {
            Condition = condition;
        }
    }
    
   
}