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
			// we want this attribute to take priority over ConditionalInclude.
			base.order = 1000;
			this.type = type;
			this.text = text;
		}
	}
}

