namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Static class containing strings referencing animator states and parameters.
	/// </summary>
	public static class AnimationControllerInfo
	{
//KEVIN TODO: Remove the Space from the Locomotion State Name to standardize it with everything else		
		public const string k_LocomotionState 				= "Exploration Locomotion";
		public const string k_StrafeLocomotionState	 		= "Strafe Locomotion";
		public const string k_RightFootJumpState 			= "RightFootJump";
		public const string k_LeftFootJumpState 			= "LeftFootJump";
		public const string k_RightFootStrafeJump 			= "RightFootStrafeJump";
		public const string k_LeftFootStrafeJump 			= "LeftFootStrafeJump";
		public const string k_RollLandState 				= "RollLand";
		public const string k_LandState 					= "Land";
		public const string k_GroundedFootRightParameter	= "OnRightFoot";
		public const string k_VerticalSpeedParameter 		= "VerticalSpeed";
		public const string k_StrafeParameter 				= "Strafe";
		public const string k_FallParameter 				= "Fall";
		public const string k_ForwardSpeedParameter 		= "ForwardSpeed";
		public const string k_LateralSpeedParameter 		= "LateralSpeed";
		public const string k_TurningSpeedParameter 		= "TurningSpeed";
		public const string k_SpeedMultiplier 				= "SpeedMultiplier";
	}
}