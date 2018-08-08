using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[CreateAssetMenu(fileName = "Third Person Animation Configuration", menuName = "Standard Assets/Characters/Third Person Animation Configuration", order = 1)]
	public class ThirdPersonAnimationConfiguration : ScriptableObject
	{
		[SerializeField]
		protected float floatInterpolation = 0.05f;
		
		[SerializeField]
		protected string forwardSpeedParameter = "ForwardSpeed";
		
		[SerializeField]
		protected string lateralSpeedParameter = "LateralSpeed";
		
		[SerializeField]
		protected string turningSpeedParameter = "TurningSpeed";

		[SerializeField]
		protected string verticalSpeedParameter = "VerticalSpeed";
		
		[SerializeField]
		protected string groundedParameter = "Grounded";
		
		[SerializeField]
		protected string hasInputParameter = "HasInput";
		
		[SerializeField]
		protected string fallingTimeParameter = "FallTime";
		
		[SerializeField]
		protected string footednessParameter = "OnRightFoot";
		
		[SerializeField]
		protected string jumpedParameter = "Jumped";
		
		[SerializeField]
		protected string physicsJumpParameter = "PhysicsJump";
		
		[SerializeField]
		protected string jumpedLateralSpeedParameter = "JumpedLateralSpeed";
		
		[SerializeField]
		protected string jumpedForwardSpeedParameter = "JumpedForwardSpeed";

		[SerializeField]
		protected string predictedFallDistanceParameter = "PredictedFallDistance";
		
		[SerializeField]
		protected string rapidTurnParameter = "RapidTurn";

		[SerializeField]
		protected bool invertFootedness;

		[SerializeField]
		protected float footednessThreshold = 0.25f, footednessThresholdOffset = 0.25f;

		[SerializeField]
		protected float headLookAtWeight = 1f;

		[SerializeField]
		protected float headLookAtMaxRotation = 60f;

		[SerializeField]
		protected float headLookAtRotationSpeed = 90f;

		public float floatInterpolationTime
		{
			get { return floatInterpolation; }
		}

		public string forwardSpeedParameterName
		{
			get { return forwardSpeedParameter; }
		}

		public string lateralSpeedParameterName
		{
			get { return lateralSpeedParameter; }
		}

		public string turningSpeedParameterName
		{
			get { return turningSpeedParameter; }
		}

		public string verticalSpeedParameterName
		{
			get { return verticalSpeedParameter; }
		}

		public string groundedParameterName
		{
			get { return groundedParameter; }
		}

		public string hasInputParameterName
		{
			get { return hasInputParameter; }
		}

		public string fallingTimeParameterName
		{
			get { return fallingTimeParameter; }
		}

		public string footednessParameterName
		{
			get { return footednessParameter; }
		}

		public string jumpedParameterName
		{
			get { return jumpedParameter; }
		}

		public string jumpedLateralSpeedParameterName
		{
			get { return jumpedLateralSpeedParameter; }
		}

		public string jumpedForwardSpeedParameterName
		{
			get { return jumpedForwardSpeedParameter; }
		}

		public string predictedFallDistanceParameterName
		{
			get { return predictedFallDistanceParameter; }
		}

		public string rapidTurnParameterName
		{
			get { return rapidTurnParameter; }
		}

		public bool invertFoot
		{
			get { return invertFootedness; }
		}

		public float footednessThresholdValue
		{
			get { return footednessThreshold; }
		}

		public float footednessThresholdOffsetValue
		{
			get { return footednessThresholdOffset; }
		}

		public float lookAtWeight
		{
			get { return headLookAtWeight; }
		}

		public float lookAtMaxRotation
		{
			get { return headLookAtMaxRotation; }
		}

		public float lookAtRotationSpeed
		{
			get { return headLookAtRotationSpeed; }
		}

		public string physicsJump
		{
			get { return physicsJumpParameter; }
		}
	}
}