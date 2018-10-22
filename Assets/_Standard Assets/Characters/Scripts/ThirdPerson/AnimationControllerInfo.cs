namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Static class containing strings referencing animator states and parameters.
	/// </summary>
	public static class AnimationControllerInfo
	{
		public const string k_LocomotionState = "Action Locomotion",
		                    k_StrafeLocomotionState = "Strafe Locomotion",
		                    k_RightFootJumpState = "RightFootPhysicsJump",
		                    k_LeftFootJumpState = "LeftFootPhysicsJump",
		                    k_RollLandState = "RollLand",
		                    k_LandState = "Land";

		public const string k_JumpedLateralSpeedParameter = "JumpedLateralSpeed",
		                    k_GroundedFootRightParameter = "OnRightFoot",
		                    k_JumpedForwardSpeedParameter = "JumpedForwardSpeed",
		                    k_VerticalSpeedParameter = "VerticalSpeed",
		                    k_StrafeParameter = "Strafe",
		                    k_FallParameter = "Fall",
		                    k_ForwardSpeedParameter = "ForwardSpeed",
		                    k_LateralSpeedParameter = "LateralSpeed",
		                    k_TurningSpeedParameter = "TurningSpeed",
		                    k_SpeedMultiplier = "SpeedMultiplier";
	}
}