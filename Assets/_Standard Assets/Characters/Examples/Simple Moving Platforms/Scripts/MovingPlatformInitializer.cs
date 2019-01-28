using StandardAssets.Characters.FirstPerson;
using UnityEngine;

namespace StandardAssets.Characters.Examples.SimpleMovingPlatforms
{
	/// <summary>
	/// Initialize the moving platforms for a third person or first person player.
	/// </summary>
	public class MovingPlatformInitializer : MonoBehaviour
	{
		// Initialize the platforms. Check if a first person input is active in the scene.
		private void Start()
		{
			InitializePlatforms(FindObjectOfType<FirstPersonInput>() != null);
		}

		/// <summary>
		/// Initialize the moving platforms for a third person or first person player.
		/// </summary>
		/// <param name="useFixedUpdate">Use FixedUpdate instead of Update (e.g. set this to true for first person).</param>
		public void InitializePlatforms(bool useFixedUpdate)
		{
			var controllers = FindObjectsOfType<MovingPlatformController>();
			if (controllers == null || controllers.Length <= 0)
			{
				return;
			}
			for (int i = 0, len = controllers.Length; i < len; i++)
			{
				var controller = controllers[i];
				if (controller != null)
				{
					controller.useFixedUpdate = useFixedUpdate;
				}
			}
		}
	}
}
