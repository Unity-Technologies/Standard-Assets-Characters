using System;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// A mapping between movement values and values used in the animator
	/// </summary>
	public interface IThirdPersonMotor 
	{
		/// <summary>
		/// Gets the turning speed.
		/// </summary>
		/// <value>Range =  -1 (rotate anticlockwise) to 1 (rotate clockwise). 0 is not turning.</value>
		float normalizedTurningSpeed { get; }
		
		/// <summary>
		/// Gets the lateral speed.
		/// </summary>
		/// <value>Range = - 1 (strafe left) to 1 (strafe). 0 is no strafing.</value>
		float normalizedLateralSpeed { get; }
		
		/// <summary>
		/// Gets the forward speed. 
		/// </summary>
		/// <value>Range = -1 (run backwards) to 1 (run forwards). 0 is no forward movement .</value>
		float normalizedForwardSpeed { get; }
		
		/// <summary>
		/// Gets the vertical speed.
		/// </summary>
		/// <value>Range = -1 (falling) to 1 (jumping).</value>
		float normalizedVerticalSpeed { get; }
		
		/// <summary>
		/// Gets the time that the character has been falling.
		/// </summary>
		/// <value>A time in seconds.</value>
		float fallTime { get; }
		
		/// <summary>
		/// The target y rotation to face.
		/// </summary>
		/// <value>An angle in degrees.</value>
		float targetYRotation { get; }

		/// <summary>
		/// Fired on jump.
		/// </summary>
		event Action jumpStarted;

		/// <summary>
		/// Fired when the character lands.
		/// </summary>
		event Action landed;

		/// <summary>
		/// Fired when the character starts falling.
		/// </summary>
		event Action<float> fallStarted;

		/// <summary>
		/// Fired for a rapid turn.
		/// </summary>
		event Action<float> rapidlyTurned;
		
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
		/// <value>Either <see cref="BlendspaceTurnaroundBehaviour"/> or <see cref="AnimationTurnaroundBehaviour"/>.</value>
		TurnaroundBehaviour currentTurnaroundBehaviour { get; }

		void Init(ThirdPersonBrain brain);

		void Subscribe();

		void Unsubscribe();

		void Update();

		void OnAnimatorMove();
	}
}
