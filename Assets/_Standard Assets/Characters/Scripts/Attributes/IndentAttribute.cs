using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// An attribute that will indent a field label on the inspector by the number specified (in Unity Indent Levels)
	/// </summary>
	public class IndentAttribute : PropertyAttribute
	{
		/// <summary>
		/// Number of Indents that will shift this field's label to the right in the inspector
		/// </summary>
		public readonly int indentLevel;

		/// <summary>
		/// Creates a new instance of <see cref="IndentAttribute"/> with the given indentLevel
		/// </summary>
		public IndentAttribute(int indentLevel)
		{
			this.indentLevel = indentLevel;
		}

	}
}