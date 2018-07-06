using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
	public class TurnSpeedOptionUiElement : MonoBehaviour
	{
		[SerializeField]
		protected Text optionName, walkRapidTurnOffset, runRapidTurnOffset, walkRapidTurnSpeed, runRapidTurnSpeed;

		[SerializeField]
		protected Image optionBacking;
		
		[SerializeField]
		protected Color inactiveColor = Color.white, activeColor = Color.cyan;

		public void Init(AnimationProperties properties)
		{
			optionName.text = properties.name;
			walkRapidTurnOffset.text = properties.walkRapidTurnOffset.ToString();
			runRapidTurnOffset.text = properties.runRapidTurnOffset.ToString();
			walkRapidTurnSpeed.text = properties.walkRapidTurnSpeed.ToString();
			runRapidTurnSpeed.text = properties.runRapidTurnSpeed.ToString();
		}

		public void Select()
		{
			optionBacking.color = activeColor;
		}

		public void Deselect()
		{
			optionBacking.color = inactiveColor;
		}
	}
}