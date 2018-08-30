using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// An attribute that will indent a field label on the Unity Editor by by the number specified (in Unity Indent Levels)
	/// </summary>
	public class IndentAttribute : PropertyAttribute
	{
		public readonly int indentLevel;

		/// <summary>
		/// Constructor
		/// </summary>
		public IndentAttribute(int indentLevel)
		{
			this.indentLevel = indentLevel;
		}

	}
}