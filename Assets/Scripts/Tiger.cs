using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class Tiger : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Settings")]
    public float speed = 4f;
    public float rotationSpeed = 8f;
    public float catchDistance = 1.5f;

    private CharacterController controller;
    private Animator anim;

    public AudioClip roarSoundEffect;
    private float time = 0f;
    float roarSoundTime = 10f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

    }

    void Update()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

        controller.Move(direction * speed * Time.deltaTime);

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= catchDistance)
        {
            EndGame();
        }
        time += Time.deltaTime;
        if (time >= roarSoundTime)
        {
            AudioSource.PlayClipAtPoint(roarSoundEffect, transform.position);
            time = 0f;
        }
    }

    void EndGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
