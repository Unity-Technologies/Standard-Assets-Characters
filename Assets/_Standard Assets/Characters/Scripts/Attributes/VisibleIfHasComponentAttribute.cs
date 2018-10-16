using System;
using UnityEngine;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// Attribute to make a field visible is a component is attached to the game object.
	/// </summary>
	public class VisibleIfHasComponentAttribute : PropertyAttribute
	{
		/// <summary>
		/// The component type to check for.
		/// </summary>
		public readonly Type componentType;
		
		/// <summary>
		/// Creates a new instance of <see cref="VisibleIfHasComponentAttribute"/> with the given componentType.
		/// </summary>
		public VisibleIfHasComponentAttribute(Type componentType)
		{
			this.componentType = componentType;
		}
	}
}