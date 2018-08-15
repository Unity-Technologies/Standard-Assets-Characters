using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class ThirdPersonCharacterBearing : CharacterBearing
	{
		public override Vector3 CalculateCharacterBearing()
		{
			Vector3 bearing = mainCamera.forward;
			bearing.y = 0f;
			bearing.Normalize();

			return bearing;
		}
	}
}