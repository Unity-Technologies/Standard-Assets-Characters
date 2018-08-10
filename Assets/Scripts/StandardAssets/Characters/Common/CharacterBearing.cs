using UnityEngine;

namespace StandardAssets.Characters.Common
{
	public abstract class CharacterBearing
	{
		protected Transform cameraMain;
		
		protected Transform mainCamera
		{
			get
			{
				if (cameraMain == null)
				{
					cameraMain = Camera.main.transform;
				}
				return cameraMain;
			}
		}
		
		public abstract Vector3 CalculateCharacterBearing();
	}
}