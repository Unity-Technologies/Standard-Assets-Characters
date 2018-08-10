using UnityEngine;


namespace Attributes
{
    public class ConditionalIncludeAttribute : PropertyAttribute
    {
        public readonly string conditionField;
        public readonly object conditionElement;
        
        public ConditionalIncludeAttribute(string conditionField, object conditionElement = null)
        {
            this.conditionField = conditionField;
            this.conditionElement = conditionElement;
        }
    }
}