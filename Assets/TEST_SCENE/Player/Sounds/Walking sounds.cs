using Mirror;
using UnityEngine;

public class WalkingSounds : NetworkBehaviour
{
    public AudioSource audio_player;
    public AudioClip[] sounds_variants;

    private Rigidbody rb;
    private int currentSurfaceIndex = 0;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        if (audio_player == null) Debug.LogError("Аудио источник не назначен!");
        if (sounds_variants.Length < 3) Debug.LogError("Нужно минимум 3 звука шагов!");
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (audio_player == null || rb == null) return;

        Ray ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.2f))
        {
            if (hit.collider.CompareTag("concrete")) currentSurfaceIndex = 1;
            else if (hit.collider.CompareTag("wood")) currentSurfaceIndex = 2;
            else currentSurfaceIndex = 0;
        }

        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        bool isMoving = horizontalVelocity.sqrMagnitude > 0.1f;

        if (isMoving)
        {
            if (audio_player.clip != sounds_variants[currentSurfaceIndex])
                audio_player.clip = sounds_variants[currentSurfaceIndex];

            if (!audio_player.isPlaying)
                audio_player.Play();

            audio_player.pitch = horizontalVelocity.magnitude > 5.5f ? 3f : 1.2f;
        }
        else
        {
            if (audio_player.isPlaying)
                audio_player.Stop();
        }
    }
}