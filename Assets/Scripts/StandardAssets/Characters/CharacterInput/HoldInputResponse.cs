using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
    [CreateAssetMenu(fileName = "Hold Response", menuName = "New Input Response/Create Unity Experimental Input Hold Response", order = 1)]
    public class HoldInputResponse : InputResponse
    {
        
        public InputActionReference actionReference;
        
        
        public override void Init()
        {
            actionReference.action.performed += ctx => OnInputEnded();
            actionReference.action.cancelled += ctx => OnInputEnded();
            actionReference.action.started += ctx => OnInputStarted();
        }

        public override void Tick()
        {
           // Empty for now
        }

        //TODO Validate action reference to have slow tap modifier
        
    }
}