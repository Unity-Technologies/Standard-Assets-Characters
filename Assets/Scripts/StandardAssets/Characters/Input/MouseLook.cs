using UnityEngine;

namespace StandardAssets.Characters.Input
{
    public class MouseLook :MonoBehaviour
    {
        public InputResponse input;

        public void Start()
        {
            Debug.Log("INIT");
            input.Init();
        }
    
        
        //THismakes life slow 
        void Update()
        {
            
            //input.Tick();
        }
    }
}