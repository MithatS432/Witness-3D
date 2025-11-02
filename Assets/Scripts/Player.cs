using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    private Animator anim;
    // YENİ: CharacterController'ı buraya ekledik
    private CharacterController controller;

    [Header("Player Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    // Zıplama hissini iyileştirmek için varsayılan değeri artırdık
    public float jumpForce = 12f;

    // Artık kullanılmıyor ama kalsın (GroundCheck kaldırıldı)
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;

    private float verticalVelocity;
    // Artık kullanılmıyor (controller.isGrounded kullanılıyor)
    // private bool isGrounded;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    private float rotationY = 0f;

    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Physics Settings")]
    // Hızlı düşüş için varsayılan değeri artırdık
    public float gravity = -25f;

    void Start()
    {
        anim = GetComponent<Animator>();
        // CharacterController referansını al
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
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Yatay hareket vektörünü oluştur
        Vector3 move = transform.right * x + transform.forward * z;

        // CharacterController.Move() ile yatay hareketi uygula.
        // Bu, çarpışma kontrolü yapıldığı için platformlardan geçmeyi engeller.
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void HandleJump()
    {
        // CharacterController.isGrounded özelliğini kullan
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
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

}