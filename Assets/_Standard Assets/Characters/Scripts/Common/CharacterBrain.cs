using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.Common
{
	/// <summary>
	/// Abstract bass class for character brains
	/// </summary>
	[RequireComponent(typeof(ICharacterPhysics))]
	[RequireComponent(typeof(ICharacterInput))]
	public abstract class CharacterBrain : MonoBehaviour, INormalizedForwardSpeedContainer
	{
		/// <summary>
		/// The Physic implementation used to do the movement
		/// e.g. CharacterController or Rigidbody (or New C# CharacterController analog)
		/// </summary>
		protected ICharacterPhysics characterPhysics;

		/// <summary>
		/// The Input implementation to be used
		/// e.g. Default unity input or (in future) the new new input system
		/// </summary>
		protected ICharacterInput characterInput;

		/// <summary>
		/// Used to report the character bearing for different kinds of movement
		/// </summary>
		protected CharacterBearing characterBearing;

		/// <summary>
		/// Gets the physics implementation used by the Character
		/// </summary>
		public ICharacterPhysics physicsForCharacter
		{
			get { return characterPhysics; }
		}

		/// <summary>
		/// Gets the input implementation used by the Character
		/// </summary>
		public ICharacterInput inputForCharacter
		{
			get { return characterInput; }
		}

		/// <summary>
		/// Gets the bearing of the character to be used by different movements 
		/// </summary>
		public CharacterBearing bearingOfCharacter
		{
			get { return characterBearing; }
		}

		/// <summary>
		/// Gets/sets the planar speed (i.e. ignoring the displacement) of the CharacterBrain
		/// </summary>
		public float planarSpeed { get; protected set; }
		
		private Vector3 lastPosition;

		/// <inheritdoc/>
		public abstract float normalizedForwardSpeed { get;}
		
		/// <summary>
		/// Gets the movement event handler.
		/// </summary>
		public abstract MovementEventHandler movementEventHandler { get; }

		public abstract float targetYRotation { get; set; }

		/// <summary>
		/// Get physics and input on Awake
		/// </summary>
		protected virtual void Awake()
		{
			characterPhysics = GetComponent<ICharacterPhysics>();
			characterInput = GetComponent<ICharacterInput>();
			Cursor.lockState = CursorLockMode.Locked;
			lastPosition = transform.position;
		}

		/// <summary>
		/// Calculates the planarSpeed of the CharacterBrain
		/// </summary>
		protected virtual void Update()
		{
			Vector3 newPosition = transform.position;
			newPosition.y = 0;
			float displacement = (lastPosition - newPosition).magnitude;
			planarSpeed = displacement / Time.deltaTime;
			lastPosition = newPosition;
		}
	}
}