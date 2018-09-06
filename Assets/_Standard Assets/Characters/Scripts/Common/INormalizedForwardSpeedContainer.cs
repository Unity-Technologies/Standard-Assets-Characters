namespace StandardAssets.Characters.Common
{
	/// <summary>
	/// Interface for retrieving the normalized forward speed of an object.
	/// </summary>
	/// <remarks>Used by <see cref="StandardAssets.Characters.Physics.BaseCharacterPhysics"/> to apply gravity
	/// multipliers based on forward speed.</remarks>
	public interface INormalizedForwardSpeedContainer
	{
		/// <summary>
		/// Gets the normalized forward speed of the moving object.
		/// </summary>
		float normalizedForwardSpeed { get; }
	}
}