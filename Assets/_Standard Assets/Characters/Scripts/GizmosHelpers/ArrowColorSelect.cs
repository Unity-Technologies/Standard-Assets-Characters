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
		public Color Color = Color.red;

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

#if UNITY_EDITOR
		/// <summary>
		/// Runs the OnValidate Monobehaviour method
		/// Public access modifier to allow OnValidate from <see cref="GizmoArrowHelper"/>
		/// </summary>
		public void OnValidate()
		{
			UpdateColor();
		}
#endif

		/// <summary>
		/// Method to update the arrow color based on the color field.
		/// </summary>
		private void UpdateColor()
		{
			if (block == null)
			{
				block = new MaterialPropertyBlock();
			}
			block.SetColor("_Color", Color);
			foreach (var r in renderers)
			{
				r.SetPropertyBlock(block);
			}
			
		}
	}
}