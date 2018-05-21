using UnityEngine;
using System;

public class DefaultControllerInput : MonoBehaviour, IControllerInput
{
    protected Vector2 m_Movement;
    protected Vector2 m_Camera;
    protected bool m_Jump;
    protected bool m_Sprint;


    public Vector2 MoveInput
    {
        get { return m_Movement; }
    }


    public Vector2 CameraInput
    {
        get { return m_Camera; }
    }


    public bool JumpInput
    {
        get { return m_Jump; }
    }


    public bool SprintInput
    {
        get { return m_Sprint; }
    }


    void Update()
    {
        // We prefer caching inputs instead of polling in the Get* function, as it will mean getting the input only once.
        m_Movement.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        m_Camera.Set(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        m_Jump = Input.GetButton("Jump");
        m_Sprint = Input.GetKey (KeyCode.LeftShift);
    }
}
