
namespace StandardAssets.Characters.Input
{

// GENERATED AUTOMATICALLY FROM 'Assets/Prefabs/FirstPerson/DemoInputActions.inputactions'

    [System.Serializable]
    public class DemoInputActions : UnityEngine.Experimental.Input.InputActionWrapper
    {
        private bool m_Initialized;

        private void Initialize()
        {
            // gameplay
            m_gameplay = asset.GetActionMap("gameplay");
            m_gameplay_crouch = m_gameplay.GetAction("crouch");
            m_gameplay_prone = m_gameplay.GetAction("prone");
            m_gameplay_fire = m_gameplay.GetAction("fire");
            m_gameplay_movement = m_gameplay.GetAction("movement");
            m_gameplay_look = m_gameplay.GetAction("look");
            m_Initialized = true;
        }

        // gameplay
        private UnityEngine.Experimental.Input.InputActionMap m_gameplay;
        private UnityEngine.Experimental.Input.InputAction m_gameplay_crouch;
        private UnityEngine.Experimental.Input.InputAction m_gameplay_prone;
        private UnityEngine.Experimental.Input.InputAction m_gameplay_fire;
        private UnityEngine.Experimental.Input.InputAction m_gameplay_movement;
        private UnityEngine.Experimental.Input.InputAction m_gameplay_look;

        public struct GameplayActions
        {
            private DemoInputActions m_Wrapper;

            public GameplayActions(DemoInputActions wrapper)
            {
                m_Wrapper = wrapper;
            }

            public UnityEngine.Experimental.Input.InputAction @crouch
            {
                get { return m_Wrapper.m_gameplay_crouch; }
            }

            public UnityEngine.Experimental.Input.InputAction @prone
            {
                get { return m_Wrapper.m_gameplay_prone; }
            }

            public UnityEngine.Experimental.Input.InputAction @fire
            {
                get { return m_Wrapper.m_gameplay_fire; }
            }

            public UnityEngine.Experimental.Input.InputAction @movement
            {
                get { return m_Wrapper.m_gameplay_movement; }
            }

            public UnityEngine.Experimental.Input.InputAction @look
            {
                get { return m_Wrapper.m_gameplay_look; }
            }

            public UnityEngine.Experimental.Input.InputActionMap Get()
            {
                return m_Wrapper.m_gameplay;
            }

            public void Enable()
            {
                Get().Enable();
            }

            public void Disable()
            {
                Get().Disable();
            }

            public UnityEngine.Experimental.Input.InputActionMap Clone()
            {
                return Get().Clone();
            }

            public static implicit operator UnityEngine.Experimental.Input.InputActionMap(GameplayActions set)
            {
                return set.Get();
            }
        }

        public GameplayActions @gameplay
        {
            get
            {
                if (!m_Initialized) Initialize();
                return new GameplayActions(this);
            }
        }
    }

}