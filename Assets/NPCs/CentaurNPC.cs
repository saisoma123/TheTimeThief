using UnityEngine;

public class CentaurNPC : MonoBehaviour
{
    public Transform player; // Reference to the player
    public GameObject spearPrefab; // Spear prefab to instantiate
    public float throwForce = 20f; // Force applied to the spear
    public float attackRange = 30f; // Range within which the centaur will attack
    public float stopDistance = 5f;
    public float chaseSpeed = 3f; // Speed at which the centaur chases the player
    public float attackCooldown = 2f; // Time between attacks
    public float spearSpawnHeightOffset = 3.5f; // Height above the centaur to spawn the spear
    private PlayerMovement playerMovement;
    public AudioClip throwSound;

    private AudioSource audioSource;
    private float lastAttackTime;
    private bool isChasing = false;

    void Start()
    {
        // Add an AudioSource component if not already present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference not assigned.");
            return;
        }


        // Check if the player is within attack range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange )
        {
            if (distanceToPlayer > stopDistance)
            {
                isChasing = true;
                ChasePlayer();
            }

            // Check if the attack cooldown has elapsed
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                ThrowSpear();
                lastAttackTime = Time.time; // Reset cooldown timer
            }
        }
        else
        {
            isChasing = false;
        }
    }

    private void ChasePlayer()
    {
    if (player == null) return;

    // Move toward the player
    Vector3 directionToPlayer = (player.position - transform.position).normalized;
    Vector3 newPosition = transform.position + directionToPlayer * chaseSpeed * Time.deltaTime;

    // Raycast to adjust NPC height based on the terrain
    if (Physics.Raycast(new Vector3(newPosition.x, newPosition.y + 1f, newPosition.z), Vector3.down, out RaycastHit hit, Mathf.Infinity))
    {
        newPosition.y = hit.point.y; // Adjust the height to match the terrain
    }

    transform.position = newPosition;

    // Face the player
    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * chaseSpeed);
    }


    private void ThrowSpear()
    {
        if (spearPrefab == null)
        {
            Debug.LogError("Spear prefab not assigned.");
            return;
        }

        // Calculate spear spawn position above the centaur
        Vector3 spearSpawnPosition = transform.position + Vector3.up * spearSpawnHeightOffset;

        // Instantiate the spear
        GameObject spear = Instantiate(spearPrefab, spearSpawnPosition, transform.rotation);

        // Orient the spear towards the player
        Vector3 directionToPlayer = (player.position - spearSpawnPosition).normalized;
        spear.transform.rotation = Quaternion.LookRotation(directionToPlayer);
        spear.transform.localEulerAngles = new Vector3(spear.transform.localEulerAngles.x, spear.transform.localEulerAngles.y+90, spear.transform.localEulerAngles.z+100);


        // Apply force to the spear using Rigidbody
        Rigidbody spearRigidbody = spear.GetComponent<Rigidbody>();
        if (spearRigidbody != null)
        {
            spearRigidbody.AddForce(directionToPlayer * throwForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogError("Spear prefab does not have a Rigidbody component.");
        }
        SpearCollisionHandler collisionHandler = spear.AddComponent<SpearCollisionHandler>();
        collisionHandler.damage = 10;

        // Play the throw sound
        if (throwSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(throwSound);
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize the attack range in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Visualize spear spawn position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * spearSpawnHeightOffset, 0.1f);
    }

}

public class SpearCollisionHandler : MonoBehaviour
{
    public int damage = 10; // Amount of damage the spear deals\

    private void OnCollisionEnter(Collision collision)
    {

        // TODO add logic to remove some of player's health on collision

        // Check if the object hit is the player
        PlayerMovement playerMovement = collision.gameObject.GetComponentInParent<PlayerMovement>();
        if (playerMovement != null)
        {
            print("Damaged player");
            playerMovement.DamagePlayer(damage);
            Destroy(gameObject); // Destroy the spear after hitting the player
        }
        else
        {
            // Destroy the spear if it hits anything else
            Destroy(gameObject);
            print("No player hit");
        }
    }
}