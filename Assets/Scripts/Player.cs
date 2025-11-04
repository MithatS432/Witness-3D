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
    public float wallRunDuration = 3f; // Süreyi uzattım
    public float wallCheckDistance = 0.6f;
    public LayerMask wallMask;
    private bool isWallRunning = false;
    private float wallRunTimer = 0f;
    private Vector3 wallNormal;
    private bool canWallRun = true;

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

        if (!isWallRunning)
        {
            HandleMovement();
            HandleJump();
            ApplyGravity();
        }

        HandleWallRun();
    }

    void HandleGroundCheck()
    {
        // Wall run sırasında ground check'i yapma!
        if (isWallRunning) return;

        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2 - 0.1f);
        isGrounded = Physics.CheckSphere(spherePosition, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);

        if (isGrounded)
        {
            canWallRun = true;
        }
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
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                AudioSource.PlayClipAtPoint(jumpSound, transform.position);
                verticalVelocity = jumpForce;
            }
            else if (isWallRunning)
            {
                // Wall run'dan zıplama
                Vector3 jumpDir = (wallNormal + Vector3.up * 1.5f).normalized;
                verticalVelocity = jumpForce * 1.2f;
                controller.Move(jumpDir * 5f * Time.deltaTime);
                StopWallRun();
            }
        }
    }

    void ApplyGravity()
    {
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else if (isWallRunning)
        {
            // Wall run sırasında yerçekimi yok
            verticalVelocity = 0f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    void HandleWallRun()
    {
        // Wall run devam ediyorsa
        if (isWallRunning)
        {
            wallRunTimer += Time.deltaTime;

            // Wall run hareketi
            Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up).normalized;
            if (Vector3.Dot(transform.forward, wallForward) < 0)
                wallForward = -wallForward;

            controller.Move(wallForward * wallRunSpeed * Time.deltaTime);

            // Yapışma efekti - biraz daha güçlü yapalım
            controller.Move(-wallNormal * 1f * Time.deltaTime);

            // Sonlandırma koşulları - SADECE W bırakıldığında veya süre dolduğunda
            bool wPressed = Input.GetKey(KeyCode.W);
            bool wallStillThere = IsNextToWall();

            if (wallRunTimer >= wallRunDuration || !wPressed || !wallStillThere)
            {
                StopWallRun();
            }

            return;
        }

        // Wall run başlatma koşulları
        if (canWallRun && !isGrounded && verticalVelocity < 0 && Input.GetKey(KeyCode.W))
        {
            if (CheckForWall(out Vector3 normal))
            {
                StartWallRun(normal);
            }
        }
    }

    bool CheckForWall(out Vector3 normal)
    {
        Debug.DrawRay(transform.position, transform.right * wallCheckDistance, Color.red);
        Debug.DrawRay(transform.position, -transform.right * wallCheckDistance, Color.red);

        RaycastHit hit;

        // Sağ ve sol tarafları kontrol et
        if (Physics.Raycast(transform.position, transform.right, out hit, wallCheckDistance, wallMask))
        {
            normal = hit.normal;
            Debug.Log("Sağ duvar bulundu: " + hit.collider.name);
            return true;
        }

        if (Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDistance, wallMask))
        {
            normal = hit.normal;
            Debug.Log("Sol duvar bulundu: " + hit.collider.name);
            return true;
        }

        normal = Vector3.zero;
        return false;
    }

    bool IsNextToWall()
    {
        return Physics.Raycast(transform.position, transform.right, wallCheckDistance, wallMask) ||
               Physics.Raycast(transform.position, -transform.right, wallCheckDistance, wallMask);
    }

    void StartWallRun(Vector3 normal)
    {
        Debug.Log("Wall Run Başladı!");

        isWallRunning = true;
        wallRunTimer = 0f;
        wallNormal = normal;
        verticalVelocity = 0f; // Düşüşü durdur

        // Kamerayı hafif yana yatır
        float tiltAngle = Vector3.Dot(transform.right, wallNormal) * 15f;
        cameraTransform.localEulerAngles = new Vector3(rotationY, 0f, tiltAngle);
    }

    void StopWallRun()
    {
        if (!isWallRunning) return;

        Debug.Log("Wall Run Bitti! Süre: " + wallRunTimer + "s");

        isWallRunning = false;
        canWallRun = false;

        // Kamerayı düzelt
        cameraTransform.localEulerAngles = new Vector3(rotationY, 0f, 0f);

        // 0.3 saniye sonra tekrar wall run yapabilir
        Invoke(nameof(ResetWallRun), 0.3f);
    }

    void ResetWallRun()
    {
        canWallRun = true;
    }

    // Debug için
    void OnDrawGizmos()
    {
        if (controller == null) return;

        // Ground check gizmos
        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2 - 0.1f);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(spherePosition, groundCheckDistance);

        // Wall check gizmos
        Gizmos.color = isWallRunning ? Color.yellow : Color.blue;
        Gizmos.DrawRay(transform.position, transform.right * wallCheckDistance);
        Gizmos.DrawRay(transform.position, -transform.right * wallCheckDistance);
    }
}