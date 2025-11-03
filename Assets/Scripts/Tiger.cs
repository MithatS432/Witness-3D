using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class Tiger : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Settings")]
    public float speed = 4f;
    public float rotationSpeed = 8f;
    public float catchDistance = 1.5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;

    public AudioClip roarSoundEffect;
    private float time = 0f;
    private float roarSoundTime = 10f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (player == null) return;

        // Yere temas kontrolü
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // küçük bir değer yere sabitlemek için
        }

        // Oyuncuya yönelme
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction.magnitude >= 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        // Yatay hareket (sadece yere basarken)
        if (controller.isGrounded)
        {
            Vector3 move = direction * speed;
            controller.Move(move * Time.deltaTime);
        }

        // Yerçekimi uygulama
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Oyuncuya ulaşma
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= catchDistance)
        {
            EndGame();
        }

        time += Time.deltaTime;
        if (time >= roarSoundTime)
        {
            AudioSource.PlayClipAtPoint(roarSoundEffect, transform.position, 1f);
            time = 0f;
        }
    }

    void EndGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
