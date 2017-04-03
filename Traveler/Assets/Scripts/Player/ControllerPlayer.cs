using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*
    Author: Ludwig Gustavsson, Ludvig Grönborg
    Last Edited: 2017/04/03
*/
[RequireComponent(typeof(ControllerClimbing))]
public class ControllerPlayer : MonoBehaviour {

    public enum MovementState
    {
        Idle,
        Moving,
        Jumping,
        Falling,
        Running,
        Climbing,
        Sliding
    }

    [Header("Current player state (read only)")]
    [SerializeField]
    private MovementState m_currentState = MovementState.Idle;

    [Header("Grounded Movement")]
    [Range(5,20)]
    [SerializeField]
    private float m_topSpeed;
    [SerializeField]
    private float m_accelerationspeed;
    [SerializeField]
    private float m_deccelerationspeed;

    [Header("Other Movement")]
    [Range(600, 900)]
    [SerializeField]
    private float m_jumpForce = 750;
    [Range(0, 0.1f)]
    [SerializeField]
    private float m_aircontrolForce = 0.1f;
    [SerializeField]
    private float m_walkableMaxAngle = 45.0f;

    [Header("Animation")]
    [SerializeField]
    private float m_turningSpeed = 5.0f;
    [SerializeField]
    private float m_animationFactor = 0.25f;

    [Header("Other settings")]
    [SerializeField]
    private LayerMask m_StandableLayers;
    [SerializeField]
    private float m_maxTiltX = 10.0f;
    [SerializeField]
    private float m_maxTiltZ = 5.0f;

    private float m_currentAcceleration = 0;
    private float m_accelerationTilt = 0;
    private Vector3 m_currentVelocity;
    private Vector3 m_slopeVelocity;

    private bool m_isGrounded;
    private Vector3 m_lastInputDirection;


    // Components
    private Rigidbody m_rigidbody;
    private CapsuleCollider m_collider;
    private Transform m_model;
    private InputManager m_inputManager;
    private Animator m_animator;
    private ControllerClimbing m_controllerClimbing;

    // Variables defined here only for caching purposes
    private RaycastHit m_rayHit;

    private const float m_slopeRayDistance = 1.5f;


    void Start () {
        m_rigidbody = GetComponent<Rigidbody>();
        m_collider = GetComponent<CapsuleCollider>();
        m_model = transform.FindChild("Model").transform;
        m_inputManager = GameObject.FindObjectOfType<InputManager>();
        m_animator = m_model.GetComponent<Animator>();
        m_controllerClimbing = GetComponent<ControllerClimbing>();
        CurrentMovementState = m_currentState;
    }
	
	void Update ()
    {
        DetermineState();
        UpdateState();
    }

    // Finds out what state the player is currently in
    private void DetermineState()
    {
        if (!CurrentMovementState.Equals(MovementState.Climbing))
        {
            if (IsGrounded())
            {
                if (!IsSliding())
                {
                    if (m_inputManager.MovingCharacter())
                        CurrentMovementState = MovementState.Moving;
                    else
                    {
                        CurrentMovementState = MovementState.Idle;
                    }
                }
                else
                {
                    CurrentMovementState = MovementState.Sliding;
                }

                m_controllerClimbing.State = ControllerClimbing.ClimbingState.NotClimbing;
                m_isGrounded = true;
                UsingGravity = false;
            }
            else
            {
                m_isGrounded = false;
                UsingGravity = true;
                if (m_rigidbody.velocity.y > 0)
                    CurrentMovementState = MovementState.Jumping;
                else
                    CurrentMovementState = MovementState.Falling;
            }
        }
    }

    private void UpdateState()
    {
        switch (m_currentState)
        {
            case MovementState.Idle:
                UpdateIdle();
                UpdateJump();
                UpdateModel();
                break;
            case MovementState.Moving:
                UpdateMoving();
                UpdateJump();
                UpdateModel();
                break;
            case MovementState.Jumping:
                UpdateAirborne();
                m_controllerClimbing.LookForLedges();
                UpdateModel();
                break;
            case MovementState.Falling:
                UpdateAirborne();
                m_controllerClimbing.LookForLedges();
                UpdateModel();
                break;
            case MovementState.Running:
                break;
            case MovementState.Climbing:
                m_controllerClimbing.LookForLedges();
                break;
            case MovementState.Sliding:
                break;
            default:
                break;
        }
    }

    void UpdateJump()
    {
        if (m_isGrounded && m_inputManager.Jumping())
        {
            m_rigidbody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
        }
    }

    private void UpdateIdle()
    {
        m_currentAcceleration = 0;
        m_rigidbody.velocity = Vector3.zero;
    }

    private void UpdateMoving()
    {
        // Determine player forward direction
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);

        // Make sure player sticks to speed limit
        m_currentAcceleration = Mathf.Clamp(m_currentAcceleration, 0, m_topSpeed);

        // Set velocity
        m_currentVelocity = rot * new Vector3(
            Input.GetAxisRaw("Horizontal"), // Left / Right
            0,                              // Jump / Fall
            Input.GetAxisRaw("Vertical")    // Forward / Back
            ).normalized * m_currentAcceleration;

        m_currentVelocity.y = m_rigidbody.velocity.y;
        m_rigidbody.velocity = m_currentVelocity;
        //print(m_rigidbody.velocity.magnitude);

