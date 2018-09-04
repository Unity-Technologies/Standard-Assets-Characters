using System;
using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Camera events which the OpenCharacterController listens to during startup.
	/// </summary>
	public class OpenCharacterControllerCameraEvents : MonoBehaviour
	{
		/// <summary>
		/// Fired when OnPreRender is called. 
		/// </summary>
		public event Action preRendered;
		
		private void OnPreRender()
		{
			if (preRendered != null)
			{
				preRendered();
			}
		}
	}
}