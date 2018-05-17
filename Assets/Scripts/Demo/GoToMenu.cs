using UnityEngine;
using UnityEngine.SceneManagement;

namespace Demo
{
	public class GoToMenu : MonoBehaviour
	{
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				SceneManager.LoadScene(0);
			}
		}
	}
}