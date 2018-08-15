using System;
using UnityEngine;
using Attributes.Types;

namespace Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HelperBoxAttribute : PropertyAttribute
    {
        public readonly string text;
        public readonly HelperType type;

        public HelperBoxAttribute(HelperType type,string text)
        {
            this.type = type;
            this.text = text;
        }
    }
}

