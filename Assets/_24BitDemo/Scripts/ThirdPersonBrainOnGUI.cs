using StandardAssets.Characters.ThirdPerson;
using UnityEngine;

namespace Demo
{
	public class ThirdPersonBrainOnGUI : MonoBehaviour
	{
		private ThirdPersonBrain thirdPersonBrain;

		private void Start()
		{
			thirdPersonBrain = FindObjectOfType<ThirdPersonBrain>();
		}

		private void OnGUI()
		{
			if (thirdPersonBrain == null || !thirdPersonBrain.isActiveAndEnabled)
			{
				return;
			}
			
			string turnaroundType = "None";
			if (thirdPersonBrain.turnaround is BlendspaceTurnaroundBehaviour)
			{
				turnaroundType = "Blendspace";
			}
			else if (thirdPersonBrain.turnaround is AnimationTurnaroundBehaviour)
			{
				turnaroundType = "Animation";
			}
			
			GUI.Label(new Rect(Screen.width * 0.8f, 0, Screen.width * 0.2f, Screen.height * 0.1f),
			          string.Format("Turn around: {0}\nPress T to cycle", turnaroundType));

			var rootMotion = thirdPersonBrain.thirdPersonMotor;
			if (rootMotion != null)
			{
				GUI.Label(
					new Rect(Screen.width * 0.8f, Screen.height * 0.1f, Screen.width * 0.2f, Screen.height * 0.1f),
					string.Format("Sprint: {0}", rootMotion.sprint));
			}
		}
	}
}
