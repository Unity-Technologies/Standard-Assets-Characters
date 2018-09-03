using UnityEngine;
using UnityEngine.EventSystems;
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
		private Vector2 stickAxis = Vector2.zero;
		
		/// <summary>
		/// Position on the screen of the joystick base with hat in the center.
		/// </summary>
		private Vector2 joystickPosition = Vector2.zero;

		public float onScreenJoystickHorizontalAxis { get { return stickAxis.x; } }
		public float onScreenJoystickVerticalAxis { get { return stickAxis.y; } }

		/// <summary>
		/// Cache the starting positions of the joystick.
		/// </summary>
		private void Awake()
		{
			joystickPosition = joystickBase.position;
		}
		
		/// <summary>
		/// On Screen joystick dead zone.
		/// </summary>
		/// <param name="stickInput">The current position of the on screen joystick hat relative to its origin.</param>
		/// <returns>The on screen joystick input vector with applied dead zone.</returns>
		private Vector2 OnScreenDeadZone(Vector2 stickInput)
		{
			if (stickInput.magnitude < deadZone)
			{
				stickInput = Vector2.zero;
			}
			else
			{
				stickInput = stickInput.normalized * ((stickInput.magnitude - deadZone));
			}
			return stickInput;
		}

		/// <summary>
		/// Calculates the position delta of the on screen joystick hat and moves the hat on screen,
		/// keeping its position on screen within the bounds of the <see cref="joystickMovementLimit"/>.
		/// </summary>
		/// <param name="eventData"></param>
		public void OnDrag(PointerEventData eventData)
		{
			Vector2 dragDirection = eventData.position - joystickPosition;
			float joystickBaseFootprint = joystickBase.sizeDelta.x * 0.5f;
			stickAxis = (dragDirection.magnitude > joystickBaseFootprint) 
				? dragDirection.normalized : dragDirection / joystickBaseFootprint;
			joystickHat.anchoredPosition = (stickAxis * joystickBaseFootprint) * joystickMovementLimit;
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
			stickAxis = Vector2.zero;
			joystickHat.anchoredPosition = Vector2.zero;
		}

		/// <summary>
		/// Gets the on screen analog stick vector
		/// <see cref="stickAxis"/>
		/// </summary>
		/// <returns>Normalized vector with applied dead zone</returns>
		public Vector2 GetStickVector()
		{
			return OnScreenDeadZone(stickAxis);
		}
	}
}