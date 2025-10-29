using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody rb;
    private Animator anim;

    [Header("Player Settings")]
    public float speed = 5f;
    public float jumpForce = 5f;
    public bool isGround;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    private float rotationY = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

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
        Camera.main.transform.localRotation = Quaternion.Euler(rotationY, 0f, 0f);

        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGround = false;
        }
        if (Input.GetKey(KeyCode.LeftShift))
            speed = 20f;
        else
            speed = 10f;

    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
            isGround = true;
    }
}
