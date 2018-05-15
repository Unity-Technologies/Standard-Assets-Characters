using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
    public class CameraMoveSwitcher:MonoBehaviour,IThirdPersonMotorStateMachine
    {
        
        
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Z))
            {
                idling();
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.X))
            {
                walking();
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.C))
            {
                running();
            }
        }

        public Action idling { get; set; }
        public Action walking { get; set; }
        public Action running { get; set; }
    }
}