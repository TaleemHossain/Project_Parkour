using UnityEngine;

public class CMmovement : MonoBehaviour
{
    [Header("References")]
    Animator animator;
    private CharacterController characterController;
    [SerializeField] private Transform cameraTransform;
    [Header("Ground Check settings")]
    public bool isGrounded;
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 10f;

    [Header("Input")]
    private float moveInput;
    private float turnInput;
    private Vector3 desiredMove = Vector3.zero;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        GroundCheck();
        ReadInput();
        ComputeDesiredMove();
        GroundMovement();
        Turn();
    }
    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
        animator.SetBool("isGrounded", isGrounded);
    }

    private void ReadInput()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }

    private void ComputeDesiredMove()
    {
        Vector3 inputVec = new Vector3(turnInput, 0f, moveInput).normalized;

        desiredMove = cameraTransform.TransformDirection(inputVec).normalized;
        desiredMove.y = 0f;
    }

    private void GroundMovement()
    {
        Vector3 move = desiredMove.normalized;
        move *= moveSpeed;
        characterController.Move(move * Time.deltaTime);
        animator.SetFloat("moveAmount", move.sqrMagnitude, 0.1f, Time.deltaTime);
        animator.SetFloat("crouchMoveAmount", 0f);
    }

    private void Turn()
    {
        Vector3 forwardOnlyInput = new Vector3(turnInput, 0f, Mathf.Max(0f, moveInput)).normalized;
        Vector3 forwardMove = cameraTransform.TransformDirection(forwardOnlyInput).normalized;
        forwardMove.y = 0f;

        if (forwardMove.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forwardMove.normalized).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime).normalized;
        }
    }
}
