using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
    [CreateAssetMenu(fileName = "NewInputResponse", menuName = "New Input Response/Create Unity Experimental Mouse Input Response", order = 1)]
    public class MouseLookResponse:InputResponse
    {
        public InputActionReference actionReference;

        private Vector2 m_Look;

       
        public override void Init()
        {
            actionReference.action.performed += ctx => m_Look = ctx.ReadValue<Vector2>();
        }

        
        public override void Tick()
        {
            Debug.Log(m_Look.ToString());
        }
    }
}