        // Debug velocity vector
        Vector3 origin = transform.position + (Vector3.up * m_collider.bounds.size.y / 2) + (m_rigidbody.velocity.normalized * m_collider.bounds.size.x / 2);
        Debug.DrawLine(origin, origin + m_rigidbody.velocity.normalized * 0.2f, Color.red);

        // Accelerate player
        m_currentAcceleration += m_accelerationspeed * Time.deltaTime;

        m_lastInputDirection = m_rigidbody.velocity;
    }

    private void UpdateAirborne()
    {
        if (m_inputManager.MovingCharacter())
        {
            // Determine player forward direction
            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0;
            Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);

            // Set velocity
            Vector3 aircontrol = (rot * new Vector3(
                Input.GetAxisRaw("Horizontal"), // Left / Right
                0,                              // Jump / Fall
                Input.GetAxisRaw("Vertical")    // Forward / Back
                ).normalized * m_aircontrolForce);

            m_rigidbody.velocity += aircontrol;

            m_lastInputDirection = m_rigidbody.velocity;
        }
    }

    private void UpdateModel()
    {
        //m_model.rotation = new Quaternion(m_model.rotation.x, velocityRot.y, m_model.rotation.z, velocityRot.w);
        //float tiltX = Mathf.Lerp(m_model.rotation.x, Mathf.Clamp(m_accelerationspeed, -m_maxTilt, m_maxTilt), Time.deltaTime * m_rigidbody.velocity.magnitude *10);
        //float tiltZ = Mathf.Lerp(m_model.rotation.z, Mathf.Clamp(m_accelerationspeed, -m_maxTilt, m_maxTilt), Time.deltaTime * m_rigidbody.velocity.magnitude * 10);

        RotateTowardsVelocity();

        float tiltX = Input.GetAxis("Vertical") * m_maxTiltX;
        float tiltZ = Input.GetAxis("Horizontal") * m_maxTiltZ;
        //m_model.rotation = new Quaternion(tiltX, velocityRot.y, Mathf.Abs(velocityRot.y), velocityRot.w);
        //m_model.rotation = Quaternion.Euler(Mathf.Abs(tiltX),  velocityRot.eulerAngles.y, -0);

        if (CurrentMovementState == MovementState.Moving)
            m_animator.SetFloat("Run Speed", m_rigidbody.velocity.magnitude * m_animationFactor);
        else
            m_animator.SetFloat("Run Speed", 0);
    }

    private void RotateTowardsVelocity()
    {
        // Rotate model in latest direction provided by player
        Quaternion inputDir = Quaternion.LookRotation(m_lastInputDirection.normalized);
        Quaternion velocityRot = Quaternion.Lerp(m_model.rotation, inputDir, Time.deltaTime * m_turningSpeed);

        // Update y rotation
        m_model.rotation = Quaternion.Euler(m_model.rotation.eulerAngles.x, velocityRot.eulerAngles.y, m_model.rotation.eulerAngles.z);
    }

    public void ActivateClimbing()
    {
        RigidbodyKinematic = true;
        UsingGravity = false;
        CurrentMovementState = MovementState.Climbing;
    }

    public void DeactivateClimbing()
    {
        RigidbodyKinematic = false;
        UsingGravity = true;
        CurrentMovementState = MovementState.Falling;
    }

    private bool IsGrounded()
    {
        float offsetCenter = 0.3f;
        float offsetHeight = 0.5f;
        float distance = 0.6f;

        Vector3 tempV = Vector3.zero;
        Vector3 v = new Vector3(transform.position.x, m_collider.bounds.min.y + offsetHeight, transform.position.z);

        for (int i = 0; i < 4; i++)
        {
            // Define ray
            switch (i)
            {
                case (0):
                    tempV = new Vector3(transform.forward.x * offsetCenter, 0, transform.forward.z * offsetCenter);
                    break;
                case (1):
                    tempV = -tempV;
                    break;
                case (2):
                    tempV = new Vector3(transform.right.x * offsetCenter, 0, transform.right.z * offsetCenter);
                    break;
                case (3):
                    tempV = -tempV;
                    break;
                default:
                    break;
            }
            // Shoot ray
            if (Physics.Raycast(tempV + v, Vector3.down, out m_rayHit, distance, m_StandableLayers))
            {
                if (!CurrentMovementState.Equals(MovementState.Sliding))
                    m_rigidbody.MovePosition(new Vector3(m_rigidbody.position.x, m_rayHit.point.y + m_collider.bounds.size.y / 2, m_rigidbody.position.z));
                return true;
            }
        }
        return false;
    }

    private bool IsSliding()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out m_rayHit, m_slopeRayDistance))
        {
            //Set slopevelocity to be relative to hit normal
            m_slopeVelocity = new Vector3(m_rayHit.normal.x, -m_rayHit.normal.y, m_rayHit.normal.z);
            Debug.DrawRay(transform.position, m_slopeVelocity);

            //Check if current angle between upwards vector and normal is equal to or greater than given value
            float a = Vector3.Angle(Vector3.up, m_rayHit.normal);
            if (a >= m_walkableMaxAngle)
                return true;
        }
        return false;
    }

    // Get & Set
    // PUBLIC
    public MovementState CurrentMovementState { get { return m_currentState; } set { m_currentState = value; } }

    // PRIVATE
    private bool UsingGravity { get { return m_rigidbody.useGravity; } set { m_rigidbody.useGravity = value; } }
    private bool RigidbodyKinematic { get { return m_rigidbody.isKinematic; } set { m_rigidbody.isKinematic = value; } }
}