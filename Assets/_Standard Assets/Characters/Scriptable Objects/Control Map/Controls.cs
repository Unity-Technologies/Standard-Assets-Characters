// GENERATED AUTOMATICALLY FROM 'Assets/_Standard Assets/Characters/Scriptable Objects/Control Map/Controls.inputactions'

using System;
using UnityEngine;
using UnityEngine.Experimental.Input;


[Serializable]
public class Controls : InputActionAssetReference
{
    public Controls()
    {
    }
    public Controls(InputActionAsset asset)
        : base(asset)
    {
    }

    bool m_Initialized;

    void Initialize()
    {
        // Movement
        m_Movement = asset.GetActionMap("Movement");
        m_Movement_move = m_Movement.GetAction("move");
        m_Movement_look = m_Movement.GetAction("look");
        m_Movement_jump = m_Movement.GetAction("jump");
        m_Movement_strafe = m_Movement.GetAction("strafe");
        m_Movement_sprint = m_Movement.GetAction("sprint");
        m_Movement_crouch = m_Movement.GetAction("crouch");
        m_Movement_recentre = m_Movement.GetAction("recentre");
        m_Initialized = true;
    }

    void Uninitialize()
    {
        m_Movement = null;
        m_Movement_move = null;
        m_Movement_look = null;
        m_Movement_jump = null;
        m_Movement_strafe = null;
        m_Movement_sprint = null;
        m_Movement_crouch = null;
        m_Movement_recentre = null;
        m_Initialized = false;
    }
    public void SetAsset(InputActionAsset newAsset)
    {
        if (newAsset == asset) return;
        if (m_Initialized) Uninitialize();
        asset = newAsset;
    }
    public override void MakePrivateCopyOfActions()
    {
        SetAsset(ScriptableObject.Instantiate(asset));
    }
    // Movement
    InputActionMap m_Movement;
    InputAction m_Movement_move;
    InputAction m_Movement_look;
    InputAction m_Movement_jump;
    InputAction m_Movement_strafe;
    InputAction m_Movement_sprint;
    InputAction m_Movement_crouch;
    InputAction m_Movement_recentre;
    public struct MovementActions
    {
        Controls m_Wrapper;
        public MovementActions(Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @move { get { return m_Wrapper.m_Movement_move; } }
        public InputAction @look { get { return m_Wrapper.m_Movement_look; } }
        public InputAction @jump { get { return m_Wrapper.m_Movement_jump; } }
        public InputAction @strafe { get { return m_Wrapper.m_Movement_strafe; } }
        public InputAction @sprint { get { return m_Wrapper.m_Movement_sprint; } }
        public InputAction @crouch { get { return m_Wrapper.m_Movement_crouch; } }
        public InputAction @recentre { get { return m_Wrapper.m_Movement_recentre; } }
        public InputActionMap Get() { return m_Wrapper.m_Movement; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(MovementActions set) { return set.Get(); }
    }
    public MovementActions @Movement
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new MovementActions(this);
        }
    }
}
