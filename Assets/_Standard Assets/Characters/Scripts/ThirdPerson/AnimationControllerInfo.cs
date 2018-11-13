namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Static class containing strings referencing animator states and parameters.
	/// </summary>
	public static class AnimationControllerInfo
	{
		public const string k_LocomotionState = "Exploration Locomotion",
		                    k_StrafeLocomotionState = "Strafe Locomotion",
		                    k_RightFootJumpState = "RightFootJump",
		                    k_LeftFootJumpState = "LeftFootJump",
		                    k_RightFootStrafeJump = "RightFootStrafeJump",
		                    k_LeftFootStrafeJump = "LeftFootStrafeJump",
		                    k_RollLandState = "RollLand",
		                    k_LandState = "Land";

		public const string k_GroundedFootRightParameter = "OnRightFoot",
		                    k_VerticalSpeedParameter = "VerticalSpeed",
		                    k_StrafeParameter = "Strafe",
		                    k_FallParameter = "Fall",
		                    k_ForwardSpeedParameter = "ForwardSpeed",
		                    k_LateralSpeedParameter = "LateralSpeed",
		                    k_TurningSpeedParameter = "TurningSpeed",
		                    k_SpeedMultiplier = "SpeedMultiplier";
	}
}