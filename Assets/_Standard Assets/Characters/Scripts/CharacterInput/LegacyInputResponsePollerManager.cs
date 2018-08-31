using System.Collections.Generic;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Singleton which manages all of the <see cref="LegacyInputResponsePoller"/>
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
		/// <param name="inputResponse">The <see cref="LegacyInputResponse"/> being set up</param>
		/// <param name="behaviour">The behaviour of the <see cref="LegacyInputResponse"/></param>
		/// <param name="axis">The name of the axis in the Input Manager</param>
		/// <param name="useAxisAsButton">The behaviour of an axis relating tp a button</param>
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