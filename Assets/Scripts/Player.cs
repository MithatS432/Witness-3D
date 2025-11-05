using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


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
    private float rotationY = 0f; // <-- Artık HandleMouseLook'da kullanılacak

    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Physics Settings")]
    public float gravity = -25f;

    [Header("Wall Run Settings")]
    public float wallRunSpeed = 12f;
    public float wallRunDuration = 9999f;
    public float wallCheckDistance = 0.7f;
    public LayerMask wallMask;
    private bool isWallRunning = false;
    private float wallRunTimer = 0f; // <-- Artık Update içinde kullanılacak
    private Vector3 wallNormal;
    private bool canWallRun = true; // <-- Artık StartWallRun/StopWallRun'da kullanılacak
    public float wallStickForce = 15f;
    public float wallRunMaxFallSpeed = 0f;
    public float wallRunUpwardForce = 2f;

    private float scaledGroundCheck;
    private float scaledWallCheck;

    // Duvar Zıplaması için Yatay İtme
    private Vector3 wallJumpHorizontalVelocity = Vector3.zero;
    public float wallJumpHorizontalDamp = 0.1f;

    [Header("Menu Settings")]
    public Button continueButton;
    public Button quitButton;
    private bool isMenuOpen = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        scaledGroundCheck = groundCheckDistance * transform.localScale.y;
        scaledWallCheck = wallCheckDistance * transform.localScale.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        continueButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);

        continueButton.onClick.AddListener(() =>
        {
            isMenuOpen = false;
            continueButton.gameObject.SetActive(false);
            quitButton.gameObject.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        });
    }


    void Update()
    {
        scaledGroundCheck = groundCheckDistance * transform.localScale.y;
        scaledWallCheck = wallCheckDistance * transform.localScale.x;

        HandleGroundCheck();
        HandleMouseLook();
        HandleWallRunStart();
        HandleMovementAndGravity();
        HandleJump();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isMenuOpen = !isMenuOpen;
            continueButton.gameObject.SetActive(isMenuOpen);
            quitButton.gameObject.SetActive(isMenuOpen);

            if (isMenuOpen)
            {
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

    }

    void HandleGroundCheck()
    {
        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2f - 0.05f);

        isGrounded = Physics.CheckSphere(spherePosition, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);


        if (isGrounded)
        {
            canWallRun = true;
            wallJumpHorizontalVelocity = Vector3.zero;
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        rotationY -= mouseY; // <-- rotationY artık kullanılıyor
        rotationY = Mathf.Clamp(rotationY, -60f, 60f);

        // Kamera eğimi (Roll)
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

            // 🔥 DÜZELTME 1: Dikey Hızı SIFIRDA sabitle! Düşme veya kayma yok.
            verticalVelocity = 0f;

            // İleri Hareket
            Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, wallNormal).normalized;
            Vector3 wallRunMove = projectedForward * wallRunSpeed;

            // Duvara Yapışma Kuvveti
            Vector3 stickToWall = -wallNormal * wallStickForce;

            // Yapışma kuvveti ve ileri hareketi birleştir. Dikey hız sıfır.
            totalMove = wallRunMove + stickToWall;

            // DÜZELTME 2: Wall Run bitiş koşullarını daha kesin yaptık.
            float verticalInput = Input.GetAxis("Vertical");
            // Sadece W bırakılırsa VEYA duvardan uzaklaşılırsa bitir.
            if (verticalInput < 0.1f || !IsNextToWall(wallNormal))
            {
                StopWallRun();
            }
        }
        else // Normal Hareket ve Yerçekimi
        {
            // ... (Normal hareket ve yerçekimi mantığı aynı kalacak)
            if (isGrounded && verticalVelocity < 0)
                verticalVelocity = -2f;
            else
                verticalVelocity += gravity * Time.deltaTime;

            totalMove += Vector3.up * verticalVelocity;

            if (wallJumpHorizontalVelocity.magnitude > 0.1f)
            {
                totalMove += wallJumpHorizontalVelocity;
                wallJumpHorizontalVelocity = Vector3.Lerp(wallJumpHorizontalVelocity, Vector3.zero, wallJumpHorizontalDamp);
            }
            else
            {
                wallJumpHorizontalVelocity = Vector3.zero;
            }
        }

        controller.Move(totalMove * Time.deltaTime);
    }

    void HandleJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        if (isGrounded)
        {
            if (jumpSound) AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            verticalVelocity = jumpForce;
        }
        else if (isWallRunning)
        {
            Vector3 jumpDir = (wallNormal * 1.5f + Vector3.up).normalized;
            verticalVelocity = jumpForce * 1.1f;

            wallJumpHorizontalVelocity = jumpDir * jumpForce * 0.6f;

            StopWallRun();
        }
    }

    void HandleWallRunStart()
    {
        if (isWallRunning) return;

        // 🔥 DÜZELTME: canWallRun kontrolü geri eklendi!
        // Yerde değiliz VE Wall Run hakkımız var VE W tuşuna basıyoruz
        if (canWallRun && !isGrounded && Input.GetKey(KeyCode.W))
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

        if (Physics.Raycast(origin, transform.right, out hit, scaledWallCheck, wallMask))
        {
            normal = hit.normal;
            return true;
        }
        if (Physics.Raycast(origin, -transform.right, out hit, scaledWallCheck, wallMask))
        {
            normal = hit.normal;
            return true;
        }

        normal = Vector3.zero;
        return false;
    }

    bool IsNextToWall(Vector3 currentWallNormal)
    {
        Vector3 origin = transform.position + Vector3.up * (controller.height / 2f);
        return Physics.Raycast(origin, -currentWallNormal, scaledWallCheck + 0.1f, wallMask);
    }

    void StartWallRun(Vector3 normal)
    {
        isWallRunning = true;
        wallRunTimer = 0f;
        wallNormal = normal;

        verticalVelocity = 0f;

        controller.Move(-wallNormal * 0.2f);

        // canWallRun burada true olarak kalmalı veya hiç kullanılmamalı, 
        // ancak StopWallRun'daki mantık için geri ekliyorum.
        // canWallRun = true;

        Debug.Log("Wall Run Başladı");
    }

    void StopWallRun()
    {
        if (!isWallRunning) return;

        isWallRunning = false;
        canWallRun = false; // <-- canWallRun artık kullanılıyor

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

        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2f - groundCheckDistance);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(spherePosition, groundCheckDistance);

        Vector3 origin = transform.position + Vector3.up * (controller.height / 2f);
        Gizmos.color = isWallRunning ? Color.yellow : Color.blue;
        Gizmos.DrawRay(origin, transform.right * scaledWallCheck);
        Gizmos.DrawRay(origin, -transform.right * scaledWallCheck);

        if (isWallRunning)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(origin, wallNormal * 1.5f);
        }
    }
}