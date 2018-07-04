using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[CreateAssetMenu(fileName = "AnimationMotorProperties", menuName = "Animation Third Person Motor Properties", order = 1)]
	public class AnimationMotorProperties : ScriptableObject
	{
		[SerializeField]
		protected AnimationInputProperties forwardMovement;

		[SerializeField]
		protected AnimationInputProperties strafeForwardMovement;

		[SerializeField]
		protected AnimationInputProperties strafeLateralMovement;
		
		public AnimationInputProperties forwardMovementProperties
		{
			get { return forwardMovement; }
		}

		public AnimationInputProperties strafeForwardMovementProperties
		{
			get { return strafeForwardMovement; }
		}

		public AnimationInputProperties strafeLateralMovementProperties
		{
			get { return strafeLateralMovement; }
		}
	}
}