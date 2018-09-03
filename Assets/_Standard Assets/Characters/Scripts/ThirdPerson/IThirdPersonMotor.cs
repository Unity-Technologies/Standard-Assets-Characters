using System;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// A mapping between movement values and values used in the animator
	/// </summary>
	public interface IThirdPersonMotor 
	{
		/// <summary>
		/// The turning speed.
		/// </summary>
		/// <value>Range =  -1 (rotate anticlockwise) to 1 (rotate clockwise). 0 is not turning.</value>
		float normalizedTurningSpeed { get; }
		
		/// <summary>
		/// The lateral speed.
		/// </summary>
		/// <value>Range = - 1 (strafe left) to 1 (strafe). 0 is no strafing.</value>
		float normalizedLateralSpeed { get; }
		
		/// <summary>
		/// The forward speed. 
		/// </summary>
		/// <value>Range = -1 (run backwards) to 1 (run forwards). 0 is no forward movement .</value>
		float normalizedForwardSpeed { get; }
		
		/// <summary>
		/// The vertical speed.
		/// </summary>
		/// <value>Range = -1 (falling) to 1 (jumping).</value>
		float normalizedVerticalSpeed { get; }
		
		/// <summary>
		/// The time that the character has been falling
		/// </summary>
		float fallTime { get; }
		
		/// <summary>
		/// The target y rotation to face.
		/// </summary>
		/// <value>An angle in degrees.</value>
		float targetYRotation { get; }
		
		/// <summary>
		/// Fired on jump
		/// </summary>
		Action jumpStarted { get; set; }
		
		/// <summary>
		/// When the character lands
		/// </summary>
		Action landed { get; set; }
		
		/// <summary>
		/// When the starts falling
		/// </summary>
		Action<float> fallStarted { get; set; }
		
		/// <summary>
		/// Fired for a rapid turn
		/// </summary>
		Action<float> rapidlyTurned { get; set; }
		
		/// <summary>
		/// Gets the current movement state.
		/// </summary>
		/// <value>Possible states include running, walking and turning around.</value>
		ThirdPersonGroundMovementState currentGroundMovementState { get; }
		
		/// <summary>
		/// Gets the current aerial state.
		/// </summary>
		/// <value>Possible states include grounded, falling and jump.</value>
		ThirdPersonAerialMovementState currentAerialMovementState { get; }
		
		/// <summary>
		/// Gets the current turnaround behaviour.
		/// </summary>
		/// <value>Either a blend space turnaround or animation based turnaround.</value>
		/// See <seealso cref="BlendspaceTurnaroundBehaviour"/> and <seealso cref="AnimationTurnaroundBehaviour"/>.
		TurnaroundBehaviour currentTurnaroundBehaviour { get; }

		void Init(ThirdPersonBrain brain);

		void Subscribe();

		void Unsubscribe();

		void Update();

		void OnAnimatorMove();
	}
}
