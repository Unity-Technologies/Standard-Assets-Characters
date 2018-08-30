using System.Security.AccessControl;
using UnityEngine;

namespace StandardAssets.Characters.GizmosHelpers
{ 
	public class ArrowColorSelect : MonoBehaviour
	{
		private MaterialPropertyBlock block;
		public MeshRenderer[] renderers;
		
		public Color Color = Color.red;

		private void OnEnable()
		{
			UpdateColor();
		}

		public void OnValidate()
		{
			UpdateColor();
		}
		
		

		void UpdateColor()
		{
			if(block == null)
				block = new MaterialPropertyBlock(); 
			block.SetColor("_Color", Color);
			foreach (var r in renderers)
			{
				r.SetPropertyBlock(block);
			}
			
		}
	}
}