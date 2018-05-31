using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
    [CreateAssetMenu(fileName = "NewInputResponse", menuName = "New Input Response/Create Unity Experimental Input Response", order = 1)]
    public class NewInputResponse : InputResponse
    {
        public InputActionReference actionReference;

        private bool toggle;
        
        public override void Init()
        {
            
            actionReference.action.performed += ctx => TogglePress();

        }

        private void TogglePress()
        {   
            if (toggle)
            {
                OnInputEnded();
            }
            else
            {
                OnInputStarted();
            }

            toggle = !toggle;
        }

        public override void Tick()
        {
            
        }
    }
}