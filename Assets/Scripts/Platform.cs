using UnityEngine;

public class Platform : MonoBehaviour
{
    private Collider2D platCollider;
    private GameObject player;
    private Rigidbody2D playerRb;
    private Collider2D playerCollider;

    void Start()
    {
        platCollider = GetComponent<Collider2D>();
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
            playerCollider = player.GetComponent<Collider2D>();
        }
    }

    void Update()
    {
        if (player == null) return;

        // Get the bottom of player and top of platform
        float playerBottom = playerCollider.bounds.min.y;
        float platformTop = platCollider.bounds.max.y;

        // TERRARIA LOGIC:
        // Pass through if:
        // 1. Moving Upwards (Jumping through from bottom)
        // 2. Player's feet are still below the platform surface (Passing through sides)
        // 3. Holding "S" is handled by your PlayerMovement script's IgnoreCollision

        if (playerRb.linearVelocity.y > 0.1f || playerBottom < platformTop - 0.1f)
        {
            platCollider.isTrigger = true;
        }
        else
        {
            platCollider.isTrigger = false;
        }
    }
}