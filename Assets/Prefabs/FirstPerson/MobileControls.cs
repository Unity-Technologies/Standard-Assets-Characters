// GENERATED AUTOMATICALLY FROM 'Assets/Prefabs/FirstPerson/MobileControls.inputactions'

[System.Serializable]
public class MobileControls : UnityEngine.Experimental.Input.InputActionWrapper
{
    private bool m_Initialized;
    private void Initialize()
    {
        // gameplay
        m_gameplay = asset.GetActionMap("gameplay");
        m_gameplay_lookTouch = m_gameplay.GetAction("lookTouch");
        m_gameplay_movementTouch = m_gameplay.GetAction("movementTouch");
        m_gameplay_touchDelta = m_gameplay.GetAction("touchDelta");
        m_Initialized = true;
    }
    // gameplay
    private UnityEngine.Experimental.Input.InputActionMap m_gameplay;
    private UnityEngine.Experimental.Input.InputAction m_gameplay_lookTouch;
    private UnityEngine.Experimental.Input.InputAction m_gameplay_movementTouch;
    private UnityEngine.Experimental.Input.InputAction m_gameplay_touchDelta;
    public struct GameplayActions
    {
        private MobileControls m_Wrapper;
        public GameplayActions(MobileControls wrapper) { m_Wrapper = wrapper; }
        public UnityEngine.Experimental.Input.InputAction @lookTouch { get { return m_Wrapper.m_gameplay_lookTouch; } }
        public UnityEngine.Experimental.Input.InputAction @movementTouch { get { return m_Wrapper.m_gameplay_movementTouch; } }
        public UnityEngine.Experimental.Input.InputAction @touchDelta { get { return m_Wrapper.m_gameplay_touchDelta; } }
        public UnityEngine.Experimental.Input.InputActionMap Get() { return m_Wrapper.m_gameplay; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public UnityEngine.Experimental.Input.InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator UnityEngine.Experimental.Input.InputActionMap(GameplayActions set) { return set.Get(); }
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
