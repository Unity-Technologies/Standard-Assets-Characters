using UnityEngine;

namespace Attributes
{
    public class HelperBoxAttribute : PropertyAttribute
    {
        public readonly string text;

        public HelperBoxAttribute()
        {
            text = "";
        }

        public HelperBoxAttribute(string _title)
        {
            text = _title;
        }
}
}