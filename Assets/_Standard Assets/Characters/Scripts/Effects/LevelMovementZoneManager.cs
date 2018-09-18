using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	public class LevelMovementZoneManager : MonoBehaviour
	{
		[SerializeField]
		protected LevelMovementZoneConfiguration configuration;
		
		public static LevelMovementZoneManager instance { get; private set; }

		public static LevelMovementZoneConfiguration config
		{
			get
			{
				if (instance != null)
				{
					return instance.configuration;
				}

				return null;
			}
		}

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}
}