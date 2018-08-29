using UnityEngine;
using UnityEngine.EventSystems;

namespace StandardAssets.Characters.CharacterInput
{
	public class StaticOnScreenJoystick: MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
	{          
		[SerializeField]
		protected float joystickMovementLimmit = 1f; //The Travel distanc for the joystick top 

		protected Vector2 stickAxis = Vector2.zero;

		[SerializeField]
		protected RectTransform joystickBase;
		
		[SerializeField]
		protected RectTransform joystickHat;

		[SerializeField] 
		protected float deadZone = 0.2f;
			
		private Vector2 joystickPosition = Vector2.zero;

		public float onScreenJoystickHorizontalAxis { get { return stickAxis.x; } }
		public float onScreenJoystickVerticalAxis { get { return stickAxis.y; } }
	  

		private void Awake()
		{
			
			//gameObject.transform.parent.gameObject.SetActive(false);
			//Get starting transform of joystick
			joystickPosition.x = joystickBase.position.x;
			joystickPosition.y = joystickBase.position.y;
		}
	
		/// <summary>
		/// Calculate the position delata of the joystick hat
		/// Prevent the joystick hat from being dragged past the limit 
		/// </summary>
		/// <param name="eventData"></param>
		public void OnDrag(PointerEventData eventData)
		{
			Vector2 dragDirection = eventData.position - joystickPosition; 
			
			stickAxis = (dragDirection.magnitude > joystickBase.sizeDelta.x / 2f) 
				? dragDirection.normalized : dragDirection / (joystickBase.sizeDelta.x / 2f);
			
			joystickHat.anchoredPosition = (stickAxis * joystickBase.sizeDelta.x / 2f) * joystickMovementLimmit;
		}

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

		public Vector2 GetStickVector()
		{
			return ApplyOnScreenDeadZone(stickAxis);
		}
		
		Vector2 ApplyOnScreenDeadZone(Vector2 stickInput)
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
	}
}