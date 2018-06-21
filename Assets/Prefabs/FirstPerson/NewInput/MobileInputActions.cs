// GENERATED AUTOMATICALLY FROM 'Assets/Prefabs/FirstPerson/NewInput/MobileInputActions.inputactions'

[System.Serializable]
public class MobileInputActions : UnityEngine.Experimental.Input.InputActionWrapper
{
    private bool m_Initialized;
    private void Initialize()
    {
        // gamePlay
        m_gamePlay = asset.GetActionMap("gamePlay");
        m_gamePlay_movement = m_gamePlay.GetAction("movement");
        m_gamePlay_look = m_gamePlay.GetAction("look");
        m_Initialized = true;
    }
    // gamePlay
    private UnityEngine.Experimental.Input.InputActionMap m_gamePlay;
    private UnityEngine.Experimental.Input.InputAction m_gamePlay_movement;
    private UnityEngine.Experimental.Input.InputAction m_gamePlay_look;
    public struct GamePlayActions
    {
        private MobileInputActions m_Wrapper;
        public GamePlayActions(MobileInputActions wrapper) { m_Wrapper = wrapper; }
        public UnityEngine.Experimental.Input.InputAction @movement { get { return m_Wrapper.m_gamePlay_movement; } }
        public UnityEngine.Experimental.Input.InputAction @look { get { return m_Wrapper.m_gamePlay_look; } }
        public UnityEngine.Experimental.Input.InputActionMap Get() { return m_Wrapper.m_gamePlay; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public UnityEngine.Experimental.Input.InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator UnityEngine.Experimental.Input.InputActionMap(GamePlayActions set) { return set.Get(); }
    }
    public GamePlayActions @gamePlay
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new GamePlayActions(this);
        }
    }
}
