using UnityEngine;

namespace StandardAssets.Characters.GizmosHelpers
{
	/// <summary>
	/// Attached to the GizmoArrow object to allow its colour to change in code
	/// </summary>
	public class ArrowColorSelect : MonoBehaviour
	{
		/// <summary>
		/// A collection of Mesh Renderers for all components of the GizmoArrow
		/// </summary>
		public MeshRenderer[] renderers;
		
		/// <summary>
		/// Color selected for this instance of the GizmoArrow, defaulted to Red.
		/// </summary>
		public Color color = Color.red;

		/// <summary>
		/// The collection of properties for all components of this instance of the GizmoArrow
		/// </summary>
		private MaterialPropertyBlock block;

		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		private void OnEnable()
		{
			UpdateColor();
		}

		/// <summary>
		/// Updates the arrow color on change in the inspector
		/// <remarks>
		/// Public access modifier to allow OnValidate from <see cref="GizmoArrowHelper"/>
		/// </remarks>
		/// </summary>
		public void OnValidate()
		{
			#if UNITY_EDITOR
				UpdateColor();
			#endif
		}
		
		/// <summary>
		/// Method to update the arrow color based on the color field.
		/// </summary>
		private void UpdateColor()
		{
			if (block == null)
			{
				block = new MaterialPropertyBlock();
			}
			block.SetColor("_Color", color);
			foreach (var r in renderers)
			{
				r.SetPropertyBlock(block);
			}
			
		}
	}
}