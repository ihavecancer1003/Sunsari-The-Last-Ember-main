using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 100;
    public int damage = 10;

    public void TakeDamage(int amount) {
        health -= amount;
        Debug.Log("Дайсан " + amount + " хохирол авлаа. Үлдсэн амь: " + health);
        if (health <= 0) Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponent<PlayerCombat>().TakeDamage(damage);
        }
    }
}