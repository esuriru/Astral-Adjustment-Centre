// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour, IDamageable
{
    // NOTE - Remove unused code
    // [SerializeField] Animator animator;
    private float health = 100;
    private float immunityTimer = 0;
    private bool isDead;

    private void Update()
    {
        if (immunityTimer > 0 && !isDead)
        {
            immunityTimer -= Time.deltaTime;
        }
    }

    public void Damage(float damage)
    {
        if (immunityTimer <= 0)
        {
            health -= damage;
            Debug.Log("Hit");
            // NOTE - Remove unused code
            // animator.SetTrigger("hit");

            if (health <= 0)
            {
                isDead = true;
                // animator.SetBool("isDead", isDead);

                Despawn();
            }
            immunityTimer = 0.5f;

            // Debug.Log(health);
        }
    }

    public void Despawn()
    {
        // Return to pool
        // NOTE - Superfluous this
        StartCoroutine(ObjectPoolManager.Instance.ReturnObjectToPool(this.gameObject));
    }
}
