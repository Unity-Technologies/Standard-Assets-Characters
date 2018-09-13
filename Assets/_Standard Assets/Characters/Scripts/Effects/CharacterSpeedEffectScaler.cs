using UnityEngine;
using Util;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Abstract base class for effects that need to scale based on character speed
	/// </summary>
	public abstract class CharacterSpeedEffectScaler : MonoBehaviour
	{
		[SerializeField, Tooltip("The transform of the character. This is used to calculate velocity")]
		protected Transform characterTransform;

		[SerializeField, Tooltip("Used to normalize the velocity")]
		protected float maxSpeed = 10f;

		private Vector3 lastPosition;

		protected float normalizedSpeed;

		/// <summary>
		/// Caches the first position of character into lastPosition
		/// </summary>
		protected virtual void Awake()
		{
			if (characterTransform != null)
			{
				lastPosition = characterTransform.position;
				lastPosition.y = 0;
			}
		}

		/// <summary>
		/// Calculates normalized speed and applies the effect
		/// </summary>
		protected virtual void Update()
		{
			if (characterTransform != null)
			{
				Vector3 newPosition = characterTransform.position;
				newPosition.y = 0;
				float displacement = (lastPosition - newPosition).magnitude;
				float speed = displacement / Time.deltaTime;
				normalizedSpeed = Mathf.Clamp(speed / maxSpeed, 0, 1);
				ApplyNormalizedSpeedToEffect(normalizedSpeed);
				lastPosition = newPosition;	
			}
			else
			{
				normalizedSpeed = 1f; //If there is no character transform, set the normal speed to 1
			}
		}

		/// <summary>
		/// Scales effects based on normalized speed
		/// </summary>
		/// <param name="normalizedSpeed">the normalized character speed</param>
		protected abstract void ApplyNormalizedSpeedToEffect(float normalizedSpeed);
	}
}