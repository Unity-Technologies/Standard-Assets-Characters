using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Singleton manager of level zone defaults
	/// </summary>
	public class LevelMovementZoneManager : MonoBehaviour
	{
		[SerializeField, Tooltip("References a ScriptableObject that contains default MovementEventLibraries for different zones")]
		protected LevelMovementZoneConfiguration configuration;
		
		/// <summary>
		/// Gets the instance of the LevelMovementZoneManager. Set privately
		/// </summary>
		/// <value>
		/// the <see cref="LevelMovementZoneManager"/> if the instance exists
		/// null if the instance does not exist
		/// </value>
		public static LevelMovementZoneManager instance { get; private set; }

		/// <summary>
		/// Checks if the instance exists
		/// </summary>
		/// <value>
		/// true if the instance exists.
		/// false if the instance does not exist.
		/// </value>
		public static bool instanceExists
		{
			get { return instance != null; }
		}
		
		/// <summary>
		/// Gets the <see cref="LevelMovementZoneConfiguration"/>
		/// </summary>
		/// <value><see cref="LevelMovementZoneConfiguration"/> if the instance exists. null if it does not</value>
		public static LevelMovementZoneConfiguration config
		{
			get
			{
				if (instanceExists)
				{
					return instance.configuration;
				}

				return null;
			}
		}

		/// <summary>
		/// Sets up the instance and destroys the instance if it already exists
		/// </summary>
		private void Awake()
		{
			if (!instanceExists)
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