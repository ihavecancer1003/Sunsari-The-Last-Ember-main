using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour, IDamageable
{
    [Header("Movement Stats")]
    public Transform player;
    public float chaseSpeed = 8f;       // Илүү хурдан хөөх
    public float diveSpeed = 35f;        // Маш хурдтай шумбах
    public float attackRange = 6.5f;
    public bool canAttack = true;

    [Header("Hover Settings")]
    public float hoverHeight = 3f;
    public float hoverForce = 60f;
    public float horizontalOffset = 4f;
    public LayerMask groundLayer;

    public enum EnemyState { Idle, Hovering, Diving, Resting }
    public EnemyState currentState = EnemyState.Idle;

    private Rigidbody2D rb;
    private bool isDiving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 0.5f;

        if (player == null) player = GameObject.Find("Vesper")?.transform;
    }

    void Update()
    {
        // If we are diving or resting, don't run the Chase/Hover logic
        if (currentState == EnemyState.Diving || currentState == EnemyState.Resting)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < 18f) // Within range to hover
        {
            currentState = EnemyState.Hovering;
            ChasePlayer(); // This moves it to the target offset (above/side)

            // Only start the dive if we are close enough to the hover position
            if (distanceToPlayer < 6.5f && canAttack)
            {
                StartCoroutine(PerformDive());
            }
        }
    }

    void FixedUpdate()
    {
        // Газар мэдэрч хөвөх
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, hoverHeight, groundLayer);
        if (hit.collider != null && !isDiving)
        {
            float lift = (hoverHeight - hit.distance) / hoverHeight;
            rb.AddForce(Vector2.up * lift * hoverForce, ForceMode2D.Force);
            rb.linearDamping = 3f;
        }
        else
        {
            rb.linearDamping = 0.8f;
        }
    }

    void ChasePlayer()
    {
        // vesperiin zuun tald bgaa eshiig shalgana
        float side;
        if (transform.position.x < player.position.x)
        {
            side = -1f;
        }
        else
        {
            side = 1f;
        }
        Vector2 targetPos = new Vector2(player.position.x + (side * horizontalOffset), player.position.y + 5f);
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;

        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, dir * chaseSpeed, Time.deltaTime * 5f);

        // Эсрэг тал руу харуулах
        if (transform.position.x < player.position.x)
            transform.localScale = new Vector3(-1, 1, 1); // Баруун тийш харуулах
        else
            transform.localScale = new Vector3(1, 1, 1);  // Зүүн тийш харуулах
    }

    IEnumerator PerformDive()
    {
        float side;
        if (transform.position.x < player.position.x)
        {
            side = 2f;
        }
        else
        {
            side = -2f;
        }
        canAttack = false;
        isDiving = true;
        currentState = EnemyState.Diving;

        // 1. Charge up: Stop for a moment to signal the player
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);

        // 2. The Attack
        if (player != null)
        {
            Vector2 targetPos = new Vector2(player.position.x + (side * 1f), player.position.y);
            Vector2 diveDir = (targetPos - (Vector2)transform.position).normalized;
            rb.linearVelocity = diveDir * diveSpeed;
        }

        // 3. Duration of the dive
        yield return new WaitForSeconds(1.0f);

        // 4. Recovery: Stop and wait before chasing again
        currentState = EnemyState.Resting;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);

        // 5. Reset: Allow chasing/hovering again
        isDiving = false;
        currentState = EnemyState.Idle;

        // Simple cooldown before next possible attack
        yield return new WaitForSeconds(2.0f);
        canAttack = true;
    }

    // --- IDamageable INTERFACE ---
    public void TakeDamage(int damage)
    {
        // 1 цохилтоор үхэх логик:
        Debug.Log("Мангас нэг цохилтоор устлаа!");
        Die();
    }

    void Die()
    {
        // Энд чи дараа нь өөрийн үхэх анимаци эсвэл эффектээ дуудаж болно
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Vesper-т 15 damage өгнө
            collision.gameObject.GetComponent<PlayerCombat>()?.TakeDamage(15);
        }
    }
}