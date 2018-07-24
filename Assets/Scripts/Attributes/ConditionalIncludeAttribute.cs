using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

#endif

namespace Attributes
{
    public class ConditionalIncludeAttribute : PropertyAttribute
    {
        public readonly string conditionField;
        public readonly object conditionElement;
        
        public ConditionalIncludeAttribute(string conditionField, object conditionElement)
        {
            this.conditionField = conditionField;
            this.conditionElement = conditionElement;
        }
        
    }
    
   
}