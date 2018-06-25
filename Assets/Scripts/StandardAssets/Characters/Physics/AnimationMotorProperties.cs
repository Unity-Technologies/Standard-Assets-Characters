using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	[CreateAssetMenu(fileName = "AnimationMotorProperties", menuName = "Animation Third Person Motor Properties", order = 1)]
	public class AnimationMotorProperties : ScriptableObject
	{
		[SerializeField]
		private float inputForwardVelocity = 0.5f,
		              inputForwardDecay = 10f,
		              inputForwardChangeVelocity = 5f,
		              inputLateralVelocity = 0.5f,
		              inputLateralDecay = 10f,
		              inputLateralChangeVelocity = 5f,
					  walkProportionOfSpeed = 0.5f;

		public float forwardInputVelocity
		{
			get { return inputForwardVelocity; }
		}

		public float forwardInputDecay
		{
			get { return inputForwardDecay; }
		}

		public float forwardInputChangeVelocity
		{
			get { return inputForwardChangeVelocity; }
		}

		public float lateralInputVelocity
		{
			get { return inputLateralVelocity; }
		}

		public float lateralInputDecay
		{
			get { return inputLateralDecay; }
		}

		public float lateralInputChangeVelocity
		{
			get { return inputLateralChangeVelocity; }
		}

		public float walkSpeedProportion
		{
			get { return walkProportionOfSpeed; }
		}
	}
}