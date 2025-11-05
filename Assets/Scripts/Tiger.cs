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
    // 🔥 DÜZELTME 1: Yakalama mesafesini artırın veya ölçeğe göre ayarlayın.
    // Başlangıçta 1.5f idi, 3.0f'e çıkardık. (Inspector'da bu değeri daha da artırabilirsiniz!)
    public float catchDistance = 3.0f; 
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    
    private float effectiveCatchDistance; 

    public AudioClip roarSoundEffect;
    private float time = 0f;
    public float roarSoundTime = 10f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // 🔥 DÜZELTME 2: Etkili yakalama mesafesini, Tiger ve Player'ın yarıçaplarını toplayarak hesaplayın.
        if (player != null && player.GetComponent<CharacterController>() != null)
        {
            // Tiger'ın yarıçapı + Oyuncunun yarıçapı + Ekstra mesafe (catchDistance)
            CharacterController playerController = player.GetComponent<CharacterController>();
            effectiveCatchDistance = controller.radius + playerController.radius + catchDistance;
        }
        else
        {
             // Eğer oyuncunun Controller'ı yoksa, sadece catchDistance kullanılır.
            effectiveCatchDistance = catchDistance;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Yere temas kontrolü
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        // Oyuncuya yönelme
        Vector3 direction = (player.position - transform.position);
        
        // Takip sırasında dikey mesafeyi (y) göz ardı et (duvardayken garip dönmeleri engeller)
        Vector3 flatDirection = new Vector3(direction.x, 0, direction.z).normalized; 

        if (flatDirection.magnitude >= 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(flatDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        // Yatay hareket
        if (controller.isGrounded)
        {
            Vector3 move = flatDirection * speed;
            controller.Move(move * Time.deltaTime);
        }

        // Yerçekimi uygulama
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 🔥 DÜZELTME 3: Etkili mesafeyi kontrol et.
        float currentDistance = direction.magnitude;
        if (currentDistance <= effectiveCatchDistance)
        {
            EndGame();
        }

        // Ses Efekti Zamanlayıcı
        time += Time.deltaTime;
        if (time >= roarSoundTime)
        {
            if (roarSoundEffect != null)
            {
                AudioSource.PlayClipAtPoint(roarSoundEffect, transform.position, 1f);
            }
            time = 0f;
        }
    }

    void EndGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}