using UnityEngine;

namespace StandardAssets.Characters.Common
{
	/// <summary>
	/// Abstract utility class used to calculate bearing of a character using the main camera
	/// </summary>
	public abstract class CharacterBearing
	{
		public Transform cameraMain { get; private set; }

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