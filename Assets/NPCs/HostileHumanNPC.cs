using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCChase : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;
    public float stopDistance = 1.5f;
    public float chaseDistance = 10f;
    private bool playerFound = false;
    private Animator animator;
    private Vector3 lastPosition;
    private bool isChasing = false;
    public int damageAmount = 10;

    public AudioClip hitSound;

    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>(); 
        lastPosition = transform.position;

        // Add an AudioSource component if not already present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerFound)
        {
            GameObject playerObj = GameObject.Find("PLAYER");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerFound = true;
            }
        }

        if (playerFound)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= chaseDistance)
            {
                isChasing = true;
            }

            else if (distance > chaseDistance + stopDistance)
            {
                isChasing = false; // Stop chasing when far enough away
            }

            if (isChasing)
            {
                if (distance > stopDistance)
                {
                    Vector3 direction = (player.position - transform.position).normalized;

                    // Ensure the NPC stays vertical by zeroing out the Y component of the direction for rotation
                    Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

                    // Calculate new position
                    Vector3 newPosition = transform.position + direction * speed * Time.deltaTime;

                    // Perform a raycast downward to adjust for terrain height
                    if (Physics.Raycast(new Vector3(newPosition.x, newPosition.y + 1f, newPosition.z), Vector3.down, out RaycastHit hit, Mathf.Infinity))
                    {
                        newPosition.y = hit.point.y; // Adjust the height to match the terrain
                    }

                    // Apply the adjusted position
                    transform.position = newPosition;

                       // Rotate to face the player, staying upright
                    if (flatDirection.sqrMagnitude > 0.01f) // Avoid LookRotation errors on near-zero vectors
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
                    }
                }
            }

            
            float actualSpeed = Vector3.Distance(lastPosition, transform.position) / Time.deltaTime;

            if (animator != null)
            {
                animator.SetFloat("speed", actualSpeed);
            }
            lastPosition = transform.position;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        // Check if the NPC collides with the player
        if (other.CompareTag("Player"))
        {
            // Play the hit sound
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }
    }
}