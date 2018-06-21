// GENERATED AUTOMATICALLY FROM 'Assets/Prefabs/FirstPerson/NewInput/GameplayInputActions.inputactions'

[System.Serializable]
public class GameplayInputActions : UnityEngine.Experimental.Input.InputActionWrapper
{
    private bool m_Initialized;
    private void Initialize()
    {
        // gamepad
        m_gamepad = asset.GetActionMap("gamepad");
        m_gamepad_movement = m_gamepad.GetAction("movement");
        m_gamepad_look = m_gamepad.GetAction("look");
        m_gamepad_jump = m_gamepad.GetAction("jump");
        m_gamepad_crouch = m_gamepad.GetAction("crouch");
        m_gamepad_sprint = m_gamepad.GetAction("sprint");
        m_gamepad_prone = m_gamepad.GetAction("prone");
        m_Initialized = true;
    }
    // gamepad
    private UnityEngine.Experimental.Input.InputActionMap m_gamepad;
    private UnityEngine.Experimental.Input.InputAction m_gamepad_movement;
    private UnityEngine.Experimental.Input.InputAction m_gamepad_look;
    private UnityEngine.Experimental.Input.InputAction m_gamepad_jump;
    private UnityEngine.Experimental.Input.InputAction m_gamepad_crouch;
    private UnityEngine.Experimental.Input.InputAction m_gamepad_sprint;
    private UnityEngine.Experimental.Input.InputAction m_gamepad_prone;
    public struct GamepadActions
    {
        private GameplayInputActions m_Wrapper;
        public GamepadActions(GameplayInputActions wrapper) { m_Wrapper = wrapper; }
        public UnityEngine.Experimental.Input.InputAction @movement { get { return m_Wrapper.m_gamepad_movement; } }
        public UnityEngine.Experimental.Input.InputAction @look { get { return m_Wrapper.m_gamepad_look; } }
        public UnityEngine.Experimental.Input.InputAction @jump { get { return m_Wrapper.m_gamepad_jump; } }
        public UnityEngine.Experimental.Input.InputAction @crouch { get { return m_Wrapper.m_gamepad_crouch; } }
        public UnityEngine.Experimental.Input.InputAction @sprint { get { return m_Wrapper.m_gamepad_sprint; } }
        public UnityEngine.Experimental.Input.InputAction @prone { get { return m_Wrapper.m_gamepad_prone; } }
        public UnityEngine.Experimental.Input.InputActionMap Get() { return m_Wrapper.m_gamepad; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public UnityEngine.Experimental.Input.InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator UnityEngine.Experimental.Input.InputActionMap(GamepadActions set) { return set.Get(); }
    }
    public GamepadActions @gamepad
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new GamepadActions(this);
        }
    }
}
