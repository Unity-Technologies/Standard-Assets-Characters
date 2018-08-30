using System.Collections.Generic;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Singleton which manages all of the LegacyInputResponsePollers
	/// </summary>
	public class LegacyInputResponsePollerManager : MonoBehaviour
	{
		/// <summary>
		/// Instance property
		/// </summary>
		public static LegacyInputResponsePollerManager instance { get; private set; }

		/// <summary>
		/// Mapping of the Input Response to a poller
		/// </summary>
		private Dictionary<LegacyInputResponse, LegacyInputResponsePoller> pollers = new Dictionary<LegacyInputResponse, LegacyInputResponsePoller>();
		
		/// <summary>
		/// Singleton checks
		/// </summary>
		private void Awake()
		{
			if (instance != null)
			{
				Destroy(gameObject);
			}
			else
			{
				instance = this;
			}
		}

		/// <summary>
		/// Creates the poller if it doesn't exist
		/// </summary>
		/// <param name="inputResponse"></param>
		/// <param name="behaviour"></param>
		/// <param name="axis"></param>
		/// <param name="useAxisAsButton"></param>
		public void InitPoller(LegacyInputResponse inputResponse, DefaultInputResponseBehaviour behaviour, string axis, AxisAsButton useAxisAsButton)
		{
			if (pollers.ContainsKey(inputResponse))
			{
				return;
			}
			
			GameObject pollorObj = new GameObject();
			pollorObj.name = string.Format("LegacyInput_{0}_Poller", inputResponse.name);
			pollorObj.transform.SetParent(transform);
			LegacyInputResponsePoller poller = pollorObj.AddComponent<LegacyInputResponsePoller>();
			poller.Init(inputResponse, behaviour, axis,useAxisAsButton);
			
			pollers.Add(inputResponse, poller);
		}
	}
}