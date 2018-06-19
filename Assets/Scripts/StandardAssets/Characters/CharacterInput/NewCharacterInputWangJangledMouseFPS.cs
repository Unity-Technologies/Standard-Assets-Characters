using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
    public class NewCharacterInputWangJangledMouseFPS :MonoBehaviour, ICharacterInput
    {
	    public Vector2 mouseLook { get; private set; }

	    private Vector2 gamePadLook;
		
	    [SerializeField]
	    private NewInputActions controls;

	    private Vector2 look;

	    public Vector2 moveInput { get; private set; }

	    public bool hasMovementInput 
	    { 
		    get { return moveInput != Vector2.zero; }
	    }

	    public Action jumpPressed { get; set; }

	    public void OnEnable()
	    {
		    controls.Enable();
		    controls.gameplay.movement.performed += Move;
		    controls.gameplay.look.performed += MouseLook;
		    controls.gameplay.jump.performed += Jump;
		    controls.gameplay.gamePadLook.performed += GamepadLook;

		    CinemachineCore.GetInputAxis = LookInputOverride;
	    }

	    public void OnDisable()
	    {
		    controls.Disable();
		    controls.gameplay.movement.performed -= Move;
		    controls.gameplay.look.performed -= MouseLook;
		    controls.gameplay.gamePadLook.performed -= GamepadLook;
	    }

	    private void Move(InputAction.CallbackContext ctx)
	    {
		    moveInput = ctx.ReadValue<Vector2>();
	    }

	   
	    private void MouseLook(InputAction.CallbackContext ctx)
	    {
		    mouseLook = ctx.ReadValue<Vector2>();
		    Debug.Log(mouseLook.ToString());
	    }
	    
	    private void GamepadLook(InputAction.CallbackContext ctx)
	    {
		    gamePadLook = ctx.ReadValue<Vector2>();
	    }


	    /// <summary>
	    /// Sets the Cinemachine cam POV to mouse inputs.
	    /// </summary>
	    private float LookInputOverride(string axis)
	    {
		    if (axis == "Mouse X")
		    {
			    return gamePadLook.x;
		    }

		    if (axis == "Mouse Y")
		    {
			    return gamePadLook.y;
		    }

		    return 0;
	    }

	    private void Jump(InputAction.CallbackContext ctx)
	    {
		    if (jumpPressed != null)
		    {
			    jumpPressed();
		    }	
	    }
		
    }
}