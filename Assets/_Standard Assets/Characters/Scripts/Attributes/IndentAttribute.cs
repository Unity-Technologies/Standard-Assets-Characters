using UnityEngine;

namespace Attributes
{
	public class IndentAttribute : PropertyAttribute
	{
		public readonly int indentLevel;

		public IndentAttribute(int indentLevel)
		{
			this.indentLevel = indentLevel;
		}

	}
}