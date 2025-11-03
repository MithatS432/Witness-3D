using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    private Animator anim;
    private CharacterController controller;

    [Header("Player Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpForce = 12f;

    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;

    private float verticalVelocity;
    public AudioClip jumpSound;

    [Header("Footstep Settings")]
    public AudioClip[] footstepSounds;
    public float walkStepRate = 0.5f;
    public float runStepRate = 0.3f;
    private float footstepTimer = 0f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    private float rotationY = 0f;

    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Physics Settings")]
    public float gravity = -25f;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        rotationY -= mouseY;
        rotationY = Mathf.Clamp(rotationY, -60f, 60f);
        cameraTransform.localRotation = Quaternion.Euler(rotationY, 0f, 0f);
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        HandleFootsteps(move.magnitude, isRunning);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            verticalVelocity = jumpForce;
        }
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    void HandleFootsteps(float moveAmount, bool isRunning)
    {
        if (!controller.isGrounded || moveAmount == 0)
        {
            footstepTimer = 0f;
            return;
        }

        float stepRate = isRunning ? runStepRate : walkStepRate;
        footstepTimer += Time.deltaTime;

        if (footstepTimer >= stepRate)
        {
            footstepTimer = 0f;
            PlayFootstepSound();
        }
    }

    void PlayFootstepSound()
    {
        if (footstepSounds.Length == 0) return;

        int index = Random.Range(0, footstepSounds.Length);
        AudioSource.PlayClipAtPoint(footstepSounds[index], transform.position);
    }
}
