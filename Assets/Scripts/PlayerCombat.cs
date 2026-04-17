using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat & Parry")]
    public Transform attackPoint;
    public float attackRange = 0.6f;
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

    [Header("Input Buffering")] //input buffering hiisen
    private float attackBufferTimer;

    private PlayerMovement movement;

    void Start()
    {
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
        attackBufferTimer -= Time.deltaTime; // buffer timer dooshilno

        if (Input.GetKeyDown(KeyCode.X)) // attack input avna
        {
            attackBufferTimer = 0.12f;
        }

        // attack hiij boloh eshiig shalgana
        if (attackBufferTimer > 0)
        {
            // ! eniig oorchilj bolno. dashlaj bhdaa attack hiij boldog bolgoj bolno
            if (movement != null && !movement.isDashing)
            {
                PerformAttack();
                attackBufferTimer = 0; // buffer hiisnii daraa arilgana
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
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>()?.TakeDamage(finalDamage);
        }
    }
    public void TakeDamage(int damage)
    {
        if (movement.isDashing || isParryActive)
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
        if (currentHealth <= 0) Die();
    }
    IEnumerator ActivateParry()
    {
        isParryActive = true;
        yield return new WaitForSeconds(parryWindow);
        isParryActive = false;
    }
    void Die() => gameObject.SetActive(false);
}
