// GENERATED AUTOMATICALLY FROM 'Assets/Prefabs/FirstPerson/CrouchAction.inputactions'

[System.Serializable]
public class CrouchAction : UnityEngine.Experimental.Input.InputActionWrapper
{
    private bool m_Initialized;
    private void Initialize()
    {
        // default
        m_default = asset.GetActionMap("default");
        m_default_action = m_default.GetAction("action");
        m_Initialized = true;
    }
    // default
    private UnityEngine.Experimental.Input.InputActionMap m_default;
    private UnityEngine.Experimental.Input.InputAction m_default_action;
    public struct DefaultActions
    {
        private CrouchAction m_Wrapper;
        public DefaultActions(CrouchAction wrapper) { m_Wrapper = wrapper; }
        public UnityEngine.Experimental.Input.InputAction @action { get { return m_Wrapper.m_default_action; } }
        public UnityEngine.Experimental.Input.InputActionMap Get() { return m_Wrapper.m_default; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public UnityEngine.Experimental.Input.InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator UnityEngine.Experimental.Input.InputActionMap(DefaultActions set) { return set.Get(); }
    }
    public DefaultActions @default
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new DefaultActions(this);
        }
    }
}
