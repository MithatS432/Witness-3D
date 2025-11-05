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
    private bool isGrounded;
    public AudioClip jumpSound;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    private float rotationY = 0f;

    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Physics Settings")]
    public float gravity = -25f;

    [Header("Wall Run Settings")]
    public float wallRunSpeed = 8f;
    public float wallRunDuration = 1.5f;
    public float wallCheckDistance = 0.6f;
    public LayerMask wallMask;
    private bool isWallRunning = false;
    private float wallRunTimer = 0f;
    private Vector3 wallNormal;
    private bool canWallRun = true;
    public float wallStickForce = 8f;
    public float wallRunMaxFallSpeed = -3f;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMouseLook();
        HandleMovementAndGravity();
        HandleJump();
        HandleWallRun();
    }

    void HandleGroundCheck()
    {
        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2 - 0.1f);
        isGrounded = Physics.CheckSphere(spherePosition, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);

        if (isGrounded) canWallRun = true;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        rotationY -= mouseY;
        rotationY = Mathf.Clamp(rotationY, -60f, 60f);

        float targetTilt = isWallRunning ? Mathf.Clamp(Vector3.Dot(transform.right, wallNormal), -1f, 1f) * 15f : 0f;
        float currentTilt = cameraTransform.localEulerAngles.z;
        if (currentTilt > 180) currentTilt -= 360;
        float newTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * 5f);

        cameraTransform.localRotation = Quaternion.Euler(rotationY, 0f, newTilt);
    }

    void HandleMovementAndGravity()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        Vector3 totalMove = move * currentSpeed;

        if (isWallRunning)
        {
            wallRunTimer += Time.deltaTime;
            Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up).normalized;
            if (Vector3.Dot(transform.forward, wallForward) < 0)
                wallForward = -wallForward;

            // Yerçekimini azalt
            verticalVelocity += gravity * 0.2f * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, wallRunMaxFallSpeed);

            Vector3 stickToWall = -wallNormal * wallStickForce;
            totalMove = wallForward * wallRunSpeed + stickToWall + Vector3.up * verticalVelocity;

            // Wall Run sonlanma koşulları
            bool wPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
            if (wallRunTimer >= wallRunDuration || !wPressed || !IsNextToWall())
            {
                StopWallRun();
            }
        }
        else
        {
            // Normal yerçekimi
            if (isGrounded && verticalVelocity < 0)
                verticalVelocity = -2f;
            else
                verticalVelocity += gravity * Time.deltaTime;

            totalMove += Vector3.up * verticalVelocity;
        }

        controller.Move(totalMove * Time.deltaTime);
    }

    void HandleJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        if (isGrounded)
        {
            AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            verticalVelocity = jumpForce;
        }
        else if (isWallRunning)
        {
            Vector3 jumpDir = (wallNormal * 1.5f + Vector3.up).normalized;
            verticalVelocity = jumpForce * 1.1f;
            controller.Move(jumpDir * jumpForce * 0.5f * Time.deltaTime);
            StopWallRun();
        }
    }

    void HandleWallRun()
    {
        if (isWallRunning) return;

        if (canWallRun && !isGrounded && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            if (CheckForWall(out Vector3 normal))
            {
                StartWallRun(normal);
            }
        }
    }

    bool CheckForWall(out Vector3 normal)
    {
        Vector3 origin = transform.position + Vector3.up * (controller.height / 2f);
        RaycastHit hit;

        if (Physics.Raycast(origin, transform.right, out hit, wallCheckDistance, wallMask))
        {
            normal = hit.normal;
            return true;
        }
        if (Physics.Raycast(origin, -transform.right, out hit, wallCheckDistance, wallMask))
        {
            normal = hit.normal;
            return true;
        }

        normal = Vector3.zero;
        return false;
    }

    bool IsNextToWall()
    {
        Vector3 origin = transform.position + Vector3.up * (controller.height / 2f);
        return Physics.Raycast(origin, transform.right, wallCheckDistance, wallMask) ||
               Physics.Raycast(origin, -transform.right, wallCheckDistance, wallMask);
    }

    void StartWallRun(Vector3 normal)
    {
        isWallRunning = true;
        wallRunTimer = 0f;
        wallNormal = normal;
        verticalVelocity = 0f;
        Debug.Log("Wall Run Başladı");
    }

    void StopWallRun()
    {
        if (!isWallRunning) return;

        isWallRunning = false;
        canWallRun = false;
        Debug.Log("Wall Run Bitti");
        Invoke(nameof(ResetWallRun), 0.3f);
    }

    void ResetWallRun()
    {
        canWallRun = true;
    }

    void OnDrawGizmos()
    {
        if (controller == null) return;

        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2 - 0.1f);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(spherePosition, groundCheckDistance);

        Gizmos.color = isWallRunning ? Color.yellow : Color.blue;
        Gizmos.DrawRay(transform.position, transform.right * wallCheckDistance);
        Gizmos.DrawRay(transform.position, -transform.right * wallCheckDistance);

        if (isWallRunning)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, wallNormal);
        }
    }
}
