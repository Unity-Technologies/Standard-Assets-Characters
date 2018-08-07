using System.Collections.Generic;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
    public class LegacyInputResponsePollerManager : MonoBehaviour
    {
        public static LegacyInputResponsePollerManager instance { get; private set; }

        private Dictionary<LegacyInputResponse, LegacyInputResponsePoller> pollers = new Dictionary<LegacyInputResponse, LegacyInputResponsePoller>();
        
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

        public void InitPoller(LegacyInputResponse inputResponse, DefaultInputResponseBehaviour behaviour, string axis, bool useAxisAsButton)
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