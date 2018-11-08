using UnityEngine;
using UnityEngine.SceneManagement;

namespace Demo
{
	public class ModeCanvas : MonoBehaviour
	{
		[SerializeField]
		protected GameObject firstPerson, thirdPerson, modeSelector;

		void Awake()
		{
			modeSelector.SetActive(true);
		}

		public void SelectFirstPerson()
		{
			firstPerson.SetActive(true);
			modeSelector.SetActive(false);
		}

		public void SelectThirdPerson()
		{
			thirdPerson.SetActive(true);
			modeSelector.SetActive(false);
		}

		public void Restart()
		{
			SceneManager.LoadScene(0);
		}

		public void Quit()
		{
			Application.Quit();
		}
	}
}