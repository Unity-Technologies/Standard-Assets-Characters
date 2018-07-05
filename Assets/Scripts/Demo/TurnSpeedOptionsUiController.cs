using UnityEngine;

namespace Demo
{
	public class TurnSpeedOptionsUiController : MonoBehaviour
	{
		[SerializeField]
		protected TurnSpeedOptionUiElement uiPrefab;

		private TurnSpeedOptionUiElement[] uiElements;

		private int currentIndex = 0;

		public void Init(AnimationProperties[] properties)
		{
			int index = -1;
			uiElements = new TurnSpeedOptionUiElement[properties.Length];
			foreach (AnimationProperties animationProperties in properties)
			{
				index++;
				TurnSpeedOptionUiElement ui = Instantiate(uiPrefab);
				ui.transform.SetParent(transform, false);
				ui.Init(animationProperties);
				uiElements[index] = ui;
			}
		}

		public void SetIndex(int i)
		{
			uiElements[currentIndex].Deselect();
			currentIndex = i;
			uiElements[currentIndex].Select();
		}
	}
}