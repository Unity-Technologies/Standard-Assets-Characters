using StandardAssets.Characters.Effects.Configs;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Singleton manager of level zone defaults
	/// </summary>
	public class LevelMovementZoneManager : MonoBehaviour
	{
		[SerializeField, Tooltip("Contains the level's default Movement Event Libraries for different zones")]
		LevelMovementZoneConfig m_Configuration;
		
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
		public static bool instanceExists{ get { return instance != null; } }
		
		/// <summary>
		/// Gets the <see cref="LevelMovementZoneConfig"/>
		/// </summary>
		/// <value><see cref="LevelMovementZoneConfig"/> if the instance exists. null if it does not</value>
		public static LevelMovementZoneConfig config { get { return instanceExists ? instance.m_Configuration : null; } }


		// Sets up the instance and destroys the instance if it already exists
		void Awake()
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