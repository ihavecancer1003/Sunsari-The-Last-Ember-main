using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat & Parry")]
    public Transform attackPoint;
    public float attackRange = 0.8f; // Бага зэрэг томрууллаа
    public LayerMask enemyLayers;
    public int attackDamage = 20;
    public float parryWindow = 0.2f;
    private bool isParryActive;

    [Header("Parry Stack System")]
    public int parryStack = 0;
    public int maxParryStack = 5;
    public int superDamage = 300;

    [Header("Health & UI")]
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthSlider;
    public float iFrameDuration = 1f; // Цохиулсны дараах хамгаалалтын хугацаа
    private bool isInvincible = false;

    [Header("Input Buffering")]
    private float attackBufferTimer;

    private PlayerMovement movement;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    void Update()
    {
        attackBufferTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.X)) 
        {
            attackBufferTimer = 0.12f;
        }

        if (attackBufferTimer > 0)
        {
            // DASH-ATTACK: movement.isDashing-ийг эндээс хассан тул Dash хийхдээ цохиж болно
            if (movement != null)
            {
                PerformAttack();
                attackBufferTimer = 0;
            }
        }
    }

    public void PerformAttack()
    {
        int finalDamage = (parryStack >= maxParryStack) ? superDamage : attackDamage;
        if (parryStack >= maxParryStack)
        {
            parryStack = 0;
            Debug.Log("ULTIMATE ATTACK!");
        }

        StartCoroutine(ActivateParry());

        // Радиус доторх бүх Collider-ийг олно
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        
        foreach (Collider2D obj in hitObjects)
        {
            // Зөвхөн "Enemy" Tag-тай бөгөөд IDamageable интерфейстэй бол цохино
            if (obj.CompareTag("Enemy"))
            {
                IDamageable damageable = obj.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(finalDamage);
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // Хамгаалалттай үед (I-Frame) эсвэл Dash хийж байхдаа damage авахгүй
        if (isInvincible || movement.isDashing || isParryActive)
        {
            if (isParryActive)
            {
                parryStack++;
                Debug.Log("Parry Success! Stack: " + parryStack);
            }
            return;
        }

        currentHealth -= damage;
        if (healthSlider != null) healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(BecomeInvincible());
            ApplyKnockback();
        }
    }

    private IEnumerator BecomeInvincible()
    {
        isInvincible = true;
        // Энд чи SpriteRenderer ашиглан Vesper-ийг анивчдаг болгож болно
        yield return new WaitForSeconds(iFrameDuration);
        isInvincible = false;
    }

    private void ApplyKnockback()
    {
        // Цохиулсан зүг рүү бага зэрэг шидэгдэх
        Vector2 knockbackDir = new Vector2(-transform.localScale.x, 0.5f).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDir * 7f, ForceMode2D.Impulse);
    }

    IEnumerator ActivateParry()
    {
        isParryActive = true;
        yield return new WaitForSeconds(parryWindow);
        isParryActive = false;
    }

    void Die()
    {
        Debug.Log("Vesper үхлээ...");
        // Хөдөлгөөн зогсоож, объектыг идэвхгүй болгох
        if (movement != null) movement.enabled = false;
        gameObject.SetActive(false); 
    }

    // Scene цонхонд Attack Range-ийг харах (Зөвхөн хөгжүүлэлтийн үед харагдана)
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}