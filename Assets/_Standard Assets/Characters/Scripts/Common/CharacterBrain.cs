using System;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.Common
{
	/// <summary>
	/// Abstract bass class for character brains
	/// </summary>
	[RequireComponent(typeof(CharacterPhysics))]
	[RequireComponent(typeof(ICharacterInput))]
	public abstract class CharacterBrain : MonoBehaviour, INormalizedForwardSpeedContainer
	{
		public Action<string> changeMovementZone;
		
		/// <summary>
		/// The Physic implementation used to do the movement
		/// e.g. CharacterController or Rigidbody (or New C# CharacterController analog)
		/// </summary>
		protected CharacterPhysics characterPhysics;

		/// <summary>
		/// The Input implementation to be used
		/// e.g. Default unity input or (in future) the new new input system
		/// </summary>
		protected ICharacterInput characterInput;

		/// <summary>
		/// Gets the physics implementation used by the Character
		/// </summary>
		public CharacterPhysics physicsForCharacter
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
			characterPhysics = GetComponent<CharacterPhysics>();
			characterInput = GetComponent<ICharacterInput>();
			lastPosition = transform.position;
		}

		/// <summary>
		/// Calculates the planarSpeed of the CharacterBrain
		/// </summary>
		protected virtual void Update()
		{
			Vector3 newPosition = transform.position;
			newPosition.y = 0f;
			float displacement = (lastPosition - newPosition).magnitude;
			planarSpeed = displacement / Time.deltaTime;
			lastPosition = newPosition;
		}

		public void ChangeMovementZone(string zoneId)
		{
			if (changeMovementZone != null)
			{
				changeMovementZone(zoneId);
			}
		}
	}
}