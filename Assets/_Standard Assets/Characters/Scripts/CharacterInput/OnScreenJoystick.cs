using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.CharacterInput
{
	public class OnScreenJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
	{           
		[SerializeField, Tooltip("The travel distance for the joystick hat")]
		protected float joystickMovementLimit = 1f;

		[SerializeField, Tooltip("The base of the Joystick - does not move")]
		protected RectTransform joystickBase;
		
		[SerializeField, Tooltip("The hat of the Joystick - does move")]
		protected RectTransform joystickHat;

		[SerializeField, Range(0f, 1f), Tooltip("The deadzone of the input")] 
		protected float deadZone = 0.2f;
			
		protected Vector2 stickAxis = Vector2.zero;
		private Vector2 joystickPosition = Vector2.zero;

		public float onScreenJoystickHorizontalAxis { get { return stickAxis.x; } }
		public float onScreenJoystickVerticalAxis { get { return stickAxis.y; } }

		/// <summary>
		/// Cache the starting positions of the joystick
		/// </summary>
		private void Awake()
		{
			//Get starting transform of joystick
			joystickPosition.x = joystickBase.position.x;
			joystickPosition.y = joystickBase.position.y;
		}
		
		/// <summary>
		/// Takes the deadzone into account
		/// </summary>
		/// <param name="stickInput"></param>
		/// <returns></returns>
		private Vector2 ApplyOnScreenDeadZone(Vector2 stickInput)
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
		/// Calculate the position delta of the joystick hat
		/// Prevent the joystick hat from being dragged past the limit 
		/// </summary>
		/// <param name="eventData"></param>
		public void OnDrag(PointerEventData eventData)
		{
			Vector2 dragDirection = eventData.position - joystickPosition; 
			
			stickAxis = (dragDirection.magnitude > joystickBase.sizeDelta.x / 2f) 
				? dragDirection.normalized : dragDirection / (joystickBase.sizeDelta.x / 2f);
			
			joystickHat.anchoredPosition = (stickAxis * joystickBase.sizeDelta.x / 2f) * joystickMovementLimit;
		}

		/// <summary>
		/// Moves the joystick to the push position
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerDown(PointerEventData eventData)
		{
			OnDrag(eventData);
		}
		
		/// <summary>
		/// Reset the joystick top to its starting position
		/// Reset the inputVector to Zero 
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerUp(PointerEventData eventData)
		{
			stickAxis = Vector2.zero;
			joystickHat.anchoredPosition = Vector2.zero;
		}

		/// <summary>
		/// Gets the stick
		/// </summary>
		/// <returns></returns>
		public Vector2 GetStickVector()
		{
			return ApplyOnScreenDeadZone(stickAxis);
		}
	}
}