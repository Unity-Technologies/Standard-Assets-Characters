using System;
using UnityEngine;
using UnityEngine.Experimental.Input;


namespace StandardAssets.Characters.Input
{
    [CreateAssetMenu(fileName = "NewInputResponse", menuName = "New Input Response/Create Unity Experimental Input Response", order = 1)]
    public class NewInputResponse : InputResponse
    {
        public InputActionReference actionReference;

        private bool toggle = false;
        
        public override void Init()
        {
            actionReference.action.performed += ctx => TogglePress();

        }

        private void TogglePress()
        {
            if (toggle)
            {
                OnDisabled();
            }
            else
            {
                OnEnabled();
            }

            toggle = !toggle;
        }

        public override void Tick()
        {
            
        }
    }
}