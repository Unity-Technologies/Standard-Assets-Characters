using UnityEngine;

namespace Demo
{
	public class DebugHelpers : MonoBehaviour
	{
		[SerializeField]
		protected int targetFrameRate = 15;

		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
			{
				if (Time.timeScale > 0.1f)
				{
					Time.timeScale -= 0.1f;
				}
			}

			if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				if (Time.timeScale < 1f)
				{
					Time.timeScale += 0.1f;
				}
			}

			if (Input.GetKeyDown(KeyCode.L))
			{
				if (Application.targetFrameRate < 0)
				{
					Application.targetFrameRate = targetFrameRate;
				}
				else
				{
					Application.targetFrameRate = -1;
				}
			}
		}
	}
}