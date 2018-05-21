using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class DefaultThirdPersonCharacter : MonoBehaviour
{
    public Transform    cameraTransform;
    public float        maxForwardSpeed             = 10f;
    public bool         useAcceleration             = true;
    public float        groundAcceleration          = 20f;
    public float        groundDeceleration          = 15f;
    [Range (0f, 1f)]
    public float        airborneAccelProportion     = 0.5f;
    [Range (0f, 1f)] 
    public float        airborneDecelProportion     = 0.5f;
    public float        gravity                     = 10f;
    public float        jumpSpeed                   = 15f;
    public bool         interpolateTurning          = true;
    public float        turnSpeed                   = 500f;
    [Range (0f, 1f)] 
    public float        airborneTurnSpeedProportion = 0.5f;

    protected bool                  m_IsGrounded    = true;
    protected bool                  m_ReadyToJump;
    protected float                 m_ForwardSpeed;
    protected float                 m_VerticalSpeed;
    protected IControllerInput      m_Input;
    protected CharacterController   m_CharCtrl;
    protected Animator              m_Animator;

    const float k_GroundedRayDistance = 1f;

    readonly int m_HashForwardSpeedPara     = Animator.StringToHash ("ForwardSpeed");
    readonly int m_HashVerticalSpeedPara    = Animator.StringToHash ("VerticalSpeed");
    readonly int m_HashGroundedPara         = Animator.StringToHash ("Grounded");
    readonly int m_HashNormalisedTimePara   = Animator.StringToHash ("NormalisedTime");

    protected bool IsMoveInput
    {
        get { return !Mathf.Approximately (m_Input.MoveInput.sqrMagnitude, 0f); }
    }

    void Awake ()
    {
        m_Input = GetComponent<IControllerInput> ();
        m_Animator = GetComponent<Animator> ();
        m_CharCtrl = GetComponent<CharacterController> ();
    }
    
    void FixedUpdate ()
    {
        SetForward ();
        CalculateForwardMovement ();
        CalculateVerticalMovement ();
        SetNormalisedTime ();
    }

    void SetForward ()
    {
        if (!IsMoveInput)
            return;

        Vector3 flatForward = cameraTransform.forward;
        flatForward.y = 0f;
        flatForward.Normalize();

        Vector2 moveInput = m_Input.MoveInput;
        Vector3 localMovementDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            
        Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
        cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

        Quaternion targetRotation = Quaternion.LookRotation(cameraToInputOffset * flatForward);

        if (interpolateTurning)
        {
            float actualTurnSpeed = m_IsGrounded ? turnSpeed : turnSpeed * airborneTurnSpeedProportion;
            targetRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * Time.deltaTime);
        }

        transform.rotation = targetRotation;
    }
    
    void CalculateForwardMovement ()
    {
        Vector2 moveInput = m_Input.MoveInput;
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        float desiredSpeed = moveInput.magnitude * maxForwardSpeed;

        if (useAcceleration)
        {
            float acceleration = m_IsGrounded
                ? (IsMoveInput ? groundAcceleration : groundDeceleration)
                : (IsMoveInput ? groundAcceleration : groundDeceleration) * airborneDecelProportion;

            m_ForwardSpeed = Mathf.MoveTowards(m_ForwardSpeed, desiredSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            m_ForwardSpeed = desiredSpeed;
        }

        m_Animator.SetFloat(m_HashForwardSpeedPara, m_ForwardSpeed);
    }
    
    void CalculateVerticalMovement ()
    {
        if (!m_Input.JumpInput)
            m_ReadyToJump = true;

        if (m_IsGrounded)
        {
            m_VerticalSpeed = -gravity;

            if (m_Input.JumpInput && m_ReadyToJump)
            {
                m_VerticalSpeed = jumpSpeed;
                m_IsGrounded = false;
                m_ReadyToJump = false;
            }
        }
        else
        {
            if (Mathf.Approximately (m_VerticalSpeed, 0f))
            {
                m_VerticalSpeed = 0f;
            }
            m_VerticalSpeed -= gravity * Time.deltaTime;
        }

        m_Animator.SetFloat (m_HashVerticalSpeedPara, m_VerticalSpeed);
    }

    void SetNormalisedTime ()
    {
        float normalisedTime = m_Animator.GetCurrentAnimatorStateInfo (0).normalizedTime;
        m_Animator.SetFloat (m_HashNormalisedTimePara, normalisedTime);
    }

    void OnAnimatorMove ()
    {
        Vector3 movement;
        if (m_IsGrounded && m_Animator.deltaPosition.z >= groundAcceleration * Time.deltaTime)
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
            if (Physics.Raycast (ray, out hit, k_GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                movement = Vector3.ProjectOnPlane (m_Animator.deltaPosition, hit.normal);
            }
            else
            {
                movement = m_Animator.deltaPosition;
            }
        }
        else
        {
            movement = m_ForwardSpeed * transform.forward * Time.deltaTime;
        }

        movement += m_VerticalSpeed * Vector3.up * Time.deltaTime;

        m_CharCtrl.Move(movement);

        m_IsGrounded = m_CharCtrl.isGrounded;
        m_Animator.SetBool(m_HashGroundedPara, m_IsGrounded);
    }
}
