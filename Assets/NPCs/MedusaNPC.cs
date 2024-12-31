using UnityEngine;
using TMPro;

public class MedusaNPC : MonoBehaviour
{
    public Transform player;
    public float freezeDistance = 40f; // Maximum distance for freezing
    public LayerMask obstacleMask; // Mask for obstacles that could block the view
    public float headHeightOffset = 1.15f; // Adjust this value to place the ray at Medusa's head level
    public AudioClip throwSound;

    private AudioSource audioSource;

    private bool isPlayerFrozen = false;
    public TMP_Text infoText;      // TMP_Text to display general info/warnings in UI

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

        // Calculate direction to player
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Check if player is within freeze distance
        if (distanceToPlayer <= freezeDistance)
        {
            // Normalize the direction
            Vector3 normalizedDirection = directionToPlayer.normalized;

            // Check if player is looking at Medusa
            if (IsLookingAtMedusa(player, normalizedDirection) && IsLineOfSightClear(normalizedDirection, distanceToPlayer))
            {
                FreezePlayer();
            }
            else
            {
                UnfreezePlayer();
            }
        }
        else
        {
            UnfreezePlayer();
        }
    }

    private bool IsLookingAtMedusa(Transform player, Vector3 directionToMedusa)
    {
        // Check the player's forward direction
        float dotProduct = -Vector3.Dot(player.forward, directionToMedusa.normalized);
        print(dotProduct);

        // Assuming a threshold for "looking at" (you can adjust 0.5 to tweak sensitivity)
        return dotProduct > 0.5f;
    }

    private bool IsLineOfSightClear(Vector3 directionToPlayer, float distanceToPlayer)
    {
        // Raycast to check if there's a clear line of sight
        Vector3 rayOrigin = transform.position + Vector3.up * headHeightOffset;
        Debug.DrawRay(rayOrigin, directionToPlayer * distanceToPlayer, Color.red);
        return !Physics.Raycast(rayOrigin, directionToPlayer, distanceToPlayer, obstacleMask);
    }

    private void FreezePlayer()
    {
        if (!isPlayerFrozen)
        {
            Debug.Log("Player is frozen!");
            isPlayerFrozen = true;
            infoText.text = "You have been frozen by Medusa!" + "\n" + "Look away to break free!";
            player.GetComponent<CharacterController>().enabled = false;
            // Play the throw sound
            if (throwSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(throwSound);
            }
        }
        
    }

    private void UnfreezePlayer()
    {
        if (isPlayerFrozen)
        {
            Debug.Log("Player is unfrozen!");
            isPlayerFrozen = false;
            infoText.text = "";
            player.GetComponent<CharacterController>().enabled = true;
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize freeze distance in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, freezeDistance);
    }
}