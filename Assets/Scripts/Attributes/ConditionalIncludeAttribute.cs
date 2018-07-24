using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

#endif

namespace Attributes
{
    public class ConditionalIncludeAttribute : PropertyAttribute
    {
        public readonly string conditionField;
        public readonly object enumElement;
        
        public ConditionalIncludeAttribute(string conditionField, object enumElement)
        {
            this.conditionField = conditionField;
            this.enumElement = enumElement;
        }
        
    }
    
   
}