using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody rb;
    private Animator anim;

    [Header("Player Settings")]
    public float speed = 5f;
    public float jumpForce = 7f;
    public bool isGround;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    private float rotationY = 0f;

    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Physics Settings")]
    public float gravityMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        rb.MovePosition(transform.position + move * speed * Time.fixedDeltaTime);
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        rotationY -= mouseY;
        rotationY = Mathf.Clamp(rotationY, -60f, 60f);
        cameraTransform.localRotation = Quaternion.Euler(rotationY, 0f, 0f);

        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }

        ApplyBetterJumpPhysics();

        speed = Input.GetKey(KeyCode.LeftShift) ? 10f : 5f;

        //anim.SetFloat("Speed", new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGround = false;
    }

    void ApplyBetterJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (gravityMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
            isGround = true;
    }
}
