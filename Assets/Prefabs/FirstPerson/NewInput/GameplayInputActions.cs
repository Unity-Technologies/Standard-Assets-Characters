// GENERATED AUTOMATICALLY FROM 'Assets/Prefabs/FirstPerson/NewInput/GameplayInputActions.inputactions'

[System.Serializable]
public class GameplayInputActions : UnityEngine.Experimental.Input.InputActionWrapper
{
    private bool m_Initialized;
    private void Initialize()
    {
        // controls
        m_controls = asset.GetActionMap("controls");
        m_controls_movement = m_controls.GetAction("movement");
        m_controls_look = m_controls.GetAction("look");
        m_controls_jump = m_controls.GetAction("jump");
        m_controls_crouch = m_controls.GetAction("crouch");
        m_controls_sprint = m_controls.GetAction("sprint");
        m_controls_prone = m_controls.GetAction("prone");
        m_Initialized = true;
    }
    // controls
    private UnityEngine.Experimental.Input.InputActionMap m_controls;
    private UnityEngine.Experimental.Input.InputAction m_controls_movement;
    private UnityEngine.Experimental.Input.InputAction m_controls_look;
    private UnityEngine.Experimental.Input.InputAction m_controls_jump;
    private UnityEngine.Experimental.Input.InputAction m_controls_crouch;
    private UnityEngine.Experimental.Input.InputAction m_controls_sprint;
    private UnityEngine.Experimental.Input.InputAction m_controls_prone;
    public struct ControlsActions
    {
        private GameplayInputActions m_Wrapper;
        public ControlsActions(GameplayInputActions wrapper) { m_Wrapper = wrapper; }
        public UnityEngine.Experimental.Input.InputAction @movement { get { return m_Wrapper.m_controls_movement; } }
        public UnityEngine.Experimental.Input.InputAction @look { get { return m_Wrapper.m_controls_look; } }
        public UnityEngine.Experimental.Input.InputAction @jump { get { return m_Wrapper.m_controls_jump; } }
        public UnityEngine.Experimental.Input.InputAction @crouch { get { return m_Wrapper.m_controls_crouch; } }
        public UnityEngine.Experimental.Input.InputAction @sprint { get { return m_Wrapper.m_controls_sprint; } }
        public UnityEngine.Experimental.Input.InputAction @prone { get { return m_Wrapper.m_controls_prone; } }
        public UnityEngine.Experimental.Input.InputActionMap Get() { return m_Wrapper.m_controls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public UnityEngine.Experimental.Input.InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator UnityEngine.Experimental.Input.InputActionMap(ControlsActions set) { return set.Get(); }
    }
    public ControlsActions @controls
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new ControlsActions(this);
        }
    }
}
