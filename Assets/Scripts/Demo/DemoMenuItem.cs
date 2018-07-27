using UnityEngine;

using UnityEngine.SceneManagement;


namespace Demo
{
	public class DemoMenuItem : MonoBehaviour
	{
		[SerializeField]
		protected string sceneName;

		public void Clicked()
		{
			SceneManager.LoadScene(sceneName);
		}
	}
}