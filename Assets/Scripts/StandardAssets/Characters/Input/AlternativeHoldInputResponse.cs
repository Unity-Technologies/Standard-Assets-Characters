using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Video;

namespace StandardAssets.Characters.Input
{
    [CreateAssetMenu(fileName = "ALT Hold Response", menuName = "New Input Response/Create Unity Experimental Input ALT Hold Response", order = 1)]
    public class AlternativeHoldInputResponse: InputResponse
    {
        public InputActionReference holdStartActionReference;
        public InputActionReference holdReleaseActionReference;

        private double triggerTime = 0;
        
        public override void Init()
        {
            
            //holdStartActionReference.action.performed += ctx => OnInputStarted();
            //holdReleaseActionReference.action.performed += ctx => OnInputEnded();
            
            holdStartActionReference.action.performed += ctx => HoldStart();

            holdReleaseActionReference.action.performed += ctx => HoldRelease();
            //holdReleaseActionReference.action.performed += ctx => HoldRelease();

        }

        void HoldStart()
        {
            Debug.Log("Hold Start");
        }
        
       /*
        *  //This method takes an Action with  NO modifier, and can log for a hold and release
        
        void HoldStart(InputAction.CallbackContext ctx)
        {
            if (triggerTime == 0)
            {
                triggerTime = ctx.action.lastTriggerStartTime;
          
                Debug.Log("HOLD Release");
                
            }
            else
            {
                if (triggerTime < ctx.action.lastTriggerTime)
                {
                    
                    triggerTime = 0;
                    Debug.Log("HOLD START");
                }
            }
            
        }
        */
        
        void HoldRelease()
        {
            Debug.Log("HOLD Release");
        }

        public override void Tick()
        {
            // Empty for now
        }
    }
}