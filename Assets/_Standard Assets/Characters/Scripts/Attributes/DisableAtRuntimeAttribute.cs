using UnityEngine;

namespace Attributes
{
    public class DisableAtRuntimeAttribute : PropertyAttribute
    {
        public readonly bool enableIcon;

        public DisableAtRuntimeAttribute(bool enableIcon = true)
        {
            this.enableIcon = enableIcon;
        }
    }
}