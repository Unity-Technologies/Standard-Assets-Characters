// GENERATED AUTOMATICALLY FROM 'Assets/_Standard Assets/Characters/Scriptable Objects/Control Map/StandardControls.inputactions'
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class StandardControls : IInputActionCollection
{
    private InputActionAsset asset;
    public StandardControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""StandardControls"",
    ""maps"": [
        {
            ""name"": ""Movement"",
            ""id"": ""401f0562-653c-45d7-b6af-d43a16e5e984"",
            ""actions"": [
                {
                    ""name"": ""crouch"",
                    ""id"": ""5800e495-214e-474b-b3e0-57242409f8c1"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": """",
                    ""bindings"": []
                },
                {
                    ""name"": ""look"",
                    ""id"": ""18ffe753-4cc1-45fc-8377-f9b5292b1a4b"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": """",
                    ""bindings"": []
                },
                {
                    ""name"": ""move"",
                    ""id"": ""21a14e86-4fb7-49f9-af14-ddbfc9451078"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": """",
                    ""bindings"": []
                },
                {
                    ""name"": ""recentre"",
                    ""id"": ""f8e6d38a-3004-4632-bb57-209532eda966"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": """",
                    ""bindings"": []
                },
                {
                    ""name"": ""sprint"",
                    ""id"": ""cba1b438-67f2-4fc1-b3d0-9129c5ba7640"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": """",
                    ""bindings"": []
                },
                {
                    ""name"": ""strafe"",
                    ""id"": ""fb80f938-13a8-4a49-aacb-b1323e025a3d"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": """",
                    ""bindings"": []
                },
                {
                    ""name"": ""strafeToggle"",
                    ""id"": ""75ce618e-7b75-4ffb-a37e-d115370f4cc5"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": """",
                    ""bindings"": []
                },
                {
                    ""name"": ""jump"",
                    ""id"": ""e0e21268-ad74-44f1-bd3d-e2909ad65769"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": """",
                    ""bindings"": []
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""b5e4169a-ddc2-444e-8947-5c50310ce7ce"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""e7171b02-bd2f-4eaa-90a5-3de8ca9d7c4b"",
                    ""path"": ""<Gamepad>/rightStickPress"",
                    ""interactions"": ""Press,Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""c8406335-017f-4795-bec7-089df9ddbf19"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""6350393e-2ea8-4399-88ad-38aa713e5cc5"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""08a6f928-9ff7-4bfd-94a8-42d5c1e5965c"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": ""Dpad"",
                    ""id"": ""a0d87c4f-1156-4d0f-8f03-bcad93861151"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": ""up"",
                    ""id"": ""21f6ae24-62f4-40dc-8d9d-3fdb6534456c"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true,
                    ""modifiers"": """"
                },
                {
                    ""name"": ""down"",
                    ""id"": ""d4b88c44-0df0-4a3e-b363-14930e8dd521"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true,
                    ""modifiers"": """"
                },
                {
                    ""name"": ""left"",
                    ""id"": ""e7855ea9-d1b8-4a10-8cea-c105801a6079"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true,
                    ""modifiers"": """"
                },
                {
                    ""name"": ""right"",
                    ""id"": ""a00dce50-ead7-41fe-a956-351297d70946"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""5823f7a4-35f6-4490-8075-00e60e53960a"",
                    ""path"": ""<Keyboard>/v"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""recentre"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""8a3fdf70-fad4-4d19-bcf7-434572a3a2ff"",
                    ""path"": ""<Gamepad>/rightStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""recentre"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""effb5bdc-89f2-4815-931b-381c603a16ed"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""6916ba45-ea9d-43b1-909c-427e71ef7072"",
                    ""path"": ""<Gamepad>/leftStickPress"",
                    ""interactions"": ""Press,Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""480c3b85-5bd5-47bd-a6d1-a4fa3d4698d0"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""strafe"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""afaf61b1-2cc6-4419-87cc-1658b7d3796f"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": ""Press,Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""strafe"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""7b3fcf09-ec1d-41c8-9a2b-f51ce04f6260"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""strafeToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""d71f3c76-ea57-455a-817f-7a2a9708f387"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""dc020caf-f33b-4678-a947-14711e61e59a"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Movement
        m_Movement = asset.GetActionMap("Movement");
        m_Movement_crouch = m_Movement.GetAction("crouch");
        m_Movement_look = m_Movement.GetAction("look");
        m_Movement_move = m_Movement.GetAction("move");
        m_Movement_recentre = m_Movement.GetAction("recentre");
        m_Movement_sprint = m_Movement.GetAction("sprint");
        m_Movement_strafe = m_Movement.GetAction("strafe");
        m_Movement_strafeToggle = m_Movement.GetAction("strafeToggle");
        m_Movement_jump = m_Movement.GetAction("jump");
    }

    ~StandardControls()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes
    {
        get => asset.controlSchemes;
    }

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Movement
    private InputActionMap m_Movement;
    private IMovementActions m_MovementActionsCallbackInterface;
    private InputAction m_Movement_crouch;
    private InputAction m_Movement_look;
    private InputAction m_Movement_move;
    private InputAction m_Movement_recentre;
    private InputAction m_Movement_sprint;
    private InputAction m_Movement_strafe;
    private InputAction m_Movement_strafeToggle;
    private InputAction m_Movement_jump;
    public struct MovementActions
    {
        private StandardControls m_Wrapper;
        public MovementActions(StandardControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @crouch { get { return m_Wrapper.m_Movement_crouch; } }
        public InputAction @look { get { return m_Wrapper.m_Movement_look; } }
        public InputAction @move { get { return m_Wrapper.m_Movement_move; } }
        public InputAction @recentre { get { return m_Wrapper.m_Movement_recentre; } }
        public InputAction @sprint { get { return m_Wrapper.m_Movement_sprint; } }
        public InputAction @strafe { get { return m_Wrapper.m_Movement_strafe; } }
        public InputAction @strafeToggle { get { return m_Wrapper.m_Movement_strafeToggle; } }
        public InputAction @jump { get { return m_Wrapper.m_Movement_jump; } }
        public InputActionMap Get() { return m_Wrapper.m_Movement; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(MovementActions set) { return set.Get(); }
        public void SetCallbacks(IMovementActions instance)
        {
            if (m_Wrapper.m_MovementActionsCallbackInterface != null)
            {
                crouch.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnCrouch;
                crouch.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnCrouch;
                crouch.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnCrouch;
                look.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnLook;
                look.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnLook;
                look.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnLook;
                move.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnMove;
                move.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnMove;
                move.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnMove;
                recentre.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnRecentre;
                recentre.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnRecentre;
                recentre.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnRecentre;
                sprint.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnSprint;
                sprint.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnSprint;
                sprint.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnSprint;
                strafe.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnStrafe;
                strafe.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnStrafe;
                strafe.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnStrafe;
                strafeToggle.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnStrafeToggle;
                strafeToggle.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnStrafeToggle;
                strafeToggle.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnStrafeToggle;
                jump.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnJump;
                jump.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnJump;
                jump.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnJump;
            }
            m_Wrapper.m_MovementActionsCallbackInterface = instance;
            if (instance != null)
            {
                crouch.started += instance.OnCrouch;
                crouch.performed += instance.OnCrouch;
                crouch.canceled += instance.OnCrouch;
                look.started += instance.OnLook;
                look.performed += instance.OnLook;
                look.canceled += instance.OnLook;
                move.started += instance.OnMove;
                move.performed += instance.OnMove;
                move.canceled += instance.OnMove;
                recentre.started += instance.OnRecentre;
                recentre.performed += instance.OnRecentre;
                recentre.canceled += instance.OnRecentre;
                sprint.started += instance.OnSprint;
                sprint.performed += instance.OnSprint;
                sprint.canceled += instance.OnSprint;
                strafe.started += instance.OnStrafe;
                strafe.performed += instance.OnStrafe;
                strafe.canceled += instance.OnStrafe;
                strafeToggle.started += instance.OnStrafeToggle;
                strafeToggle.performed += instance.OnStrafeToggle;
                strafeToggle.canceled += instance.OnStrafeToggle;
                jump.started += instance.OnJump;
                jump.performed += instance.OnJump;
                jump.canceled += instance.OnJump;
            }
        }
    }
    public MovementActions @Movement
    {
        get
        {
            return new MovementActions(this);
        }
    }
    public interface IMovementActions
    {
        void OnCrouch(InputAction.CallbackContext context);
        void OnLook(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnRecentre(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
        void OnStrafe(InputAction.CallbackContext context);
        void OnStrafeToggle(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
    }
}
