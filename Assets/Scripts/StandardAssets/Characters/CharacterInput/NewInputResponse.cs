using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
    /// <inheritdoc />
    [CreateAssetMenu(fileName = "NewInputResponse", menuName = "New Input Response/Create Unity Experimental Input Response", order = 1)]
    public class NewInputResponse : InputResponse
    {
        /// <summary>
        /// The action reference - e.g.  crouch
        /// </summary>
        [SerializeField]
        private InputActionReference actionReference;

        private bool toggle;
        
        /// <inheritdoc />
        public override void Init()
        {
            actionReference.action.performed += ctx => TogglePress();
        }
        
        /// <inheritdoc />
        public override void Tick()
        {
            //TODO: refactor to use better Update mechanism in the base class so that this is unnecessary here
            //Do nothing
        }

        /// <summary>
        /// Handes the toggling of the presses
        /// </summary>
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
    }
}