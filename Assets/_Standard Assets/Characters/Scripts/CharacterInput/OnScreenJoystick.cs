using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering.UI;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.CharacterInput
{
	public class OnScreenJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
	{
		/// <summary>
		/// Travel distance of the on screen joystick hat over base. 
		/// </summary>
		[SerializeField, Tooltip("The travel distance for the joystick hat")]
		protected float joystickMovementLimit = 1f;
		
		/// <summary>
		/// Transform of the joystick base. 
		/// </summary>
		[SerializeField, Tooltip("The base of the Joystick - does not move")]
		protected RectTransform joystickBase;
		
		/// <summary>
		/// Transform of the joystick hat. 
		/// </summary>
		[SerializeField, Tooltip("The hat of the Joystick - does move")]
		protected RectTransform joystickHat;
		
		/// <summary>
		/// Dead zone for the on screen joystick hat.
		/// </summary>
		[SerializeField, Range(0f, 1f), Tooltip("The dead zone of the input")] 
		protected float deadZone = 0.2f;
		
		/// <summary>
		/// Position of the joystick hat. 
		/// </summary>
		private Vector2 stickAxisRaw = Vector2.zero;
		
		/// <summary>
		/// Position on the screen of the joystick base with hat in the center.
		/// </summary>
		private Vector2 joystickPosition = Vector2.zero;

		/// <summary>
		/// Cache the starting positions of the joystick.
		/// </summary>
		private void Awake()
		{
			joystickPosition = joystickBase.position;
		}

		/// <summary>
		/// Calculates the position delta of the on screen joystick hat and moves the hat on screen,
		/// keeping its position on screen within the bounds of the <see cref="joystickMovementLimit"/>.
		/// </summary>
		/// <param name="eventData">Pointer or touch event data</param>
		public void OnDrag(PointerEventData eventData)
		{
			Vector2 dragDirection = eventData.position - joystickPosition;
			float joystickBaseFootprintRadius = joystickBase.sizeDelta.x * 0.5f;
			float joystickHatRadius = joystickHat.sizeDelta.x * 0.5f;
			float radiusDifference = joystickBaseFootprintRadius - joystickHatRadius;
			
			stickAxisRaw = (dragDirection.sqrMagnitude > (radiusDifference*radiusDifference)) 
				? dragDirection.normalized : (dragDirection / joystickBaseFootprintRadius)*2f;	
			joystickHat.anchoredPosition = stickAxisRaw * radiusDifference * joystickMovementLimit;
		}

		/// <summary>
		/// Moves the joystick hat to the touch position.
		/// </summary>
		/// <param name="eventData">Pointer or touch event data</param>
		public void OnPointerDown(PointerEventData eventData)
		{
			OnDrag(eventData);
		}
		
		/// <summary>
		/// Reset the joystick hat to its starting position.
		/// </summary>
		/// <param name="eventData">Pointer or touch event data</param>
		public void OnPointerUp(PointerEventData eventData)
		{
			stickAxisRaw = Vector2.zero;
			joystickHat.anchoredPosition = Vector2.zero;
		}

		/// <summary>
		/// Gets the on screen analog stick vector
		/// </summary>
		/// <returns>Normalized vector with applied dead zone</returns>
		public Vector2 GetStickVector()
		{
			Vector2 stickInputVector = stickAxisRaw;
			float stickInputMagnitude = stickInputVector.magnitude;
			
			if (stickInputMagnitude < deadZone)
			{
				stickInputVector = Vector2.zero;
			}
			else
			{
				Vector2 stickInputNormalized = stickInputVector / stickInputMagnitude;
				stickInputVector = stickInputNormalized * ((stickInputMagnitude - deadZone));
			}
			return stickInputVector;
		}
	}
}