using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour, IDamageable
{
    [Header("Movement Stats")]
    public Transform player;
    public float chaseSpeed = 6f;       // Илүү хурдан хөөх
    public float diveSpeed = 20f;        // Маш хурдтай шумбах
    public float detectionRange = 20f;
    public float attackRange = 6.5f;
    public bool canAttack = true;

    [Header("Hover Settings")]
    public float hoverHeight = 3f;
    public float hoverForce = 60f;
    public float horizontalOffset = 4f;
    public LayerMask groundLayer;

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
        if (isDiving || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < attackRange)
        {
            StartCoroutine(AggressiveDive());
        }
        else if (distance < detectionRange)
        {
            ChasePlayer();
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

    IEnumerator AggressiveDive()
    {
        isDiving = true;
        if (!canAttack)
        {
            if (transform.position.x <= player.position.x + (side * horizontalOffset))
            {
                canAttack = true;
            }
        }
        else
        {
            isDiving = true;
            rb.linearVelocity = Vector2.zero;

            // Дайрахын өмнөх цэнэглэлт (Зөвхөн 0.5 секунд хүлээгээд шууд дайрна)
            yield return new WaitForSeconds(0.5f);

            if (player != null)
            {
                Vector2 diveDir = (player.position - transform.position).normalized;
                rb.linearVelocity = diveDir * diveSpeed;
            }

            yield return new WaitForSeconds(0.8f);

            yield return new WaitForSeconds(1.2f); // Дайрсны дараах амралт
            isDiving = false;
            canAttack = false;
        }
            ;

        
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